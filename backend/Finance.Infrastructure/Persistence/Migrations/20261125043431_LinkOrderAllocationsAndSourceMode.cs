using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkOrderAllocationsAndSourceMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderLineId",
                table: "StatementAllocation",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceMode",
                table: "ImportBatches",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StatementAllocation_OrderLineId",
                table: "StatementAllocation",
                column: "OrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatementAllocation_FinanceOrderLine_OrderLineId",
                table: "StatementAllocation",
                column: "OrderLineId",
                principalTable: "FinanceOrderLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatementAllocation_FinanceOrderLine_OrderLineId",
                table: "StatementAllocation");

            migrationBuilder.DropIndex(
                name: "IX_StatementAllocation_OrderLineId",
                table: "StatementAllocation");

            migrationBuilder.DropColumn(
                name: "OrderLineId",
                table: "StatementAllocation");

            migrationBuilder.DropColumn(
                name: "SourceMode",
                table: "ImportBatches");
        }
    }
}
