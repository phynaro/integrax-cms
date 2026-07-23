using Debtors.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class DebtorConfiguration : IEntityTypeConfiguration<Debtor>
{
    public void Configure(EntityTypeBuilder<Debtor> builder)
    {
        builder.ToTable("debtors");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ExternalId).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DebtorType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.CompanyName).HasMaxLength(255);
        builder.Property(x => x.DisplayName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.TaxId).HasMaxLength(50);
        
        builder.HasIndex(x => x.ExternalId).IsUnique();
        builder.HasIndex(x => x.DisplayName);
        builder.HasIndex(x => x.DebtorType);
        
        builder.HasMany(x => x.Contacts)
            .WithOne(x => x.Debtor)
            .HasForeignKey(x => x.DebtorId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(x => x.Addresses)
            .WithOne(x => x.Debtor)
            .HasForeignKey(x => x.DebtorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
