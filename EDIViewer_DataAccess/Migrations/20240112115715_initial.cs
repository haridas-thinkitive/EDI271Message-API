using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDIViewer_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    EncounterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.EncounterId);
                });

            migrationBuilder.CreateTable(
                name: "testMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubMessageData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestMessageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubMessageData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubMessageData_testMessages_TestMessageId",
                        column: x => x.TestMessageId,
                        principalTable: "testMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MessageListData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FreeMessageText_01 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Segment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoPayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncounterID = table.Column<int>(type: "int", nullable: false),
                    Logmessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubMessageDataId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageListData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageListData_SubMessageData_SubMessageDataId",
                        column: x => x.SubMessageDataId,
                        principalTable: "SubMessageData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageListData_SubMessageDataId",
                table: "MessageListData",
                column: "SubMessageDataId");

            migrationBuilder.CreateIndex(
                name: "IX_SubMessageData_TestMessageId",
                table: "SubMessageData",
                column: "TestMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageListData");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "SubMessageData");

            migrationBuilder.DropTable(
                name: "testMessages");
        }
    }
}
