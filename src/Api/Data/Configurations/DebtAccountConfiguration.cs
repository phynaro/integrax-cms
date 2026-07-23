using Accounts.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class DebtAccountConfiguration : IEntityTypeConfiguration<DebtAccount>
{
    public void Configure(EntityTypeBuilder<DebtAccount> builder)
    {
        builder.ToTable("debt_accounts");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ExternalId).HasMaxLength(255);
        builder.Property(x => x.AccountNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreditorReference).HasMaxLength(255);
        builder.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrentBalance).HasPrecision(18, 2);
        builder.Property(x => x.InterestAmount).HasPrecision(18, 2);
        builder.Property(x => x.FeesAmount).HasPrecision(18, 2);
        builder.Property(x => x.LastPaymentAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        
        builder.HasIndex(x => x.AccountNumber).IsUnique();
        builder.HasIndex(x => x.DebtorId);
        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => x.ImportBatchId);
        builder.HasIndex(x => x.Status);
        
        builder.HasOne(x => x.Debtor)
            .WithMany()
            .HasForeignKey(x => x.DebtorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.Portfolio)
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
