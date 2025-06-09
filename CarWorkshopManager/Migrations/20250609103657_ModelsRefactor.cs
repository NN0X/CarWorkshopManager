using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManager.Migrations
{
    /// <inheritdoc />
    public partial class ModelsRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserServiceTask_ServiceTasks_ServiceTasksId",
                table: "ApplicationUserServiceTask");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_VatRates_VatRateId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Customers_CustomerId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_ServiceOrderId",
                table: "ServiceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTasks_VatRates_VatRateId",
                table: "ServiceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_UsedParts_ServiceTasks_ServiceTaskId",
                table: "UsedParts");

            migrationBuilder.DropForeignKey(
                name: "FK_UsedParts_VatRates_VatRateId",
                table: "UsedParts");

            migrationBuilder.DropIndex(
                name: "IX_UsedParts_VatRateId",
                table: "UsedParts");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTasks_VatRateId",
                table: "ServiceTasks");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_CustomerId",
                table: "ServiceOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationUserServiceTask",
                table: "ApplicationUserServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserServiceTask_ServiceTasksId",
                table: "ApplicationUserServiceTask");

            migrationBuilder.DropColumn(
                name: "VatRateId",
                table: "UsedParts");

            migrationBuilder.DropColumn(
                name: "VatRateId",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ServiceOrders");

            migrationBuilder.RenameColumn(
                name: "UnitPriceNet",
                table: "UsedParts",
                newName: "UnitPriceNetSnapshot");

            migrationBuilder.RenameColumn(
                name: "HourRateNet",
                table: "ServiceTasks",
                newName: "HourRateNetSnapshot");

            migrationBuilder.RenameColumn(
                name: "ServiceTasksId",
                table: "ApplicationUserServiceTask",
                newName: "AssignedTasksId");

            migrationBuilder.AddColumn<decimal>(
                name: "VatRateSnapshot",
                table: "UsedParts",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRateSnapshot",
                table: "ServiceTasks",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ServiceOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNameSnaphot",
                table: "ServiceOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumberSnapshot",
                table: "ServiceOrders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Parts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "OrderComments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationUserServiceTask",
                table: "ApplicationUserServiceTask",
                columns: new[] { "AssignedTasksId", "MechanicsId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ApplicationUserId",
                table: "ServiceOrders",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComments_ApplicationUserId",
                table: "OrderComments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserServiceTask_MechanicsId",
                table: "ApplicationUserServiceTask",
                column: "MechanicsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserServiceTask_ServiceTasks_AssignedTasksId",
                table: "ApplicationUserServiceTask",
                column: "AssignedTasksId",
                principalTable: "ServiceTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderComments_AspNetUsers_ApplicationUserId",
                table: "OrderComments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_VatRates_VatRateId",
                table: "Parts",
                column: "VatRateId",
                principalTable: "VatRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_AspNetUsers_ApplicationUserId",
                table: "ServiceOrders",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_ServiceOrderId",
                table: "ServiceTasks",
                column: "ServiceOrderId",
                principalTable: "ServiceOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UsedParts_ServiceTasks_ServiceTaskId",
                table: "UsedParts",
                column: "ServiceTaskId",
                principalTable: "ServiceTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserServiceTask_ServiceTasks_AssignedTasksId",
                table: "ApplicationUserServiceTask");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderComments_AspNetUsers_ApplicationUserId",
                table: "OrderComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_VatRates_VatRateId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_AspNetUsers_ApplicationUserId",
                table: "ServiceOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_ServiceOrderId",
                table: "ServiceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_UsedParts_ServiceTasks_ServiceTaskId",
                table: "UsedParts");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_ApplicationUserId",
                table: "ServiceOrders");

            migrationBuilder.DropIndex(
                name: "IX_OrderComments_ApplicationUserId",
                table: "OrderComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationUserServiceTask",
                table: "ApplicationUserServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserServiceTask_MechanicsId",
                table: "ApplicationUserServiceTask");

            migrationBuilder.DropColumn(
                name: "VatRateSnapshot",
                table: "UsedParts");

            migrationBuilder.DropColumn(
                name: "VatRateSnapshot",
                table: "ServiceTasks");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "CustomerNameSnaphot",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "RegistrationNumberSnapshot",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "OrderComments");

            migrationBuilder.RenameColumn(
                name: "UnitPriceNetSnapshot",
                table: "UsedParts",
                newName: "UnitPriceNet");

            migrationBuilder.RenameColumn(
                name: "HourRateNetSnapshot",
                table: "ServiceTasks",
                newName: "HourRateNet");

            migrationBuilder.RenameColumn(
                name: "AssignedTasksId",
                table: "ApplicationUserServiceTask",
                newName: "ServiceTasksId");

            migrationBuilder.AddColumn<int>(
                name: "VatRateId",
                table: "UsedParts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VatRateId",
                table: "ServiceTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "ServiceOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationUserServiceTask",
                table: "ApplicationUserServiceTask",
                columns: new[] { "MechanicsId", "ServiceTasksId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsedParts_VatRateId",
                table: "UsedParts",
                column: "VatRateId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTasks_VatRateId",
                table: "ServiceTasks",
                column: "VatRateId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_CustomerId",
                table: "ServiceOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserServiceTask_ServiceTasksId",
                table: "ApplicationUserServiceTask",
                column: "ServiceTasksId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserServiceTask_ServiceTasks_ServiceTasksId",
                table: "ApplicationUserServiceTask",
                column: "ServiceTasksId",
                principalTable: "ServiceTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_VatRates_VatRateId",
                table: "Parts",
                column: "VatRateId",
                principalTable: "VatRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Customers_CustomerId",
                table: "ServiceOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTasks_ServiceOrders_ServiceOrderId",
                table: "ServiceTasks",
                column: "ServiceOrderId",
                principalTable: "ServiceOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTasks_VatRates_VatRateId",
                table: "ServiceTasks",
                column: "VatRateId",
                principalTable: "VatRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UsedParts_ServiceTasks_ServiceTaskId",
                table: "UsedParts",
                column: "ServiceTaskId",
                principalTable: "ServiceTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsedParts_VatRates_VatRateId",
                table: "UsedParts",
                column: "VatRateId",
                principalTable: "VatRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
