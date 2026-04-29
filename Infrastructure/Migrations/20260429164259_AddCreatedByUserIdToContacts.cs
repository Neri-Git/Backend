using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserIdToContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Contact",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Contact",
                keyColumn: "Id",
                keyValue: new Guid("3d54091d-abc8-49ec-9590-93ad3ed5458f"),
                column: "CreatedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Contact",
                keyColumn: "Id",
                keyValue: new Guid("516a34d7-ccfb-4f20-85f3-62bd0f3af271"),
                column: "CreatedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Contact",
                keyColumn: "Id",
                keyValue: new Guid("b4dcb17c-f875-43f8-9d66-36597895a466"),
                column: "CreatedByUserId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Contact");
        }
    }
}
