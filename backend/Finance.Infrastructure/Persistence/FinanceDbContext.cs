using Finance.Domain.Imports;
using Finance.Domain.Exceptions;
using Finance.Domain.Reconciliation;
using Finance.Domain.Ledger;
using Finance.Domain.Classification;
using Microsoft.EntityFrameworkCore;
using Finance.Application.Security;
using Finance.Infrastructure.Extensions;

namespace Finance.Infrastructure.Persistence;

public sealed class FinanceDbContext(DbContextOptions<FinanceDbContext> options, ICurrentActor actor) : DbContext(options)
{
    public IReadOnlyList<string> AllowedShops => actor.Shops;
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
        FinanceExtensionService.ConfigureModel(modelBuilder);
        FinanceExtensionService.ConfigureScope(modelBuilder, this);
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
        modelBuilder.Entity<ImportBatch>().HasQueryFilter(value => AllowedShops.Contains(value.ShopCode));
        modelBuilder.Entity<ImportLine>().HasQueryFilter(value => ImportBatches.Any(batch => batch.Id == value.ImportBatchId));
        modelBuilder.Entity<Payout>().HasQueryFilter(value => AllowedShops.Contains(value.ShopCode));
        modelBuilder.Entity<BankTransaction>().HasQueryFilter(value => AllowedShops.Contains(value.ShopCode));
        modelBuilder.Entity<ReconciliationCase>().HasQueryFilter(value => Payouts.Any(payout => payout.Id == value.PayoutId));
        modelBuilder.Entity<FinanceException>().HasQueryFilter(value => AllowedShops.Contains(value.ShopCode));
        modelBuilder.Entity<ExceptionActivity>().HasQueryFilter(value => FinanceExceptions.Any(item => item.Id == value.FinanceExceptionId));
        modelBuilder.Entity<JournalEntry>().HasQueryFilter(value => AllowedShops.Contains(value.ShopCode));
        modelBuilder.Entity<JournalLine>().HasQueryFilter(value => JournalEntries.Any(journal => journal.Id == value.JournalEntryId));
        modelBuilder.Entity<ClosedPeriod>().HasQueryFilter(value => AllowedShops.Contains(value.ShopCode));
        modelBuilder.Entity<ClassifiedTransaction>().HasQueryFilter(value => ImportBatches.Any(batch => batch.Id == value.ImportBatchId));
        modelBuilder.Entity<JournalEntry>().Property(value => value.Status).IsConcurrencyToken();
        modelBuilder.Entity<FinanceException>().Property(value => value.Status).IsConcurrencyToken();
        modelBuilder.Entity<ReconciliationCase>().Property(value => value.Status).IsConcurrencyToken();
        modelBuilder.Entity<ReconciliationCase>().HasMany(v => v.StatementAllocations).WithOne().HasForeignKey(v => v.ReconciliationCaseId);
        modelBuilder.Entity<ReconciliationCase>().HasMany(v => v.BankAllocations).WithOne().HasForeignKey(v => v.ReconciliationCaseId);
        modelBuilder.Entity<StatementAllocation>().Property(v => v.Id).ValueGeneratedNever();
        modelBuilder.Entity<BankAllocation>().Property(v => v.Id).ValueGeneratedNever();
        modelBuilder.Entity<StatementAllocation>().HasOne<ImportLine>().WithMany().HasForeignKey(v => v.ImportLineId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<StatementAllocation>().HasOne<Finance.Domain.Extensions.FinanceOrderLine>().WithMany().HasForeignKey(v => v.OrderLineId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<BankAllocation>().HasOne<BankTransaction>().WithMany().HasForeignKey(v => v.BankTransactionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<StatementAllocation>().HasQueryFilter(v => ReconciliationCases.Any(c => c.Id == v.ReconciliationCaseId));
        modelBuilder.Entity<BankAllocation>().HasQueryFilter(v => ReconciliationCases.Any(c => c.Id == v.ReconciliationCaseId));
        modelBuilder.Entity<TolerancePolicy>().Property(v => v.Currency).IsRequired();
        modelBuilder.Entity<ReconciliationCase>().HasIndex(v => v.PayoutId).IsUnique();
        modelBuilder.Entity<JournalEntry>().HasIndex(value => new { value.ShopCode, value.ReferenceType, value.ReferenceId }).IsUnique();
        modelBuilder.Entity<ImportLine>().HasIndex(value => value.SourceKey).IsUnique();
        modelBuilder.Entity<PeriodReopenRequest>().HasQueryFilter(v => AllowedShops.Contains(v.ShopCode));
        modelBuilder.Entity<PeriodReopenRequest>().Property(v => v.ApprovedBy).IsConcurrencyToken();
        modelBuilder.Entity<SourceAuthority>().HasIndex(v => v.ShopCode).IsUnique();
        modelBuilder.Entity<SourceAuthority>().Property(v => v.Version).IsConcurrencyToken();
        modelBuilder.Entity<SourceAuthority>().HasQueryFilter(v => AllowedShops.Contains(v.ShopCode));
        modelBuilder.Entity<SourceSwitchRequest>().HasQueryFilter(v => AllowedShops.Contains(v.ShopCode));
        modelBuilder.Entity<SourceSwitchRequest>().Property(v => v.ApprovedBy).IsConcurrencyToken();
    }
}
