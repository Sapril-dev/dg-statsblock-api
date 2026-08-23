using DunorGames.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DunorGames.Infrastructure.Persistence;

public sealed class DunorGamesDbContext(DbContextOptions<DunorGamesDbContext> options) : DbContext(options)
{
    public DbSet<GameSystemEntity> GameSystems => Set<GameSystemEntity>();

    public DbSet<StatblockEntity> Statblocks => Set<StatblockEntity>();

    public DbSet<StatblockTagEntity> StatblockTags => Set<StatblockTagEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameSystemEntity>(entity =>
        {
            entity.ToTable("GameSystems");
            entity.HasKey(gameSystem => gameSystem.Code);
            entity.Property(gameSystem => gameSystem.Code).HasMaxLength(32);
            entity.Property(gameSystem => gameSystem.DisplayName).HasMaxLength(80).IsRequired();
            entity.Property(gameSystem => gameSystem.SortOrder).IsRequired();

            entity.HasData(
                new GameSystemEntity { Code = "dungeonsAndDragons", DisplayName = "Dungeons & Dragons", SortOrder = 1 },
                new GameSystemEntity { Code = "talesOfTheValiant", DisplayName = "Tales of the Valiant", SortOrder = 2 },
                new GameSystemEntity { Code = "daggerheart", DisplayName = "Daggerheart", SortOrder = 3 },
                new GameSystemEntity { Code = "drawSteel", DisplayName = "Draw Steel", SortOrder = 4 },
                new GameSystemEntity { Code = "dc20", DisplayName = "DC20", SortOrder = 5 });
        });

        modelBuilder.Entity<StatblockEntity>(entity =>
        {
            entity.ToTable("Statblocks", table =>
            {
                table.HasCheckConstraint("CK_Statblocks_Origin", "[Origin] IN ('manual', 'imported', 'converted')");
                table.HasCheckConstraint("CK_Statblocks_Status", "[Status] IN ('draft', 'published', 'archived')");
                table.HasCheckConstraint("CK_Statblocks_SchemaVersion", "LEN([SchemaVersion]) > 0");
                table.HasCheckConstraint("CK_Statblocks_SystemDataJson", "ISJSON([SystemDataJson]) = 1");
            });

            entity.HasKey(statblock => statblock.Id);
            entity.Property(statblock => statblock.Id).ValueGeneratedNever();
            entity.Property(statblock => statblock.OwnerId).IsRequired();
            entity.Property(statblock => statblock.SchemaVersion).HasMaxLength(16).IsRequired();
            entity.Property(statblock => statblock.SystemCode).HasColumnName("System").HasMaxLength(32).IsRequired();
            entity.Property(statblock => statblock.Name).HasMaxLength(200).IsRequired();
            entity.Property(statblock => statblock.Subtitle).HasMaxLength(200);
            entity.Property(statblock => statblock.Description).HasColumnType("nvarchar(max)");
            entity.Property(statblock => statblock.Origin).HasMaxLength(16).IsRequired();
            entity.Property(statblock => statblock.SourceSystemCode).HasColumnName("SourceSystem").HasMaxLength(32);
            entity.Property(statblock => statblock.SourceReference).HasColumnName("SourceReference").HasMaxLength(1000);
            entity.Property(statblock => statblock.Status).HasMaxLength(16).HasDefaultValue("draft").IsRequired();
            entity.Property(statblock => statblock.Notes).HasColumnType("nvarchar(max)");
            entity.Property(statblock => statblock.SystemDataJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(statblock => statblock.CreatedAt).HasDefaultValueSql("CONVERT(datetimeoffset, SYSUTCDATETIME())").IsRequired();
            entity.Property(statblock => statblock.UpdatedAt).HasDefaultValueSql("CONVERT(datetimeoffset, SYSUTCDATETIME())").IsRequired();
            entity.Property(statblock => statblock.RowVersion).IsRowVersion();

            entity.HasOne<GameSystemEntity>()
                .WithMany()
                .HasForeignKey(statblock => statblock.SystemCode)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<GameSystemEntity>()
                .WithMany()
                .HasForeignKey(statblock => statblock.SourceSystemCode)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<StatblockEntity>()
                .WithMany()
                .HasForeignKey(statblock => statblock.SourceStatblockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(statblock => new { statblock.OwnerId, statblock.SystemCode, statblock.Status });
            entity.HasIndex(statblock => new { statblock.OwnerId, statblock.UpdatedAt });
        });

        modelBuilder.Entity<StatblockTagEntity>(entity =>
        {
            entity.ToTable("StatblockTags");
            entity.HasKey(tag => new { tag.StatblockId, tag.Tag });
            entity.Property(tag => tag.Tag).HasMaxLength(64).IsRequired();
            entity.HasOne(tag => tag.Statblock)
                .WithMany(statblock => statblock.Tags)
                .HasForeignKey(tag => tag.StatblockId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
