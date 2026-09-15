using JobTracker.Users.Domain.Entities;
using JobTracker.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Users.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        // The Email value object is a single value, so a value converter (VO <-> string) is a
        // better fit than an owned type. The unique index enforces one account per email.
        builder.Property(x => x.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).IsRequired();

        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
