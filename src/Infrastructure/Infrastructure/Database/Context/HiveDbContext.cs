using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Context;

public partial class HiveDbContext : DbContext
{
    public HiveDbContext(DbContextOptions<HiveDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Movie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("movies_pk_id");

            entity.ToTable("movies");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedTime).HasColumnName("created_time");
            entity.Property(e => e.FilePath)
                .HasColumnType("character varying")
                .HasColumnName("file_path");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.HashValue)
                .HasColumnType("character varying")
                .HasColumnName("hash_value");
            entity.Property(e => e.Image)
                .HasColumnType("character varying")
                .HasColumnName("image");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ModifiedTime).HasColumnName("modified_time");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.SubTitleFilePath)
                .HasColumnType("character varying")
                .HasColumnName("sub_title_file_path");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
