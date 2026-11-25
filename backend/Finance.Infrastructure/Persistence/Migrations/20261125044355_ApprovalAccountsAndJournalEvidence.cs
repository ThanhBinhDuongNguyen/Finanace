using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalAccountsAndJournalEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Evidence",
                table: "JournalEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "JournalEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PayoutAccountChange",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    SourceRequestId = table.Column<string>(type: "TEXT", nullable: false),
                    PreviousAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NewAccountReference = table.Column<string>(type: "TEXT", nullable: false),
                    IndependentChannelEvidence = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    ProposedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ProposedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ApprovalEvidence = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutAccountChange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayoutReceivingAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    AccountReference = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedChangeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DeactivatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutReceivingAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutReceivingAccount_PayoutAccountChange_ApprovedChangeId",
                        column: x => x.ApprovedChangeId,
                        principalTable: "PayoutAccountChange",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutAccountChange_ShopCode_SourceRequestId",
                table: "PayoutAccountChange",
                columns: new[] { "ShopCode", "SourceRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutReceivingAccount_ApprovedChangeId",
                table: "PayoutReceivingAccount",
                column: "ApprovedChangeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutReceivingAccount_ShopCode",
                table: "PayoutReceivingAccount",
                column: "ShopCode",
                unique: true,
                filter: "\"DeactivatedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutReceivingAccount");

            migrationBuilder.DropTable(
                name: "PayoutAccountChange");

            migrationBuilder.DropColumn(
                name: "Evidence",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "JournalEntries");
        }
    }
}
