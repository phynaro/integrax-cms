using Cases.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class CollectionCaseConfiguration : IEntityTypeConfiguration<CollectionCase>
{
    public void Configure(EntityTypeBuilder<CollectionCase> builder)
    {
        builder.ToTable("collection_cases");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.CaseNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        
        builder.HasIndex(x => x.CaseNumber).IsUnique();
        builder.HasIndex(x => x.DebtAccountId).IsUnique();
        builder.HasIndex(x => x.AssignedToId);
        builder.HasIndex(x => x.Status);
        
        builder.HasOne(x => x.DebtAccount)
            .WithOne()
            .HasForeignKey<CollectionCase>(x => x.DebtAccountId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.AssignedTo)
            .WithMany()
            .HasForeignKey(x => x.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
