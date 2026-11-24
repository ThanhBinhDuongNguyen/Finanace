using Finance.Domain.Imports;
using Finance.Domain.Exceptions;
using Finance.Domain.Reconciliation;
using Finance.Domain.Ledger;
using Finance.Domain.Classification;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
{
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportLine> ImportLines => Set<ImportLine>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<ReconciliationCase> ReconciliationCases => Set<ReconciliationCase>();
    public DbSet<FinanceException> FinanceExceptions => Set<FinanceException>();
    public DbSet<ExceptionActivity> ExceptionActivities => Set<ExceptionActivity>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<ClosedPeriod> ClosedPeriods => Set<ClosedPeriod>();
    public DbSet<ClassificationMapping> ClassificationMappings => Set<ClassificationMapping>();
    public DbSet<ClassifiedTransaction> ClassifiedTransactions => Set<ClassifiedTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportBatch>(entity =>
        {
            entity.HasKey(batch => batch.Id);
            entity.HasIndex(batch => batch.Checksum).IsUnique();
            entity.Property(batch => batch.Period).HasConversion<string>();
            entity.Property(batch => batch.SourceType).HasConversion<string>();
            entity.Property(batch => batch.Status).HasConversion<string>();
            entity.HasMany(batch => batch.Lines).WithOne().HasForeignKey(line => line.ImportBatchId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ImportLine>(entity =>
        {
            entity.HasKey(line => line.Id);
            entity.Property(line => line.OccurredOn).HasConversion<string>();
            entity.Property(line => line.Status).HasConversion<string>();
        });
        modelBuilder.Entity<Payout>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.HasIndex(value => new { value.ExternalId, value.ShopCode }).IsUnique();
            entity.Property(value => value.PayoutOn).HasConversion<string>();
        });
        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.HasIndex(value => new { value.ExternalId, value.ShopCode }).IsUnique();
            entity.Property(value => value.CreditedOn).HasConversion<string>();
        });
        modelBuilder.Entity<ReconciliationCase>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Status).HasConversion<string>();
            entity.HasIndex(value => new { value.PayoutId, value.BankTransactionId }).IsUnique();
        });
        modelBuilder.Entity<FinanceException>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Status).HasConversion<string>();
            entity.Property(value => value.DueOn).HasConversion<string>();
            entity.HasMany(value => value.Activities).WithOne().HasForeignKey(value => value.FinanceExceptionId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ExceptionActivity>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).ValueGeneratedNever();
        });
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Status).HasConversion<string>();
            entity.Property(value => value.Period).HasConversion<string>();
            entity.HasMany(value => value.Lines).WithOne().HasForeignKey(value => value.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<JournalLine>(entity => entity.HasKey(value => value.Id));
        modelBuilder.Entity<ClosedPeriod>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Period).HasConversion<string>();
            entity.HasIndex(value => new { value.ShopCode, value.Period }).IsUnique();
        });
        modelBuilder.Entity<ClassificationMapping>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.Property(value => value.SourceType).HasConversion<string>();
            entity.Property(value => value.EffectiveFrom).HasConversion<string>();
            entity.Property(value => value.EffectiveTo).HasConversion<string>();
        });
        modelBuilder.Entity<ClassifiedTransaction>(entity =>
        {
            entity.HasKey(value => value.Id);
            entity.HasIndex(value => value.ImportLineId).IsUnique();
            entity.Property(value => value.Status).HasConversion<string>();
        });
    }
}
