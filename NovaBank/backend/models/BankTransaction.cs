using System.ComponentModel.DataAnnotations;

namespace NovaBank.models;

public class BankTransaction
{
    public long Id { get; set; }
    [Required, MaxLength(40)] public string Reference { get; set; }

    public int FromAccountId { get; set; }

    public int ToAccountId { get; set; }

    public decimal Account { get; set; }

    [MaxLength(3)] public string Currency { get; set; } = "ZAR";

    [MaxLength(20)] public string Status { get; set; } = "ZAR";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
}

// using System.ComponentModel.DataAnnotations;
// namespace NovaBank.Models;
// public class BankTransaction {
//     public long Id { get; set; }
//     [Required, MaxLength(40)] public string Reference { get; set; } = Guid.NewGuid().ToString("N");
//     public int FromAccountId { get; set; }
//     public int ToAccountId { get; set; }
//     public decimal Amount { get; set; }
//     [MaxLength(3)] public string Currency { get; set; } = "ZAR";
//     [MaxLength(20)] public string Status { get; set; } = "Completed";
//     public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
// }
