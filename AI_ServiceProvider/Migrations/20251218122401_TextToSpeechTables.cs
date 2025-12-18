using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ServiceProvider.Migrations
{
    /// <inheritdoc />
    public partial class TextToSpeechTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioData",
                table: "TextToSpeechOutputs");

            migrationBuilder.DropColumn(
                name: "VoiceSettings",
                table: "TextToSpeechInputs");

            migrationBuilder.AlterColumn<string>(
                name: "InputText",
                table: "TextToSpeechInputs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000);

            migrationBuilder.AddColumn<byte[]>(
                name: "AudioData",
                table: "TextToSpeechInputs",
                type: "varbinary(max)",
                maxLength: 1073741824,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "VoiceName",
                table: "TextToSpeechInputs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioData",
                table: "TextToSpeechInputs");

            migrationBuilder.DropColumn(
                name: "VoiceName",
                table: "TextToSpeechInputs");

            migrationBuilder.AddColumn<byte[]>(
                name: "AudioData",
                table: "TextToSpeechOutputs",
                type: "varbinary(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "InputText",
                table: "TextToSpeechInputs",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "VoiceSettings",
                table: "TextToSpeechInputs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
