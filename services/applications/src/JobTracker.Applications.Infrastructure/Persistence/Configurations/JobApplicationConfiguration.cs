using JobTracker.Applications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Applications.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the <see cref="JobApplication"/> aggregate, expressed with the Fluent API
/// (kept here in Infrastructure rather than as attributes on the entity, so the Domain stays free
/// of persistence concerns). Column and table names are snake_cased globally by the naming
/// convention configured in <c>AddInfrastructure</c>.
/// </summary>
internal sealed class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("job_applications");

        builder.HasKey(x => x.Id);

        // The Guid is assigned by the domain (JobApplication.Create), not by the database.
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Owner (the authenticated user's 'sub'). Indexed because every query filters by it.
        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.Company)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasMaxLength(200);

        // Store enums as their names (e.g. "Applied") for readable, reorder-proof rows.
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Source)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        // DateOnly? maps to PostgreSQL `date`; DateTimeOffset maps to `timestamptz` (Npgsql).
        builder.Property(x => x.AppliedDate);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // SalaryRange is a value object -> map it as an OWNED type: its fields live as extra
        // columns on the same table, with no identity of their own. The whole thing is optional.
        builder.OwnsOne(x => x.Salary, salary =>
        {
            salary.Property(s => s.Min)
                .HasColumnName("salary_min")
                .HasColumnType("numeric(18,2)");

            salary.Property(s => s.Max)
                .HasColumnName("salary_max")
                .HasColumnType("numeric(18,2)");

            salary.Property(s => s.Currency)
                .HasColumnName("salary_currency")
                .HasMaxLength(3);
        });

        builder.Navigation(x => x.Salary).IsRequired(false);
    }
}
