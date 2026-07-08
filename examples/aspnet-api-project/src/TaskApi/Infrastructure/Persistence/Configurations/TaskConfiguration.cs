using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskApi.Features.Tasks.Model;

namespace TaskApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="TaskItem"/> to the <c>Tasks</c> table: string-backed enums, bounded text columns, a
/// JSON-encoded tags column, and indexes for the common list filters and sorts.
/// </summary>
internal sealed class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(2000);

        // Persist the enums by name, not ordinal: stable across member reordering and readable in the DB.
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(16);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        // SQLite has no array type: serialize tags as a JSON string, with a value comparer so EF change
        // tracking compares the list by value rather than by reference.
        var tagsComparer = new ValueComparer<List<string>>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            v => v == null ? 0 : v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
            v => v == null ? new List<string>() : v.ToList());

        builder.Property(t => t.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(tagsComparer);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);
    }
}
