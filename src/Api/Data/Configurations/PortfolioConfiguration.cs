using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolios.Core.Entities;

namespace Api.Data.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portfolios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ExternalId).IsUnique().HasFilter("\"ExternalId\" IS NOT NULL");
        builder.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId);
    }
}
