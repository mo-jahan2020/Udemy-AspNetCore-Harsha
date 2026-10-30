using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SabtAhval.Migrations
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "allowedIps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ApiClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allowedIps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "targetApiServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_targetApiServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "apiClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApiKeyHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", maxLength: 25, nullable: false),
                    TargetApiServerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apiClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_apiClients_targetApiServers_TargetApiServerId",
                        column: x => x.TargetApiServerId,
                        principalTable: "targetApiServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apiRequestLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiClientId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<short>(type: "smallint", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apiRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_apiRequestLogs_apiClients_ApiClientId",
                        column: x => x.ApiClientId,
                        principalTable: "apiClients",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "allowedIps",
                columns: new[] { "Id", "ApiClientId", "IpAddress", "IsActive" },
                values: new object[,]
                {
                    { 1, 1, "127.0.0.1", true },
                    { 2, 1, "::1", true }
                });

            migrationBuilder.InsertData(
                table: "targetApiServers",
                columns: new[] { "Id", "BaseUrl", "CreatedAt", "Description", "IsActive", "Name", "TimeoutSeconds" },
                values: new object[] { 1, "https://jsonplaceholder.typicode.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "jsonplaceholder typicode", true, "typicode", 30 });

            migrationBuilder.InsertData(
                table: "apiClients",
                columns: new[] { "Id", "ApiKeyHash", "CreatedAt", "IsActive", "TargetApiServerId", "Username" },
                values: new object[] { 1, "8ca85f2cf2f33d6efe51c890049fca0969ea10500a9f26b442b91eede06fbe9d", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1, "240" });

            migrationBuilder.CreateIndex(
                name: "IX_apiClients_TargetApiServerId",
                table: "apiClients",
                column: "TargetApiServerId");

            migrationBuilder.CreateIndex(
                name: "IX_apiClients_Username",
                table: "apiClients",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_apiRequestLogs_ApiClientId",
                table: "apiRequestLogs",
                column: "ApiClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allowedIps");

            migrationBuilder.DropTable(
                name: "apiRequestLogs");

            migrationBuilder.DropTable(
                name: "apiClients");

            migrationBuilder.DropTable(
                name: "targetApiServers");
        }
    }
}
