using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ServiceProvider.Migrations
{
    /// <inheritdoc />
    public partial class MakeAudioDataNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "AudioData",
                table: "SpeechToTextInputs",
                type: "varbinary(max)",
                maxLength: 20971520,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldMaxLength: 20971520);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "AudioData",
                table: "SpeechToTextInputs",
                type: "varbinary(max)",
                maxLength: 20971520,
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldMaxLength: 20971520,
                oldNullable: true);
        }
    }
}
