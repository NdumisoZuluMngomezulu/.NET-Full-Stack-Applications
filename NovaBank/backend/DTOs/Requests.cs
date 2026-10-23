namespace NovaBank.DTOs;
public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record TransferRequest(string RecipientAccountNumber, decimal Amount, string? Description);
