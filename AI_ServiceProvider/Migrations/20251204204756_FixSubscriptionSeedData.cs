using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AI_ServiceProvider.Migrations
{
    /// <inheritdoc />
    public partial class FixSubscriptionSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("0427efef-6a75-4a57-ad2d-979739c63e82"));

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("865ef8e5-67b6-43b8-935b-d62bb800feb9"));

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("ecf4e40a-f8fa-48a0-b232-15b20a5c2c04"));

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "BillingCycle", "CreatedAt", "MaxUsagePerMonth", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "monthly", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "Free Tier", 0m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "monthly", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 500, "Pro Plan", 29.99m },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "monthly", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), -1, "Enterprise", 199.99m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "BillingCycle", "CreatedAt", "MaxUsagePerMonth", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("0427efef-6a75-4a57-ad2d-979739c63e82"), "monthly", new DateTime(2025, 12, 4, 20, 36, 31, 600, DateTimeKind.Utc).AddTicks(221), -1, "Enterprise", 199.99m },
                    { new Guid("865ef8e5-67b6-43b8-935b-d62bb800feb9"), "monthly", new DateTime(2025, 12, 4, 20, 36, 31, 599, DateTimeKind.Utc).AddTicks(9577), 10, "Free Tier", 0m },
                    { new Guid("ecf4e40a-f8fa-48a0-b232-15b20a5c2c04"), "monthly", new DateTime(2025, 12, 4, 20, 36, 31, 600, DateTimeKind.Utc).AddTicks(215), 500, "Pro Plan", 29.99m }
                });
        }
    }
}
