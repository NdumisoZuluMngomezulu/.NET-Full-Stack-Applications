using Microsoft.AspNetCore.Identity;
namespace NovaBank.Models;
public class BankUser : IdentityUser { public string FullName { get; set; } = ""; }
