using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabtAhval.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
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
                name: "apiClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApiKeyHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    TargetApiServerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apiClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "apiRequestLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ApiClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apiRequestLogs", x => x.Id);
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
                    CreatedAt = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_targetApiServers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "allowedIps",
                columns: new[] { "Id", "ApiClientId", "IpAddress", "IsActive" },
                values: new object[] { 1, 1, "127.0.0.1", true });

            migrationBuilder.InsertData(
                table: "apiClients",
                columns: new[] { "Id", "ApiKeyHash", "CreatedAt", "IsActive", "TargetApiServerId", "Username" },
                values: new object[] { 1, "8ca85f2cf2f33d6efe51c890049fca0969ea10500a9f26b442b91eede06fbe9d", "11/10/1404 12:00:00 ق.ظ", true, 1, "240" });

            migrationBuilder.InsertData(
                table: "targetApiServers",
                columns: new[] { "Id", "BaseUrl", "CreatedAt", "Description", "IsActive", "Name", "TimeoutSeconds" },
                values: new object[] { 1, "https://jsonplaceholder.typicode.com", "11/10/1404 12:00:00 ق.ظ", "jsonplaceholder typicode", true, "typicode", 30 });

            migrationBuilder.CreateIndex(
                name: "IX_apiClients_Username",
                table: "apiClients",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allowedIps");

            migrationBuilder.DropTable(
                name: "apiClients");

            migrationBuilder.DropTable(
                name: "apiRequestLogs");

            migrationBuilder.DropTable(
                name: "targetApiServers");
        }
    }
}
