using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SewingMaterialsStorage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "colors",
                columns: table => new
                {
                    color_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    color_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("colors_pkey", x => x.color_id);
                });

            migrationBuilder.CreateTable(
                name: "compositions",
                columns: table => new
                {
                    composition_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    composition_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("compositions_pkey", x => x.composition_id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    country_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    country_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("countries_pkey", x => x.country_id);
                });

            migrationBuilder.CreateTable(
                name: "material_types",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    type_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("material_types_pkey", x => x.type_id);
                    table.CheckConstraint("ck_material_types_type_name", "type_name IN ('ткань', 'нитки', 'молния', 'пуговица')");
                });

            migrationBuilder.CreateTable(
                name: "manufacturers",
                columns: table => new
                {
                    manufacturer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    manufacturer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    country_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("manufacturers_pkey", x => x.manufacturer_id);
                    table.ForeignKey(
                        name: "manufacturers_country_id_fkey",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "country_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    material_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    manufacturer_id = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    article = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    price_per_unit = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    min_threshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("materials_pkey", x => x.material_id);
                    table.ForeignKey(
                        name: "materials_manufacturer_id_fkey",
                        column: x => x.manufacturer_id,
                        principalTable: "manufacturers",
                        principalColumn: "manufacturer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "materials_type_id_fkey",
                        column: x => x.type_id,
                        principalTable: "material_types",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consumptions",
                columns: table => new
                {
                    consumption_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    consumption_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("consumptions_pkey", x => x.consumption_id);
                    table.ForeignKey(
                        name: "consumptions_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "material_buttons",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    shape = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    button_size = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("material_buttons_pkey", x => x.material_id);
                    table.ForeignKey(
                        name: "material_buttons_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_colors",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    color_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_colors", x => new { x.material_id, x.color_id });
                    table.ForeignKey(
                        name: "material_colors_color_id_fkey",
                        column: x => x.color_id,
                        principalTable: "colors",
                        principalColumn: "color_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "material_colors_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_compositions",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    composition_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_compositions", x => new { x.material_id, x.composition_id });
                    table.ForeignKey(
                        name: "material_compositions_composition_id_fkey",
                        column: x => x.composition_id,
                        principalTable: "compositions",
                        principalColumn: "composition_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "material_compositions_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_fabrics",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    density = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("material_fabrics_pkey", x => x.material_id);
                    table.ForeignKey(
                        name: "material_fabrics_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_threads",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    thickness = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    length_per_spool = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("material_threads_pkey", x => x.material_id);
                    table.ForeignKey(
                        name: "material_threads_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_zippers",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    zipper_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    zipper_length = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("material_zippers_pkey", x => x.material_id);
                    table.ForeignKey(
                        name: "material_zippers_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplies",
                columns: table => new
                {
                    supply_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    supply_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supplies_pkey", x => x.supply_id);
                    table.ForeignKey(
                        name: "supplies_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "colors_color_name_key",
                table: "colors",
                column: "color_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "compositions_composition_name_key",
                table: "compositions",
                column: "composition_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_consumptions_material_id",
                table: "consumptions",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "countries_country_name_key",
                table: "countries",
                column: "country_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_country_id",
                table: "manufacturers",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "manufacturers_manufacturer_name_key",
                table: "manufacturers",
                column: "manufacturer_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_material_colors_color_id",
                table: "material_colors",
                column: "color_id");

            migrationBuilder.CreateIndex(
                name: "IX_material_compositions_composition_id",
                table: "material_compositions",
                column: "composition_id");

            migrationBuilder.CreateIndex(
                name: "material_types_type_name_key",
                table: "material_types",
                column: "type_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_materials_manufacturer_id",
                table: "materials",
                column: "manufacturer_id");

            migrationBuilder.CreateIndex(
                name: "IX_materials_type_id",
                table: "materials",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "materials_article_key",
                table: "materials",
                column: "article",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplies_material_id",
                table: "supplies",
                column: "material_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consumptions");

            migrationBuilder.DropTable(
                name: "material_buttons");

            migrationBuilder.DropTable(
                name: "material_colors");

            migrationBuilder.DropTable(
                name: "material_compositions");

            migrationBuilder.DropTable(
                name: "material_fabrics");

            migrationBuilder.DropTable(
                name: "material_threads");

            migrationBuilder.DropTable(
                name: "material_zippers");

            migrationBuilder.DropTable(
                name: "supplies");

            migrationBuilder.DropTable(
                name: "colors");

            migrationBuilder.DropTable(
                name: "compositions");

            migrationBuilder.DropTable(
                name: "materials");

            migrationBuilder.DropTable(
                name: "manufacturers");

            migrationBuilder.DropTable(
                name: "material_types");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}