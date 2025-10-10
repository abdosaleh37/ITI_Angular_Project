using ITI_Project.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI_Project.DAL.Data.Configurations
{
    internal class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment).IsRequired();
            builder.Property(r => r.Date).IsRequired();
            builder.Property(r => r.ReviewerName).IsRequired().HasMaxLength(200);
            builder.Property(r => r.ReviewerEmail).IsRequired().HasMaxLength(256);

            builder.HasIndex(r => r.ProductId);
            builder.HasCheckConstraint("CK_ProductReview_Rating_1_5", "[Rating] BETWEEN 1 AND 5");
        }
    }
}