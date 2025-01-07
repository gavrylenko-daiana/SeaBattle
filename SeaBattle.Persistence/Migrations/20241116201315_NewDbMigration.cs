using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeaBattle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewDbMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoordinateTypes",
                columns: table => new
                {
                    CoordinateTypeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoordinateTypes", x => x.CoordinateTypeId);
                });

            migrationBuilder.CreateTable(
                name: "GameFields",
                columns: table => new
                {
                    GameFieldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoundaryCoordinate = table.Column<int>(type: "int", nullable: false),
                    FieldSize = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameFields", x => x.GameFieldId);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "Points",
                columns: table => new
                {
                    PointId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Points", x => x.PointId);
                });

            migrationBuilder.CreateTable(
                name: "ShipTypes",
                columns: table => new
                {
                    ShipTypeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipTypes", x => x.ShipTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    AppUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.AppUserId);
                });

            migrationBuilder.CreateTable(
                name: "Coordinates",
                columns: table => new
                {
                    CoordinateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PointId = table.Column<int>(type: "int", nullable: false),
                    Quadrant = table.Column<int>(type: "int", nullable: false),
                    CoordinateTypeId = table.Column<int>(type: "int", nullable: false),
                    GameFieldId = table.Column<int>(type: "int", nullable: false),
                    IsFirstCoordinate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinates", x => x.CoordinateId);
                    table.ForeignKey(
                        name: "FK_Coordinates_CoordinateTypes_CoordinateTypeId",
                        column: x => x.CoordinateTypeId,
                        principalTable: "CoordinateTypes",
                        principalColumn: "CoordinateTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coordinates_GameFields_GameFieldId",
                        column: x => x.GameFieldId,
                        principalTable: "GameFields",
                        principalColumn: "GameFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coordinates_Points_PointId",
                        column: x => x.PointId,
                        principalTable: "Points",
                        principalColumn: "PointId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                columns: table => new
                {
                    ShipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Range = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    ShipTypeId = table.Column<int>(type: "int", nullable: false),
                    Speed = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.ShipId);
                    table.ForeignKey(
                        name: "FK_Ships_ShipTypes_ShipTypeId",
                        column: x => x.ShipTypeId,
                        principalTable: "ShipTypes",
                        principalColumn: "ShipTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameInvitations",
                columns: table => new
                {
                    GameInvitationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameInvitations", x => x.GameInvitationId);
                    table.ForeignKey(
                        name: "FK_GameInvitations_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameInvitations_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "AppUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGames",
                columns: table => new
                {
                    UserGamesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    GameFieldId = table.Column<int>(type: "int", nullable: false),
                    IsReady = table.Column<bool>(type: "bit", nullable: false),
                    IsPlayerTurn = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGames", x => x.UserGamesId);
                    table.ForeignKey(
                        name: "FK_UserGames_GameFields_GameFieldId",
                        column: x => x.GameFieldId,
                        principalTable: "GameFields",
                        principalColumn: "GameFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGames_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "AppUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipCoordinates",
                columns: table => new
                {
                    ShipCoordinateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoordinateId = table.Column<int>(type: "int", nullable: false),
                    ShipId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipCoordinates", x => x.ShipCoordinateId);
                    table.ForeignKey(
                        name: "FK_ShipCoordinates_Coordinates_CoordinateId",
                        column: x => x.CoordinateId,
                        principalTable: "Coordinates",
                        principalColumn: "CoordinateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipCoordinates_Ships_ShipId",
                        column: x => x.ShipId,
                        principalTable: "Ships",
                        principalColumn: "ShipId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CoordinateTypes",
                columns: new[] { "CoordinateTypeId", "Type" },
                values: new object[,]
                {
                    { 1, "Empty" },
                    { 2, "Filled" },
                    { 3, "Hit" },
                    { 4, "Missed" },
                    { 5, "Destroyed" }
                });

            migrationBuilder.InsertData(
                table: "ShipTypes",
                columns: new[] { "ShipTypeId", "Type" },
                values: new object[,]
                {
                    { 1, "War" },
                    { 2, "Support" },
                    { 3, "Hybrid" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_CoordinateTypeId",
                table: "Coordinates",
                column: "CoordinateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_GameFieldId",
                table: "Coordinates",
                column: "GameFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinates_PointId",
                table: "Coordinates",
                column: "PointId");

            migrationBuilder.CreateIndex(
                name: "IX_GameInvitations_AppUserId",
                table: "GameInvitations",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameInvitations_GameId",
                table: "GameInvitations",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipCoordinates_CoordinateId",
                table: "ShipCoordinates",
                column: "CoordinateId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipCoordinates_ShipId",
                table: "ShipCoordinates",
                column: "ShipId");

            migrationBuilder.CreateIndex(
                name: "IX_Ships_ShipTypeId",
                table: "Ships",
                column: "ShipTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_AppUserId",
                table: "UserGames",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_GameFieldId",
                table: "UserGames",
                column: "GameFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_GameId",
                table: "UserGames",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameInvitations");

            migrationBuilder.DropTable(
                name: "ShipCoordinates");

            migrationBuilder.DropTable(
                name: "UserGames");

            migrationBuilder.DropTable(
                name: "Coordinates");

            migrationBuilder.DropTable(
                name: "Ships");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CoordinateTypes");

            migrationBuilder.DropTable(
                name: "GameFields");

            migrationBuilder.DropTable(
                name: "Points");

            migrationBuilder.DropTable(
                name: "ShipTypes");
        }
    }
}
