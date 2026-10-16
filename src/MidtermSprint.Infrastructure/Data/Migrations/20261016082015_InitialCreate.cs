using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidtermSprint.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allergens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characteristics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characteristics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cuisines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuisines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DietaryPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietaryPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodNiches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodNiches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    PriceRange = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<decimal>(type: "TEXT", precision: 2, scale: 1, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    SupportsDineIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    SupportsTakeout = table.Column<bool>(type: "INTEGER", nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                    table.CheckConstraint("CK_Restaurants_Latitude", "Latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_Restaurants_Longitude", "Longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("CK_Restaurants_PriceRange", "PriceRange BETWEEN 1 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RestaurantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1200, nullable: false),
                    Ingredients = table.Column<string>(type: "TEXT", nullable: true),
                    MenuSection = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    CuisineId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Cuisines_CuisineId",
                        column: x => x.CuisineId,
                        principalTable: "Cuisines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItems_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantHours",
                columns: table => new
                {
                    RestaurantId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    OpensAt = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    ClosesAt = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantHours", x => new { x.RestaurantId, x.DayOfWeek });
                    table.ForeignKey(
                        name: "FK_RestaurantHours_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemAllergens",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    AllergenId = table.Column<int>(type: "INTEGER", nullable: false),
                    EvidenceStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 240, nullable: true),
                    VerifiedOn = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CrossContactNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemAllergens", x => new { x.MenuItemId, x.AllergenId });
                    table.ForeignKey(
                        name: "FK_MenuItemAllergens_Allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "Allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemAllergens_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemCharacteristics",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacteristicId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemCharacteristics", x => new { x.MenuItemId, x.CharacteristicId });
                    table.ForeignKey(
                        name: "FK_MenuItemCharacteristics_Characteristics_CharacteristicId",
                        column: x => x.CharacteristicId,
                        principalTable: "Characteristics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemCharacteristics_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemDietaryAttributes",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    DietaryPreferenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemDietaryAttributes", x => new { x.MenuItemId, x.DietaryPreferenceId });
                    table.ForeignKey(
                        name: "FK_MenuItemDietaryAttributes_DietaryPreferences_DietaryPreferenceId",
                        column: x => x.DietaryPreferenceId,
                        principalTable: "DietaryPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemDietaryAttributes_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemNiches",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    FoodNicheId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemNiches", x => new { x.MenuItemId, x.FoodNicheId });
                    table.ForeignKey(
                        name: "FK_MenuItemNiches_FoodNiches_FoodNicheId",
                        column: x => x.FoodNicheId,
                        principalTable: "FoodNiches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemNiches_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Allergens_Name",
                table: "Allergens",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Allergens_Slug",
                table: "Allergens",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characteristics_Kind_Name",
                table: "Characteristics",
                columns: new[] { "Kind", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cuisines_Name",
                table: "Cuisines",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cuisines_Slug",
                table: "Cuisines",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DietaryPreferences_Name",
                table: "DietaryPreferences",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DietaryPreferences_Slug",
                table: "DietaryPreferences",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodNiches_Name",
                table: "FoodNiches",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodNiches_Slug",
                table: "FoodNiches",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAllergens_AllergenId",
                table: "MenuItemAllergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemCharacteristics_CharacteristicId",
                table: "MenuItemCharacteristics",
                column: "CharacteristicId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemDietaryAttributes_DietaryPreferenceId",
                table: "MenuItemDietaryAttributes",
                column: "DietaryPreferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemNiches_FoodNicheId",
                table: "MenuItemNiches",
                column: "FoodNicheId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CuisineId",
                table: "MenuItems",
                column: "CuisineId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId_IsAvailable",
                table: "MenuItems",
                columns: new[] { "RestaurantId", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Latitude_Longitude",
                table: "Restaurants",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Slug",
                table: "Restaurants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItemAllergens");

            migrationBuilder.DropTable(
                name: "MenuItemCharacteristics");

            migrationBuilder.DropTable(
                name: "MenuItemDietaryAttributes");

            migrationBuilder.DropTable(
                name: "MenuItemNiches");

            migrationBuilder.DropTable(
                name: "RestaurantHours");

            migrationBuilder.DropTable(
                name: "Allergens");

            migrationBuilder.DropTable(
                name: "Characteristics");

            migrationBuilder.DropTable(
                name: "DietaryPreferences");

            migrationBuilder.DropTable(
                name: "FoodNiches");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Cuisines");

            migrationBuilder.DropTable(
                name: "Restaurants");
        }
    }
}
