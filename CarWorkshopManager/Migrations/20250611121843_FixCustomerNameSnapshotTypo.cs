using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManager.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerNameSnapshotTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerNameSnaphot", // stara, błędna nazwa
                table: "ServiceOrders",
                newName: "CustomerNameSnapshot" // nowa, poprawna nazwa
            );
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
