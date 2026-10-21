using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=tickets.db";

// Opens a new connection to the database file. Callers dispose it with "using".
SqliteConnection OpenConnection()
{
    var conn = new SqliteConnection(connectionString);
    conn.Open();
    return conn;
}

// ---------------------------------------------------------------- database setup
// Create the table on first run, and add two sample rows if it is empty.
using (var conn = OpenConnection())
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS Tickets (
            Id      INTEGER PRIMARY KEY AUTOINCREMENT,
            Title   TEXT    NOT NULL,
            Year    INTEGER,
            Assignee   TEXT,
            Cleared INTEGER NOT NULL DEFAULT 0,   -- 0 = not cleared, 1 = cleared
            Rating  INTEGER                       -- 1 to 5, or NULL if not rated
        );
        """;
    cmd.ExecuteNonQuery();

    cmd.CommandText = "SELECT COUNT(*) FROM Tickets";
    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
    {
        cmd.CommandText = """
            INSERT INTO Tickets (Title, Year, Assignee)
            VALUES ('Broken screen', 2031, 'Ndumiso'),
                   ('WiFi connection down', 2027, 'Simphiwe');
            """;
        cmd.ExecuteNonQuery();
    }
}

// ---------------------------------------------------------------- serve the web page
// index.html, style.css and app.js live in the wwwroot folder.
app.UseDefaultFiles();
app.UseStaticFiles();

// ---------------------------------------------------------------- API

// GET /api/Tickets : list every Ticket, newest first
app.MapGet("/api/Tickets", () =>
{
    var Tickets = new List<Ticket>();

    using var conn = OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Id, Title, Year, Assignee, Cleared, Rating FROM Tickets ORDER BY Id DESC";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        Tickets.Add(new Ticket(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetInt32(4) == 1,
            reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)));
    }

    return Results.Ok(Tickets);
});

// POST /api/Tickets : add a Ticket
app.MapPost("/api/Tickets", (NewTicket input) =>
{
    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest(new { error = "Title is required." });
    if (input.Year is < 1888 or > 2100)
        return Results.BadRequest(new { error = "Year must be between 1888 and 2100." });

    using var conn = OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        INSERT INTO Tickets (Title, Year, Assignee)
        VALUES ($title, $year, $Assignee);
        SELECT last_insert_rowid();
        """;
    // Always pass user input as parameters (never glue it into the SQL text).
    cmd.Parameters.AddWithValue("$title", input.Title.Trim());
    cmd.Parameters.AddWithValue("$year", (object?)input.Year ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$Assignee",
        string.IsNullOrWhiteSpace(input.Assignee) ? DBNull.Value : input.Assignee.Trim());

    var id = Convert.ToInt32(cmd.ExecuteScalar());
    return Results.Created($"/api/Tickets/{id}", new { id });
});

// PUT /api/Tickets/5/cleared : tick a Ticket off (or un-tick it) and set its rating
app.MapPut("/api/Tickets/{id:int}/cleared", (int id, ClearedUpdate input) =>
{
    if (input.Rating is < 1 or > 5)
        return Results.BadRequest(new { error = "Rating must be between 1 and 5." });

    // An un-cleared Ticket can't have a rating.
    int? rating = input.Cleared ? input.Rating : null;

    using var conn = OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "UPDATE Tickets SET Cleared = $cleared, Rating = $rating WHERE Id = $id";
    cmd.Parameters.AddWithValue("$cleared", input.Cleared ? 1 : 0);
    cmd.Parameters.AddWithValue("$rating", (object?)rating ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$id", id);

    return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
});

// DELETE /api/Tickets/5 : remove a Ticket
app.MapDelete("/api/Tickets/{id:int}", (int id) =>
{
    using var conn = OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "DELETE FROM Tickets WHERE Id = $id";
    cmd.Parameters.AddWithValue("$id", id);

    return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
});

app.Run();

// ---------------------------------------------------------------- data shapes (JSON in and out)
// Type declarations must come after the top-level statements above.
record Ticket(int Id, string Title, int? Year, string? Assignee, bool Cleared, int? Rating);
record NewTicket(string? Title, int? Year, string? Assignee);
record ClearedUpdate(bool Cleared, int? Rating);
