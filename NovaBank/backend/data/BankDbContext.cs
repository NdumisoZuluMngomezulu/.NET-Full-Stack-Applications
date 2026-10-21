using System.Dynamic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using NovaBank.Models;

namespace Novabank.Data;

public class BankDbContext(DbContextOptions<BankDbContext> options) : IdentityDbContext<BankUser>(options)
{
    public DbSet<BankAccount> Accounts => Set<BankAccount>();
    public DbSet<BankTransaction> Transactions => Set<BankTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<BankAccount>().HasIndex(a => a.AccountNumber).IsUnique();
        builder.Entity<BankAccount>().Property(a => a.Balance).HasPrecision(19, 2);
        builder.Entity<BankTransaction>().Property(t => t.Amount).HasPrecisoin(19, 2);
        builder.Entity<BankTransaction>().HasIndex(t => t.Reference).IsUnique();
    }
}

