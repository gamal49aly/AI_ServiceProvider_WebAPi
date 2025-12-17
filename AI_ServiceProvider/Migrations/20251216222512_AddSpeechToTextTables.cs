using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ServiceProvider.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeechToTextTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpeechToTextInputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AudioData = table.Column<byte[]>(type: "varbinary(max)", maxLength: 20971520, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeechToTextInputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeechToTextInputs_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextToSpeechInputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InputText = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    VoiceSettings = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextToSpeechInputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextToSpeechInputs_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeechToTextOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InputId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TranscribedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeechToTextOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeechToTextOutputs_SpeechToTextInputs_InputId",
                        column: x => x.InputId,
                        principalTable: "SpeechToTextInputs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextToSpeechOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InputId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AudioData = table.Column<byte[]>(type: "varbinary(500)", maxLength: 500, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextToSpeechOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextToSpeechOutputs_TextToSpeechInputs_InputId",
                        column: x => x.InputId,
                        principalTable: "TextToSpeechInputs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpeechToTextInputs_ChatId",
                table: "SpeechToTextInputs",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeechToTextOutputs_InputId",
                table: "SpeechToTextOutputs",
                column: "InputId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TextToSpeechInputs_ChatId",
                table: "TextToSpeechInputs",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TextToSpeechOutputs_InputId",
                table: "TextToSpeechOutputs",
                column: "InputId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeechToTextOutputs");

            migrationBuilder.DropTable(
                name: "TextToSpeechOutputs");

            migrationBuilder.DropTable(
                name: "SpeechToTextInputs");

            migrationBuilder.DropTable(
                name: "TextToSpeechInputs");
        }
    }
}
