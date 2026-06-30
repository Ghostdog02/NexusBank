using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusBank.Domain.Entities;

namespace NexusBank.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.NationalId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.AddressLine1)
            .HasMaxLength(255);

        builder.Property(p => p.AddressLine2)
            .HasMaxLength(255);

        builder.Property(p => p.City)
            .HasMaxLength(100);

        builder.Property(p => p.Country)
            .IsRequired()
            .HasColumnType("char(2)");

        builder.Property(p => p.PostalCode)
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();
    }
}
