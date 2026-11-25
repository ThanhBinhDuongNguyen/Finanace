using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinanceWorkflowCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedRequestId",
                table: "RefundConfirmation",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinanceFailedDeliveryFee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    SourceFeeId = table.Column<string>(type: "TEXT", nullable: false),
                    PackageId = table.Column<string>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    RecordedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ImportLineId = table.Column<Guid>(type: "TEXT", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceFailedDeliveryFee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceRefundRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    SourceRequestId = table.Column<string>(type: "TEXT", nullable: false),
                    OrderLineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    RequestedBy = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovalEvidence = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceRefundRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceRefundRequest_FinanceOrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "FinanceOrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinanceReturnCostAdjustment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    SourceQcId = table.Column<string>(type: "TEXT", nullable: false),
                    OrderLineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FinalAssessmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Disposition = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PostedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    RecordedBy = table.Column<string>(type: "TEXT", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceReturnCostAdjustment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceReturnCostAdjustment_FinanceCostAssessment_FinalAssessmentId",
                        column: x => x.FinalAssessmentId,
                        principalTable: "FinanceCostAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceReturnCostAdjustment_FinanceOrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "FinanceOrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodReopenRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    Period = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ProposedBy = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodReopenRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceAuthority",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceAuthority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceSwitchRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    TargetMode = table.Column<string>(type: "TEXT", nullable: false),
                    ExpectedVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    ProposedBy = table.Column<string>(type: "TEXT", nullable: false),
                    Checkpoint = table.Column<string>(type: "TEXT", nullable: false),
                    MappingManifest = table.Column<string>(type: "TEXT", nullable: false),
                    OpenWorkManifest = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceSwitchRequest", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceFailedDeliveryFee_ShopCode_SourceFeeId",
                table: "FinanceFailedDeliveryFee",
                columns: new[] { "ShopCode", "SourceFeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceRefundRequest_OrderLineId",
                table: "FinanceRefundRequest",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceRefundRequest_ShopCode_SourceRequestId",
                table: "FinanceRefundRequest",
                columns: new[] { "ShopCode", "SourceRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceReturnCostAdjustment_FinalAssessmentId",
                table: "FinanceReturnCostAdjustment",
                column: "FinalAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceReturnCostAdjustment_OrderLineId",
                table: "FinanceReturnCostAdjustment",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceReturnCostAdjustment_ShopCode_SourceQcId",
                table: "FinanceReturnCostAdjustment",
                columns: new[] { "ShopCode", "SourceQcId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceAuthority_ShopCode",
                table: "SourceAuthority",
                column: "ShopCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinanceFailedDeliveryFee");

            migrationBuilder.DropTable(
                name: "FinanceRefundRequest");

            migrationBuilder.DropTable(
                name: "FinanceReturnCostAdjustment");

            migrationBuilder.DropTable(
                name: "PeriodReopenRequest");

            migrationBuilder.DropTable(
                name: "SourceAuthority");

            migrationBuilder.DropTable(
                name: "SourceSwitchRequest");

            migrationBuilder.DropColumn(
                name: "ApprovedRequestId",
                table: "RefundConfirmation");
        }
    }
}
