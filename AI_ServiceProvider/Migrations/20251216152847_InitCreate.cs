using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ServiceProvider.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "StripePriceId", "StripeProductId" },
                values: new object[] { "price_1ScBw0PM8gQYkbOp2T1VaDW7", "prod_TZKbRasmUd0XZH" });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "BillingCycle", "StripePriceId", "StripeProductId" },
                values: new object[] { "yearly", "price_1ScByQPM8gQYkbOpcHC1WMnp", "prod_TZKdMmuEa6w1C3" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "StripePriceId", "StripeProductId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "BillingCycle", "StripePriceId", "StripeProductId" },
                values: new object[] { "monthly", null, null });
        }
    }
}
