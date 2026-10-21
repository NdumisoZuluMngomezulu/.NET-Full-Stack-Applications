using Microsoft.AspNetCore.Identity;

namespace NovaBank.models;

public class Bank : IdentityUser
{
    public string FullName { get; set; } = "";
}

// using Microsoft.AspNetCore.Identity;
// namespace NovaBank.Models;
// public class BankUser : IdentityUser { public string FullName { get; set; } = ""; }
