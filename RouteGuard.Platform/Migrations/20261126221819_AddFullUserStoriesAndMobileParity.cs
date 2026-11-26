using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteGuard.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddFullUserStoriesAndMobileParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "brand",
                table: "vehicles",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Mercedes-Benz");

            migrationBuilder.AddColumn<string>(
                name: "soat_expires_at",
                table: "vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "2026-12-15");

            migrationBuilder.AddColumn<string>(
                name: "soat_insurer",
                table: "vehicles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Rímac Seguros");

            migrationBuilder.AddColumn<string>(
                name: "soat_policy",
                table: "vehicles",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "SOAT-2026-88412");

            migrationBuilder.AddColumn<bool>(
                name: "soat_valid",
                table: "vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_data",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_language",
                table: "users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "es");

            migrationBuilder.AddColumn<string>(
                name: "license_category",
                table: "drivers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "A-IIIa");

            migrationBuilder.AddColumn<string>(
                name: "license_expires_at",
                table: "drivers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "2027-08-30");

            migrationBuilder.CreateTable(
                name: "absence_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    absence_date = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_absence_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "authorized_caregivers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    relationship = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    document_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    photo_data = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_authorized_caregivers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    sender_role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_chat_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operational_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    driver_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    expense_date = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_operational_expenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_password_reset_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pickup_change_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_date = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    new_address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    reason = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pickup_change_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "route_announcements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    message = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_route_announcements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "support_tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    category = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    subject = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_support_tickets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trip_ratings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_trip_ratings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_maintenances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    maintenance_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    workshop = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    odometer_km = table.Column<int>(type: "integer", nullable: false),
                    performed_at = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    next_due_date = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_vehicle_maintenances", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "absence_reports");

            migrationBuilder.DropTable(
                name: "authorized_caregivers");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "operational_expenses");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "pickup_change_requests");

            migrationBuilder.DropTable(
                name: "route_announcements");

            migrationBuilder.DropTable(
                name: "support_tickets");

            migrationBuilder.DropTable(
                name: "trip_ratings");

            migrationBuilder.DropTable(
                name: "vehicle_maintenances");

            migrationBuilder.DropColumn(
                name: "brand",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "soat_expires_at",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "soat_insurer",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "soat_policy",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "soat_valid",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "photo_data",
                table: "users");

            migrationBuilder.DropColumn(
                name: "preferred_language",
                table: "users");

            migrationBuilder.DropColumn(
                name: "license_category",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "license_expires_at",
                table: "drivers");
        }
    }
}
