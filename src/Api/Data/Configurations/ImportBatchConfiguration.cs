using Imports.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("import_batches");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Filename).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        
        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        
        builder.HasOne(x => x.Portfolio)
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(x => x.RolledBackBy)
            .WithMany()
            .HasForeignKey(x => x.RolledBackById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
