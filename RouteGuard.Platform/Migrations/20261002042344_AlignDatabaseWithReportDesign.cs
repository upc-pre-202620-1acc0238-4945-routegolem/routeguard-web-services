using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteGuard.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AlignDatabaseWithReportDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at",
                table: "trips",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "trips",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "trips",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "notifications",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "data_payload",
                table: "notifications",
                type: "json",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "device_platform",
                table: "notifications",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "dispatched_at",
                table: "notifications",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "notifications",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "priority_level",
                table: "notifications",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "recipient_token",
                table: "notifications",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "notifications",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "notifications",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "device_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    token = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    platform = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    registered_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_device_tokens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_device_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "geofence_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    trip_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    student_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    notification_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    alert_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    latitude = table.Column<double>(type: "double", precision: 10, scale: 7, nullable: false),
                    longitude = table.Column<double>(type: "double", precision: 10, scale: 7, nullable: false),
                    triggered_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_geofence_alerts", x => x.id);
                    table.ForeignKey(
                        name: "f_k_geofence_alerts_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "location_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    trip_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    latitude = table.Column<double>(type: "double", nullable: false),
                    longitude = table.Column<double>(type: "double", nullable: false),
                    speed_kmh = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: false),
                    battery_level = table.Column<int>(type: "int", nullable: false),
                    heading = table.Column<double>(type: "double", precision: 10, scale: 2, nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_location_records", x => x.id);
                    table.ForeignKey(
                        name: "f_k_location_records_trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    locale = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    title_template = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    body_template = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_notification_templates", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "offline_sync_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    trip_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    synced_records_count = table.Column<int>(type: "int", nullable: false),
                    raw_payload = table.Column<string>(type: "json", nullable: false),
                    synced_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_offline_sync_batches", x => x.id);
                    table.ForeignKey(
                        name: "f_k_offline_sync_batches_trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "waypoints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    trip_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    student_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    address = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    latitude = table.Column<double>(type: "double", nullable: false),
                    longitude = table.Column<double>(type: "double", nullable: false),
                    visited_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_waypoints", x => x.id);
                    table.ForeignKey(
                        name: "f_k_waypoints_trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_device_tokens_token",
                table: "device_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_device_tokens_user_id",
                table: "device_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_geofence_alerts_notification_id",
                table: "geofence_alerts",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "i_x_location_records_trip_id_recorded_at",
                table: "location_records",
                columns: new[] { "trip_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "i_x_notification_templates_type_locale",
                table: "notification_templates",
                columns: new[] { "type", "locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_offline_sync_batches_trip_id",
                table: "offline_sync_batches",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "i_x_waypoints_trip_id_order_index",
                table: "waypoints",
                columns: new[] { "trip_id", "order_index" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_tokens");

            migrationBuilder.DropTable(
                name: "geofence_alerts");

            migrationBuilder.DropTable(
                name: "location_records");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "offline_sync_batches");

            migrationBuilder.DropTable(
                name: "waypoints");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "data_payload",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "device_platform",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "dispatched_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "priority_level",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "recipient_token",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "title",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "notifications");
        }
    }
}
