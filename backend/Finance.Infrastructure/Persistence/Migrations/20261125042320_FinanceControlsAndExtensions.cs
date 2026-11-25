using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinanceControlsAndExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "ReconciliationCases",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceKey",
                table: "ImportLines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankAllocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReconciliationCaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BankTransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAllocation_BankTransactions_BankTransactionId",
                        column: x => x.BankTransactionId,
                        principalTable: "BankTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankAllocation_ReconciliationCases_ReconciliationCaseId",
                        column: x => x.ReconciliationCaseId,
                        principalTable: "ReconciliationCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinanceCostLayer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    Sku = table.Column<string>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    SourceReceiptId = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UsedQuantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReceivedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceCostLayer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceCostPolicy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    SkuGroup = table.Column<string>(type: "TEXT", nullable: false),
                    Period = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Method = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceCostPolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceExtensionAudit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Actor = table.Column<string>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceExtensionAudit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceOrderLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: false),
                    OrderId = table.Column<string>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    LegalEntity = table.Column<string>(type: "TEXT", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    Sku = table.Column<string>(type: "TEXT", nullable: false),
                    SkuGroup = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    ImportedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceOrderLine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatementAllocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReconciliationCaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImportLineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatementAllocation_ImportLines_ImportLineId",
                        column: x => x.ImportLineId,
                        principalTable: "ImportLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StatementAllocation_ReconciliationCases_ReconciliationCaseId",
                        column: x => x.ReconciliationCaseId,
                        principalTable: "ReconciliationCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TolerancePolicy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TolerancePolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceDocumentRevision",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FinanceDocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Sha256 = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ReviewedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ReviewNote = table.Column<string>(type: "TEXT", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceDocumentRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceDocumentRevision_FinanceDocument_FinanceDocumentId",
                        column: x => x.FinanceDocumentId,
                        principalTable: "FinanceDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinanceCostAssessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderLineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Adjustment = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReplacesAssessmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PolicyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    AssessedBy = table.Column<string>(type: "TEXT", nullable: false),
                    PostedOn = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceCostAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceCostAssessment_FinanceOrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "FinanceOrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefundConfirmation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderLineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShopCode = table.Column<string>(type: "TEXT", nullable: false),
                    SourceRefundId = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    ConfirmedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    ConfirmedBy = table.Column<string>(type: "TEXT", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundConfirmation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundConfirmation_FinanceOrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "FinanceOrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundConfirmation_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinanceCostAllocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FinanceCostAssessmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CostLayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceCostAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceCostAllocation_FinanceCostAssessment_FinanceCostAssessmentId",
                        column: x => x.FinanceCostAssessmentId,
                        principalTable: "FinanceCostAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceCostAllocation_FinanceCostLayer_CostLayerId",
                        column: x => x.CostLayerId,
                        principalTable: "FinanceCostLayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationCases_PayoutId",
                table: "ReconciliationCases",
                column: "PayoutId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ShopCode_ReferenceType_ReferenceId",
                table: "JournalEntries",
                columns: new[] { "ShopCode", "ReferenceType", "ReferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportLines_SourceKey",
                table: "ImportLines",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAllocation_BankTransactionId",
                table: "BankAllocation",
                column: "BankTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAllocation_ReconciliationCaseId",
                table: "BankAllocation",
                column: "ReconciliationCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceCostAllocation_CostLayerId",
                table: "FinanceCostAllocation",
                column: "CostLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceCostAllocation_FinanceCostAssessmentId",
                table: "FinanceCostAllocation",
                column: "FinanceCostAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceCostAssessment_OrderLineId_Status",
                table: "FinanceCostAssessment",
                columns: new[] { "OrderLineId", "Status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceCostLayer_ShopCode_Sku_SourceReceiptId",
                table: "FinanceCostLayer",
                columns: new[] { "ShopCode", "Sku", "SourceReceiptId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceCostPolicy_ShopCode_SkuGroup_Period",
                table: "FinanceCostPolicy",
                columns: new[] { "ShopCode", "SkuGroup", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceDocument_ShopCode_ExternalId",
                table: "FinanceDocument",
                columns: new[] { "ShopCode", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceDocumentRevision_FinanceDocumentId_Version",
                table: "FinanceDocumentRevision",
                columns: new[] { "FinanceDocumentId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceOrderLine_ShopCode_ExternalId",
                table: "FinanceOrderLine",
                columns: new[] { "ShopCode", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefundConfirmation_JournalEntryId",
                table: "RefundConfirmation",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundConfirmation_OrderLineId",
                table: "RefundConfirmation",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundConfirmation_ShopCode_SourceRefundId",
                table: "RefundConfirmation",
                columns: new[] { "ShopCode", "SourceRefundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatementAllocation_ImportLineId",
                table: "StatementAllocation",
                column: "ImportLineId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementAllocation_ReconciliationCaseId",
                table: "StatementAllocation",
                column: "ReconciliationCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAllocation");

            migrationBuilder.DropTable(
                name: "FinanceCostAllocation");

            migrationBuilder.DropTable(
                name: "FinanceCostPolicy");

            migrationBuilder.DropTable(
                name: "FinanceDocumentRevision");

            migrationBuilder.DropTable(
                name: "FinanceExtensionAudit");

            migrationBuilder.DropTable(
                name: "RefundConfirmation");

            migrationBuilder.DropTable(
                name: "StatementAllocation");

            migrationBuilder.DropTable(
                name: "TolerancePolicy");

            migrationBuilder.DropTable(
                name: "FinanceCostAssessment");

            migrationBuilder.DropTable(
                name: "FinanceCostLayer");

            migrationBuilder.DropTable(
                name: "FinanceDocument");

            migrationBuilder.DropTable(
                name: "FinanceOrderLine");

            migrationBuilder.DropIndex(
                name: "IX_ReconciliationCases_PayoutId",
                table: "ReconciliationCases");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_ShopCode_ReferenceType_ReferenceId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_ImportLines_SourceKey",
                table: "ImportLines");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "ReconciliationCases");

            migrationBuilder.DropColumn(
                name: "SourceKey",
                table: "ImportLines");
        }
    }
}
