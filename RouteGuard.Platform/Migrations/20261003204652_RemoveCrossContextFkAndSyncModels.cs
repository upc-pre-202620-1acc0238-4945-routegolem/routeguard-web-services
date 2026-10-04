using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteGuard.Platform.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCrossContextFkAndSyncModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_trips_routes_route_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "i_x_trips_route_id",
                table: "trips");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "users",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "i_x_trips_route_id",
                table: "trips",
                column: "route_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_trips_routes_route_id",
                table: "trips",
                column: "route_id",
                principalTable: "routes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
