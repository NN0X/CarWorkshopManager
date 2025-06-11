using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManager.Migrations
{
    /// <inheritdoc />
    public partial class OnDeleteActionMechanicsTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserServiceTask");

            migrationBuilder.CreateTable(
                name: "ServiceTaskMechanic",
                columns: table => new
                {
                    MechanicId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServiceTaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTaskMechanic", x => new { x.MechanicId, x.ServiceTaskId });
                    table.ForeignKey(
                        name: "FK_ServiceTaskMechanic_AspNetUsers_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceTaskMechanic_ServiceTasks_ServiceTaskId",
                        column: x => x.ServiceTaskId,
                        principalTable: "ServiceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTaskMechanic_ServiceTaskId",
                table: "ServiceTaskMechanic",
                column: "ServiceTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceTaskMechanic");

            migrationBuilder.CreateTable(
                name: "ApplicationUserServiceTask",
                columns: table => new
                {
                    AssignedTasksId = table.Column<int>(type: "int", nullable: false),
                    MechanicsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserServiceTask", x => new { x.AssignedTasksId, x.MechanicsId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserServiceTask_AspNetUsers_MechanicsId",
                        column: x => x.MechanicsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserServiceTask_ServiceTasks_AssignedTasksId",
                        column: x => x.AssignedTasksId,
                        principalTable: "ServiceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserServiceTask_MechanicsId",
                table: "ApplicationUserServiceTask",
                column: "MechanicsId");
        }
    }
}
