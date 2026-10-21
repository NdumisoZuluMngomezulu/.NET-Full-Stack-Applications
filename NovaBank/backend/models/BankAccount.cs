using System.ComponentModel.DataAnnotations;

namespace Novabank.Models;

public class BankAccount
{
    public int Id { get; set; }
    
    [Required] public string UserId { get; set; } = ""; 
    [Required, MaxLength(20)] public string AccountNumber { get; set; } = "";
    [Range(0, 999999999999)] public decimal Balance { get; set; }

    [MaxLength(3)] public string Currency { get; set; } = "ZAR";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

// using System.ComponentModel.DataAnnotations;
// namespace NovaBank.Models;
// public class BankAccount {
//     public int Id { get; set; }
//     [Required] public string UserId { get; set; } = "";
//     [Required, MaxLength(20)] public string AccountNumber { get; set; } = "";
//     [Range(0, 999999999999)] public decimal Balance { get; set; }
//     [MaxLength(3)] public string Currency { get; set; } = "ZAR";
//     public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
// }
