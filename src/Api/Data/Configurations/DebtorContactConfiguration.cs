using Debtors.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class DebtorContactConfiguration : IEntityTypeConfiguration<DebtorContact>
{
    public void Configure(EntityTypeBuilder<DebtorContact> builder)
    {
        builder.ToTable("debtor_contacts");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(50);
        builder.Property(x => x.Value).HasMaxLength(255).IsRequired();
        
        builder.HasIndex(x => x.DebtorId);
    }
}
