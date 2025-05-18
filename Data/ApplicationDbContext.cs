using Microsoft.EntityFrameworkCore;
using SewingMaterialsStorage.Models;

namespace SewingMaterialsStorage.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialType> MaterialTypes { get; set; }
        public DbSet<MaterialFabric> MaterialFabrics { get; set; }
        public DbSet<MaterialThread> MaterialThreads { get; set; }
        public DbSet<MaterialZipper> MaterialZippers { get; set; }
        public DbSet<MaterialButton> MaterialButtons { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Composition> Compositions { get; set; }
        public DbSet<MaterialColor> MaterialColors { get; set; }
        public DbSet<MaterialComposition> MaterialCompositions { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<Consumption> Consumptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка имен таблиц в соответствии с базой данных
            modelBuilder.Entity<Material>().ToTable("materials");
            modelBuilder.Entity<MaterialType>().ToTable("material_types");
            modelBuilder.Entity<MaterialFabric>().ToTable("material_fabrics");
            modelBuilder.Entity<MaterialThread>().ToTable("material_threads");
            modelBuilder.Entity<MaterialZipper>().ToTable("material_zippers");
            modelBuilder.Entity<MaterialButton>().ToTable("material_buttons");
            modelBuilder.Entity<Manufacturer>().ToTable("manufacturers");
            modelBuilder.Entity<Country>().ToTable("countries");
            modelBuilder.Entity<Color>().ToTable("colors");
            modelBuilder.Entity<Composition>().ToTable("compositions");
            modelBuilder.Entity<MaterialColor>().ToTable("material_colors");
            modelBuilder.Entity<MaterialComposition>().ToTable("material_compositions");
            modelBuilder.Entity<Supply>().ToTable("supplies");
            modelBuilder.Entity<Consumption>().ToTable("consumptions");

            // Настройка MaterialType
            modelBuilder.Entity<MaterialType>(entity =>
            {
                entity.HasKey(e => e.TypeId)
                    .HasName("material_types_pkey");

                entity.Property(e => e.TypeId)
                    .HasColumnName("type_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.TypeName)
                    .HasColumnName("type_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(e => e.TypeName)
                    .IsUnique()
                    .HasDatabaseName("material_types_type_name_key");

                entity.HasCheckConstraint("ck_material_types_type_name",
                    "type_name IN ('ткань', 'нитки', 'молния', 'пуговица')");
            });

            // Настройка Material
            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasKey(e => e.MaterialId)
                    .HasName("materials_pkey");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.MaterialName)
                    .HasColumnName("material_name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Article)
                    .HasColumnName("article")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(e => e.Article)
                    .IsUnique()
                    .HasDatabaseName("materials_article_key");

                entity.Property(e => e.PricePerUnit)
                    .HasColumnName("price_per_unit")
                    .HasColumnType("numeric(10,2)")
                    .IsRequired()
                    .HasDefaultValue(0m);

                entity.Property(e => e.MinThreshold)
                    .HasColumnName("min_threshold")
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.Notes)
                    .HasColumnName("notes")
                    .HasColumnType("text");

                entity.Property(e => e.TypeId)
                    .HasColumnName("type_id")
                    .IsRequired();

                entity.Property(e => e.ManufacturerId)
                    .HasColumnName("manufacturer_id")
                    .IsRequired();

                entity.HasOne(m => m.MaterialType)
                    .WithMany()
                    .HasForeignKey(m => m.TypeId)
                    .HasConstraintName("materials_type_id_fkey")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Manufacturer)
                    .WithMany()
                    .HasForeignKey(m => m.ManufacturerId)
                    .HasConstraintName("materials_manufacturer_id_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка MaterialFabric
            modelBuilder.Entity<MaterialFabric>(entity =>
            {
                entity.HasKey(e => e.MaterialId)
                    .HasName("material_fabrics_pkey");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Width)
                    .HasColumnName("width")
                    .HasColumnType("numeric(5,2)")
                    .IsRequired();

                entity.Property(e => e.Density)
                    .HasColumnName("density")
                    .HasColumnType("numeric(5,2)")
                    .IsRequired();

                entity.HasOne(mf => mf.Material)
                    .WithOne(m => m.FabricDetails)
                    .HasForeignKey<MaterialFabric>(mf => mf.MaterialId)
                    .HasConstraintName("material_fabrics_material_id_fkey")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка MaterialThread
            modelBuilder.Entity<MaterialThread>(entity =>
            {
                entity.HasKey(e => e.MaterialId)
                    .HasName("material_threads_pkey");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Thickness)
                    .HasColumnName("thickness")
                    .HasColumnType("numeric(5,2)")
                    .IsRequired();

                entity.Property(e => e.LengthPerSpool)
                    .HasColumnName("length_per_spool")
                    .IsRequired();

                entity.HasOne(mt => mt.Material)
                    .WithOne(m => m.ThreadDetails)
                    .HasForeignKey<MaterialThread>(mt => mt.MaterialId)
                    .HasConstraintName("material_threads_material_id_fkey")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка MaterialZipper
            modelBuilder.Entity<MaterialZipper>(entity =>
            {
                entity.HasKey(e => e.MaterialId)
                    .HasName("material_zippers_pkey");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ZipperType)
                    .HasColumnName("zipper_type")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.ZipperLength)
                    .HasColumnName("zipper_length")
                    .HasColumnType("numeric(5,2)")
                    .IsRequired();

                entity.HasOne(mz => mz.Material)
                    .WithOne(m => m.ZipperDetails)
                    .HasForeignKey<MaterialZipper>(mz => mz.MaterialId)
                    .HasConstraintName("material_zippers_material_id_fkey")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка MaterialButton
            modelBuilder.Entity<MaterialButton>(entity =>
            {
                entity.HasKey(e => e.MaterialId)
                    .HasName("material_buttons_pkey");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Shape)
                    .HasColumnName("shape")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.ButtonSize)
                    .HasColumnName("button_size")
                    .HasColumnType("numeric(5,2)")
                    .IsRequired();

                entity.HasOne(mb => mb.Material)
                    .WithOne(m => m.ButtonDetails)
                    .HasForeignKey<MaterialButton>(mb => mb.MaterialId)
                    .HasConstraintName("material_buttons_material_id_fkey")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка Manufacturer
            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasKey(e => e.ManufacturerId)
                    .HasName("manufacturers_pkey");

                entity.Property(e => e.ManufacturerId)
                    .HasColumnName("manufacturer_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ManufacturerName)
                    .HasColumnName("manufacturer_name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasIndex(e => e.ManufacturerName)
                    .IsUnique()
                    .HasDatabaseName("manufacturers_manufacturer_name_key");

                entity.Property(e => e.CountryId)
                    .HasColumnName("country_id")
                    .IsRequired();

                entity.HasOne(m => m.Country)
                    .WithMany()
                    .HasForeignKey(m => m.CountryId)
                    .HasConstraintName("manufacturers_country_id_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка Country
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.CountryId)
                    .HasName("countries_pkey");

                entity.Property(e => e.CountryId)
                    .HasColumnName("country_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CountryName)
                    .HasColumnName("country_name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasIndex(e => e.CountryName)
                    .IsUnique()
                    .HasDatabaseName("countries_country_name_key");
            });

            // Настройка Color
            modelBuilder.Entity<Color>(entity =>
            {
                entity.HasKey(e => e.ColorId)
                    .HasName("colors_pkey");

                entity.Property(e => e.ColorId)
                    .HasColumnName("color_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ColorName)
                    .HasColumnName("color_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(e => e.ColorName)
                    .IsUnique()
                    .HasDatabaseName("colors_color_name_key");
            });

            // Настройка Composition
            modelBuilder.Entity<Composition>(entity =>
            {
                entity.HasKey(e => e.CompositionId)
                    .HasName("compositions_pkey");

                entity.Property(e => e.CompositionId)
                    .HasColumnName("composition_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CompositionName)
                    .HasColumnName("composition_name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasIndex(e => e.CompositionName)
                    .IsUnique()
                    .HasDatabaseName("compositions_composition_name_key");
            });

            // Настройка MaterialColor
            modelBuilder.Entity<MaterialColor>(entity =>
            {
                entity.HasKey(e => new { e.MaterialId, e.ColorId })
                    .HasName("material_colors_pkey");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id");

                entity.Property(e => e.ColorId)
                    .HasColumnName("color_id");

                entity.HasOne(mc => mc.Material)
                    .WithMany(m => m.Colors)
                    .HasForeignKey(mc => mc.MaterialId)
                    .HasConstraintName("material_colors_material_id_fkey")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mc => mc.Color)
                    .WithMany()
                    .HasForeignKey(mc => mc.ColorId)
                    .HasConstraintName("material_colors_color_id_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка MaterialComposition
            modelBuilder.Entity<MaterialComposition>(entity =>
            {
                entity.HasKey(e => new { e.MaterialId, e.CompositionId })
                    .HasName("material_compositions_pkey");

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id");

                entity.Property(e => e.CompositionId)
                    .HasColumnName("composition_id");

                entity.HasOne(mc => mc.Material)
                    .WithMany(m => m.Compositions)
                    .HasForeignKey(mc => mc.MaterialId)
                    .HasConstraintName("material_compositions_material_id_fkey")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mc => mc.Composition)
                    .WithMany()
                    .HasForeignKey(mc => mc.CompositionId)
                    .HasConstraintName("material_compositions_composition_id_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка Supply
            modelBuilder.Entity<Supply>(entity =>
            {
                entity.HasKey(e => e.SupplyId)
                    .HasName("supplies_pkey");

                entity.Property(e => e.SupplyId)
                    .HasColumnName("supply_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .IsRequired();

                entity.Property(e => e.SupplyDate)
                    .HasColumnName("supply_date")
                    .HasColumnType("date")
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                entity.HasOne(s => s.Material)
                    .WithMany(m => m.Supplies)
                    .HasForeignKey(s => s.MaterialId)
                    .HasConstraintName("supplies_material_id_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка Consumption
            modelBuilder.Entity<Consumption>(entity =>
            {
                entity.HasKey(e => e.ConsumptionId)
                    .HasName("consumptions_pkey");

                entity.Property(e => e.ConsumptionId)
                    .HasColumnName("consumption_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.MaterialId)
                    .HasColumnName("material_id")
                    .IsRequired();

                entity.Property(e => e.ConsumptionDate)
                    .HasColumnName("consumption_date")
                    .HasColumnType("date")
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                entity.Property(e => e.OrderId)
                    .HasColumnName("order_id")
                    .HasMaxLength(50);

                entity.HasOne(c => c.Material)
                    .WithMany(m => m.Consumptions)
                    .HasForeignKey(c => c.MaterialId)
                    .HasConstraintName("consumptions_material_id_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}