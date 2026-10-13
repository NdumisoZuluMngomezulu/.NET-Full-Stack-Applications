using MovieTracker.Api.Models;

namespace MovieTracker.Api.Data;

public static class SeedData
{
    //adds a few sample movies for the first time the database is created
    public static void Seed(AppDbContext db)
    {
        if (db.Movies.Any()) return;

        db.Movies.AddRange(
            new Movie
            {
                Title = "Parasite",
                ReleaseDate = new DateOnly(2019, 5, 30),
                Genres = "Thriller,Drama",
                Country = "South Korea",
                Director = "Bong Joon-ho",
                Synopsis = "A poor family schemes its way into the lives of a wealthy household.",
                Viewings =
                [
                    new Viewing { WatchedOn = new DateOnly(2020, 2, 14), Rating = 10, Notes = "Saw it in the cinema." },
                    new Viewing { WatchedOn = new DateOnly(2024, 5, 3), Rating = 9, Notes = "Rewatch: noticed far more of the staircase imagery." }
                ]
            },
            new Movie
            {
                Title = "Spirited Away",
                ReleaseDate = new DateOnly(2001, 7, 20),
                Genres = "Animation,Fantasy",
                Country = "Japan",
                Director = "Hayao Miyazaki",
                Synopsis = "A girl wanders into a world of spirits and must work to free her parents.",
                Viewings = [new Viewing { WatchedOn = new DateOnly(2023, 8, 20), Rating = 9 }]
            },
            new Movie
            {
                Title = "Mad Max: Fury Road",
                ReleaseDate = new DateOnly(2015, 5, 15),
                Genres = "Action,Sci-Fi",
                Country = "Australia",
                Director = "George Miller"
            },
            new Movie
            {
                Title = "City of God",
                ReleaseDate = new DateOnly(2002, 8, 30),
                Genres = "Crime,Drama",
                Country = "Brazil",
                Director = "Fernando Meirelles"
            });

        db.SaveChanges();
    }
}
/*
                ReleaseDate = new DateOnly(2019, 5, 30),
                

*/