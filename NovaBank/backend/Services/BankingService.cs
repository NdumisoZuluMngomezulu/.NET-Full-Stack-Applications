using System.Data;
using Microsoft.EntityFrameworkCore;
using NovaBank.Data;
using NovaBank.DTOs;
using NovaBank.Models;
namespace NovaBank.Services;
public record TransferResult(bool Success, string Message, string? Reference = null);
public class BankingService(BankDbContext db) {
    public async Task<object> GetMyAccounts(string userId) => await db.Accounts.AsNoTracking()
        .Where(a => a.UserId == userId).Select(a => new { a.AccountNumber, a.Balance, a.Currency, a.CreatedAtUtc }).ToListAsync();
    public async Task<object> GetMyTransactions(string userId) {
        var ids = db.Accounts.Where(a => a.UserId == userId).Select(a => a.Id);
        return await db.Transactions.AsNoTracking().Where(t => ids.Contains(t.FromAccountId) || ids.Contains(t.ToAccountId))
            .OrderByDescending(t => t.CreatedAtUtc).Take(100)
            .Select(t => new { t.Reference, t.FromAccountId, t.ToAccountId, t.Amount, t.Currency, t.Status, t.CreatedAtUtc }).ToListAsync();
    }
    public async Task<TransferResult> Transfer(string userId, TransferRequest request) {
        if (request.Amount <= 0 || request.Amount > 1_000_000) return new(false, "Enter an amount greater than zero and no more than R1,000,000.");
        if (string.IsNullOrWhiteSpace(request.RecipientAccountNumber)) return new(false, "Recipient account number is required.");
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try {
            var sender = await db.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            var recipient = await db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == request.RecipientAccountNumber.Trim());
            if (sender is null) return new(false, "Your account could not be found.");
            if (recipient is null) return new(false, "Recipient account could not be found.");
            if (sender.Id == recipient.Id) return new(false, "You cannot transfer money to the same account.");
            if (sender.Currency != recipient.Currency) return new(false, "Transfers between different currencies are not supported.");
            if (sender.Balance < request.Amount) return new(false, "Insufficient funds.");
            sender.Balance -= request.Amount;
            recipient.Balance += request.Amount;
            var reference = $"NB{Guid.NewGuid():N}"[..18].ToUpperInvariant();
            db.Transactions.Add(new BankTransaction { Reference = reference, FromAccountId = sender.Id, ToAccountId = recipient.Id, Amount = request.Amount, Currency = sender.Currency, Status = "Completed" });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new(true, "Transfer completed.", reference);
        } catch (DbUpdateException) {
            await transaction.RollbackAsync();
            return new(false, "The transfer could not be completed. Please try again.");
        } catch {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
