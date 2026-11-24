using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassificationMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", nullable: false),
                    SourceBusinessType = table.Column<string>(type: "TEXT", nullable: false),
                    TargetCategory = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    EffectiveFrom = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificationMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassifiedTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImportLineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassificationMappingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ClassifiedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ClassifiedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassifiedTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassifiedTransactions_ImportLineId",
                table: "ClassifiedTransactions",
                column: "ImportLineId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassificationMappings");

            migrationBuilder.DropTable(
                name: "ClassifiedTransactions");
        }
    }
}
