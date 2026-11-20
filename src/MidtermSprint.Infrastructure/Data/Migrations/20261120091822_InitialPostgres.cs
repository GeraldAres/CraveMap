using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MidtermSprint.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "allergens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allergens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "characteristics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characteristics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cuisines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuisines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dietary_preferences",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dietary_preferences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "food_niches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_niches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    price_range = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "text", nullable: true),
                    supports_dine_in = table.Column<bool>(type: "boolean", nullable: false),
                    supports_takeout = table.Column<bool>(type: "boolean", nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurants", x => x.id);
                    table.CheckConstraint("ck_restaurants_latitude", "latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_restaurants_longitude", "longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_restaurants_price_range", "price_range BETWEEN 1 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    restaurant_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    ingredients = table.Column<string>(type: "text", nullable: true),
                    menu_section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    cuisine_id = table.Column<int>(type: "integer", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    is_available = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_menu_items_cuisines_cuisine_id",
                        column: x => x.cuisine_id,
                        principalTable: "cuisines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_menu_items_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_hours",
                columns: table => new
                {
                    restaurant_id = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    opens_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    closes_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurant_hours", x => new { x.restaurant_id, x.day_of_week });
                    table.ForeignKey(
                        name: "FK_restaurant_hours_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_allergens",
                columns: table => new
                {
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    allergen_id = table.Column<int>(type: "integer", nullable: false),
                    evidence_status = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    verified_on = table.Column<DateOnly>(type: "date", nullable: true),
                    cross_contact_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_allergens", x => new { x.menu_item_id, x.allergen_id });
                    table.ForeignKey(
                        name: "FK_menu_item_allergens_allergens_allergen_id",
                        column: x => x.allergen_id,
                        principalTable: "allergens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_allergens_menu_items_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_characteristics",
                columns: table => new
                {
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    characteristic_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_characteristics", x => new { x.menu_item_id, x.characteristic_id });
                    table.ForeignKey(
                        name: "FK_menu_item_characteristics_characteristics_characteristic_id",
                        column: x => x.characteristic_id,
                        principalTable: "characteristics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_characteristics_menu_items_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_dietary_attributes",
                columns: table => new
                {
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    dietary_preference_id = table.Column<int>(type: "integer", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_dietary_attributes", x => new { x.menu_item_id, x.dietary_preference_id });
                    table.ForeignKey(
                        name: "FK_menu_item_dietary_attributes_dietary_preferences_dietary_pr~",
                        column: x => x.dietary_preference_id,
                        principalTable: "dietary_preferences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_dietary_attributes_menu_items_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_niches",
                columns: table => new
                {
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    food_niche_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_niches", x => new { x.menu_item_id, x.food_niche_id });
                    table.ForeignKey(
                        name: "FK_menu_item_niches_food_niches_food_niche_id",
                        column: x => x.food_niche_id,
                        principalTable: "food_niches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_niches_menu_items_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_allergens_name",
                table: "allergens",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_allergens_slug",
                table: "allergens",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_characteristics_kind_name",
                table: "characteristics",
                columns: new[] { "kind", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuisines_name",
                table: "cuisines",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuisines_slug",
                table: "cuisines",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dietary_preferences_name",
                table: "dietary_preferences",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dietary_preferences_slug",
                table: "dietary_preferences",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_niches_name",
                table: "food_niches",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_niches_slug",
                table: "food_niches",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_allergens_allergen_id",
                table: "menu_item_allergens",
                column: "allergen_id");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_characteristics_characteristic_id",
                table: "menu_item_characteristics",
                column: "characteristic_id");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_dietary_attributes_dietary_preference_id",
                table: "menu_item_dietary_attributes",
                column: "dietary_preference_id");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_niches_food_niche_id",
                table: "menu_item_niches",
                column: "food_niche_id");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_cuisine_id",
                table: "menu_items",
                column: "cuisine_id");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_restaurant_id_is_available",
                table: "menu_items",
                columns: new[] { "restaurant_id", "is_available" });

            migrationBuilder.CreateIndex(
                name: "IX_restaurants_is_published_latitude_longitude",
                table: "restaurants",
                columns: new[] { "is_published", "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_restaurants_latitude_longitude",
                table: "restaurants",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_restaurants_slug",
                table: "restaurants",
                column: "slug",
                unique: true);

            migrationBuilder.Sql(
                """
                ALTER TABLE public.restaurants ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.restaurant_hours ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.menu_items ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.cuisines ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.food_niches ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.menu_item_niches ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.characteristics ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.menu_item_characteristics ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.dietary_preferences ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.menu_item_dietary_attributes ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.allergens ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.menu_item_allergens ENABLE ROW LEVEL SECURITY;

                GRANT SELECT ON TABLE public.restaurants, public.menu_items, public.allergens
                    TO anon, authenticated;

                CREATE POLICY public_read_restaurants
                    ON public.restaurants FOR SELECT TO anon, authenticated USING (true);
                CREATE POLICY public_read_menu_items
                    ON public.menu_items FOR SELECT TO anon, authenticated USING (true);
                CREATE POLICY public_read_allergens
                    ON public.allergens FOR SELECT TO anon, authenticated USING (true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "menu_item_allergens");

            migrationBuilder.DropTable(
                name: "menu_item_characteristics");

            migrationBuilder.DropTable(
                name: "menu_item_dietary_attributes");

            migrationBuilder.DropTable(
                name: "menu_item_niches");

            migrationBuilder.DropTable(
                name: "restaurant_hours");

            migrationBuilder.DropTable(
                name: "allergens");

            migrationBuilder.DropTable(
                name: "characteristics");

            migrationBuilder.DropTable(
                name: "dietary_preferences");

            migrationBuilder.DropTable(
                name: "food_niches");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "cuisines");

            migrationBuilder.DropTable(
                name: "restaurants");
        }
    }
}
