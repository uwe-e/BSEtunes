using System;
using System.Collections.Generic;
using BSEtunes.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace BSEtunes.Infrastructure.Data;

public partial class RecordsDbContext : DbContext
{
    public RecordsDbContext()
    {
    }

    public RecordsDbContext(DbContextOptions<RecordsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<album> albums { get; set; }

    public virtual DbSet<filtersetting> filtersettings { get; set; }

    public virtual DbSet<genre> genres { get; set; }

    public virtual DbSet<genre1> genres1 { get; set; }

    public virtual DbSet<history> histories { get; set; }

    public virtual DbSet<interpreten> interpretens { get; set; }

    public virtual DbSet<lieder> lieders { get; set; }

    public virtual DbSet<medium> media { get; set; }

    public virtual DbSet<playlist> playlists { get; set; }

    public virtual DbSet<playlistentry> playlistentries { get; set; }

    public virtual DbSet<playlistswithnumberofentry> playlistswithnumberofentries { get; set; }

    public virtual DbSet<titel> titels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=Platten;user=root;password=\"rolling;_Stones\"", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<album>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("albums");

            entity.Property(e => e.Album_AlbumId)
                .HasMaxLength(36)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Album_Title)
                .HasMaxLength(60)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Artist_Name)
                .HasMaxLength(60)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Artist_SortName)
                .HasMaxLength(60)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Genre_Id).HasDefaultValueSql("'0'");
            entity.Property(e => e.Genre_Name)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
        });

        modelBuilder.Entity<filtersetting>(entity =>
        {
            entity.HasKey(e => e.filterid).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.benutzer)
                .HasMaxLength(50)
                .HasDefaultValueSql("''");
            entity.Property(e => e.isused)
                .HasDefaultValueSql("b'0'")
                .HasColumnType("bit(1)");
            entity.Property(e => e.value).HasMaxLength(255);
        });

        modelBuilder.Entity<genre>(entity =>
        {
            entity.HasKey(e => e.genreid).HasName("PRIMARY");

            entity
                .ToTable("genre")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.genre1)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("genre");
            entity.Property(e => e.guid)
                .HasMaxLength(36)
                .HasDefaultValueSql("''");
            entity.Property(e => e.timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<genre1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("genres");

            entity.Property(e => e.Genre_Name)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
        });

        modelBuilder.Entity<history>(entity =>
        {
            entity.HasKey(e => e.PlayID).HasName("PRIMARY");

            entity
                .ToTable("history")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Benutzer)
                .HasMaxLength(50)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Interpret)
                .HasMaxLength(50)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Lied)
                .HasMaxLength(60)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Titel)
                .HasMaxLength(60)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Zeit)
                .HasDefaultValueSql("'0000-00-00 00:00:00'")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<interpreten>(entity =>
        {
            entity.HasKey(e => e.InterpretID).HasName("PRIMARY");

            entity
                .ToTable("interpreten")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Interpret, "iInterpret").HasAnnotation("MySql:FullTextIndex", true);

            entity.Property(e => e.Guid)
                .HasMaxLength(36)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Interpret)
                .HasMaxLength(60)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Interpret_Lang)
                .HasMaxLength(60)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<lieder>(entity =>
        {
            entity.HasKey(e => e.LiedID).HasName("PRIMARY");

            entity
                .ToTable("lieder")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Lied, "iLieder").HasAnnotation("MySql:FullTextIndex", true);

            entity.Property(e => e.Dauer).HasColumnType("datetime");
            entity.Property(e => e.Lied).HasMaxLength(100);
            entity.Property(e => e.Liedpfad).HasMaxLength(255);
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.guid)
                .HasMaxLength(36)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<medium>(entity =>
        {
            entity.HasKey(e => e.MediumID).HasName("PRIMARY");

            entity
                .ToTable("medium")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Beschreibung).HasMaxLength(50);
            entity.Property(e => e.Guid)
                .HasMaxLength(36)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Medium)
                .HasMaxLength(5)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<playlist>(entity =>
        {
            entity.HasKey(e => e.ListId).HasName("PRIMARY");

            entity
                .ToTable("playlist")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.guid, "iGuid").IsUnique();

            entity.Property(e => e.ListName)
                .HasMaxLength(100)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.User)
                .HasMaxLength(50)
                .HasDefaultValueSql("''");
            entity.Property(e => e.guid)
                .HasMaxLength(36)
                .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<playlistentry>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PRIMARY");

            entity.UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Guid).HasDefaultValueSql("''");
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.sortorder).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<playlistswithnumberofentry>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("playlistswithnumberofentries");

            entity.Property(e => e.ListName)
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.User)
                .HasMaxLength(50)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.guid)
                .HasMaxLength(36)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
        });

        modelBuilder.Entity<titel>(entity =>
        {
            entity.HasKey(e => e.TitelID).HasName("PRIMARY");

            entity
                .ToTable("titel")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Guid, "iGuid").IsUnique();

            entity.HasIndex(e => e.Titel, "iTitel").HasAnnotation("MySql:FullTextIndex", true);

            entity.Property(e => e.ErstellDatum).HasColumnType("datetime");
            entity.Property(e => e.ErstellerNm)
                .HasMaxLength(50)
                .HasDefaultValueSql("''");
            entity.Property(e => e.Guid)
                .HasMaxLength(36)
                .HasDefaultValueSql("''");
            entity.Property(e => e.MutationDatum).HasColumnType("datetime");
            entity.Property(e => e.MutationNm)
                .HasMaxLength(50)
                .HasDefaultValueSql("''");
            entity.Property(e => e.PictureFormat).HasMaxLength(5);
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Titel)
                .HasMaxLength(60)
                .HasDefaultValueSql("''");
            entity.Property(e => e.mp3tag).HasDefaultValueSql("'0'");
            entity.Property(e => e.thumbnail).HasColumnType("mediumblob");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
