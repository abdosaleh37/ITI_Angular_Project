using ITI_Project.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI_Project.DAL.Data.Configurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            // Scalars
            builder.Property(p => p.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.Description)
                   .IsRequired();

            builder.Property(p => p.Category)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Brand)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Sku)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.Property(p => p.Price)
                   .HasPrecision(18, 2);

            builder.Property(p => p.AvailabilityStatus)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(p => p.WarrantyInformation)
                   .HasMaxLength(500);

            builder.Property(p => p.ShippingInformation)
                   .HasMaxLength(500);

            builder.Property(p => p.ReturnPolicy)
                   .HasMaxLength(500);

            builder.Property(p => p.Thumbnail)
                   .IsRequired()
                   .HasMaxLength(2048);

            // Indexes
            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.Category);
            builder.HasIndex(p => p.Brand);

            // Owned value objects
            builder.OwnsOne(p => p.Dimensions, dim =>
            {
                dim.Property(d => d.Width);
                dim.Property(d => d.Height);
                dim.Property(d => d.Depth);
            });

            builder.OwnsOne(p => p.Meta, meta =>
            {
                meta.Property(m => m.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                meta.Property(m => m.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                meta.Property(m => m.Barcode)
                    .HasMaxLength(128);
                meta.Property(m => m.QrCode)
                    .HasMaxLength(2048);
            });

            // Relationships to normalized collections
            builder.HasMany(p => p.Images)
                   .WithOne(i => i.Product)
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Reviews)
                   .WithOne(r => r.Product)
                   .HasForeignKey(r => r.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.ProductTags)
                   .WithOne(pt => pt.Product)
                   .HasForeignKey(pt => pt.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
