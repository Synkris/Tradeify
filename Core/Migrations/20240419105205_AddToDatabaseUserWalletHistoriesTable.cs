using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class AddToDatabaseUserWalletHistoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentForms_Packages_PackageId",
                table: "PaymentForms");

            migrationBuilder.AlterColumn<int>(
                name: "PackageId",
                table: "PaymentForms",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "NoOfTokensBought",
                table: "PaymentForms",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AGCWalletHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DateOfTransaction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AGCWalletHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AGCWalletHistories_PaymentForms_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PaymentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AGCWalletHistories_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinimumToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaximumToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tokenamount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MiningQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MiningDuration = table.Column<int>(type: "int", nullable: false),
                    ActivationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrantWalletHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DateOfTransaction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantWalletHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrantWalletHistories_PaymentForms_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PaymentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrantWalletHistories_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MasterImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PvWalletHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DateOfTransaction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvWalletHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PvWalletHistories_PaymentForms_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PaymentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PvWalletHistories_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegFeeGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantAmountPerUser = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GrantDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegFeeGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegFeeGrants_PaymentForms_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PaymentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGenerationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChildId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Generation = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatePaid = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BonusAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGenerationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGenerationLogs_AspNetUsers_ChildId",
                        column: x => x.ChildId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGenerationLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DateOfTransaction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletHistories_PaymentForms_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PaymentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WalletHistories_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGrantHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateEarned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountEarned = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegFeeGrantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGrantHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGrantHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGrantHistories_RegFeeGrants_RegFeeGrantId",
                        column: x => x.RegFeeGrantId,
                        principalTable: "RegFeeGrants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AGCWalletHistories_PaymentId",
                table: "AGCWalletHistories",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_AGCWalletHistories_WalletId",
                table: "AGCWalletHistories",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_GrantWalletHistories_PaymentId",
                table: "GrantWalletHistories",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_GrantWalletHistories_WalletId",
                table: "GrantWalletHistories",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PvWalletHistories_PaymentId",
                table: "PvWalletHistories",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PvWalletHistories_WalletId",
                table: "PvWalletHistories",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_RegFeeGrants_PaymentId",
                table: "RegFeeGrants",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGenerationLogs_ChildId",
                table: "UserGenerationLogs",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGenerationLogs_UserId",
                table: "UserGenerationLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGrantHistories_RegFeeGrantId",
                table: "UserGrantHistories",
                column: "RegFeeGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGrantHistories_UserId",
                table: "UserGrantHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletHistories_PaymentId",
                table: "WalletHistories",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletHistories_WalletId",
                table: "WalletHistories",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentForms_Packages_PackageId",
                table: "PaymentForms",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentForms_Packages_PackageId",
                table: "PaymentForms");

            migrationBuilder.DropTable(
                name: "AGCWalletHistories");

            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.DropTable(
                name: "GrantWalletHistories");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "PvWalletHistories");

            migrationBuilder.DropTable(
                name: "UserGenerationLogs");

            migrationBuilder.DropTable(
                name: "UserGrantHistories");

            migrationBuilder.DropTable(
                name: "WalletHistories");

            migrationBuilder.DropTable(
                name: "RegFeeGrants");

            migrationBuilder.DropColumn(
                name: "NoOfTokensBought",
                table: "PaymentForms");

            migrationBuilder.AlterColumn<int>(
                name: "PackageId",
                table: "PaymentForms",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentForms_Packages_PackageId",
                table: "PaymentForms",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
