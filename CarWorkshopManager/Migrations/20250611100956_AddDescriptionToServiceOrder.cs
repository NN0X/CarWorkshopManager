﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManager.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToServiceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ServiceOrders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ServiceOrders");
        }
    }
}
