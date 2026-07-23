using Debtors.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class DebtorAddressConfiguration : IEntityTypeConfiguration<DebtorAddress>
{
    public void Configure(EntityTypeBuilder<DebtorAddress> builder)
    {
        builder.ToTable("debtor_addresses");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Label).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.AddressLine1).HasMaxLength(255).IsRequired();
        builder.Property(x => x.AddressLine2).HasMaxLength(255);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.PostalCode).HasMaxLength(20);
        builder.Property(x => x.Country).HasMaxLength(100);
        
        builder.HasIndex(x => x.DebtorId);
    }
}
