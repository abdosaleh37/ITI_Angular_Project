using ITI_Project.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI_Project.DAL.Data.Configurations
{
    internal class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.Url).IsRequired().HasMaxLength(2048);
            builder.Property(pi => pi.SortOrder).HasDefaultValue(0);

            builder.HasIndex(pi => new { pi.ProductId, pi.SortOrder }).IsUnique(false);
            builder.HasIndex(pi => new { pi.ProductId, pi.IsPrimary }).IsUnique(false);
        }
    }
}