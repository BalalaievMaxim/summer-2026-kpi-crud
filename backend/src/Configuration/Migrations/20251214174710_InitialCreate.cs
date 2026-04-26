using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymManagement.Configuration.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classtype",
                columns: table => new
                {
                    class_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("classtype_pkey", x => x.class_type_id);
                });

            migrationBuilder.CreateTable(
                name: "client",
                columns: table => new
                {
                    client_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("client_pkey", x => x.client_id);
                });

            migrationBuilder.CreateTable(
                name: "coach",
                columns: table => new
                {
                    coach_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("coach_pkey", x => x.coach_id);
                });

            migrationBuilder.CreateTable(
                name: "facilityzone",
                columns: table => new
                {
                    zone_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("facilityzone_pkey", x => x.zone_id);
                });

            migrationBuilder.CreateTable(
                name: "membershipplan",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    duration_months = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("membershipplan_pkey", x => x.plan_id);
                });

            migrationBuilder.CreateTable(
                name: "invoice",
                columns: table => new
                {
                    invoice_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying"),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("invoice_pkey", x => x.invoice_id);
                    table.ForeignKey(
                        name: "invoice_client_id_fkey",
                        column: x => x.client_id,
                        principalTable: "client",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "class",
                columns: table => new
                {
                    class_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    class_type_id = table.Column<int>(type: "integer", nullable: false),
                    coach_id = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("class_pkey", x => x.class_id);
                    table.ForeignKey(
                        name: "class_class_type_id_fkey",
                        column: x => x.class_type_id,
                        principalTable: "classtype",
                        principalColumn: "class_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "class_coach_id_fkey",
                        column: x => x.coach_id,
                        principalTable: "coach",
                        principalColumn: "coach_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membership",
                columns: table => new
                {
                    membership_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    plan_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("membership_pkey", x => x.membership_id);
                    table.ForeignKey(
                        name: "membership_client_id_fkey",
                        column: x => x.client_id,
                        principalTable: "client",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "membership_plan_id_fkey",
                        column: x => x.plan_id,
                        principalTable: "membershipplan",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planaccess",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "integer", nullable: false),
                    zone_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("planaccess_pkey", x => new { x.plan_id, x.zone_id });
                    table.ForeignKey(
                        name: "planaccess_plan_id_fkey",
                        column: x => x.plan_id,
                        principalTable: "membershipplan",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "planaccess_zone_id_fkey",
                        column: x => x.zone_id,
                        principalTable: "facilityzone",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollment",
                columns: table => new
                {
                    enrollment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_id = table.Column<int>(type: "integer", nullable: false),
                    class_id = table.Column<int>(type: "integer", nullable: false),
                    registration_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("enrollment_pkey", x => x.enrollment_id);
                    table.ForeignKey(
                        name: "enrollment_class_id_fkey",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "class_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "enrollment_client_id_fkey",
                        column: x => x.client_id,
                        principalTable: "client",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_class_type_id",
                table: "class",
                column: "class_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_coach_id",
                table: "class",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "classtype_name_key",
                table: "classtype",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "client_email_key",
                table: "client",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "client_phone_key",
                table: "client",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "coach_email_key",
                table: "coach",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "enrollment_client_id_class_id_key",
                table: "enrollment",
                columns: new[] { "client_id", "class_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_class_id",
                table: "enrollment",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "facilityzone_name_key",
                table: "facilityzone",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_client_id",
                table: "invoice",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_client_id",
                table: "membership",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_plan_id",
                table: "membership",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "membershipplan_name_key",
                table: "membershipplan",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planaccess_zone_id",
                table: "planaccess",
                column: "zone_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrollment");

            migrationBuilder.DropTable(
                name: "invoice");

            migrationBuilder.DropTable(
                name: "membership");

            migrationBuilder.DropTable(
                name: "planaccess");

            migrationBuilder.DropTable(
                name: "class");

            migrationBuilder.DropTable(
                name: "client");

            migrationBuilder.DropTable(
                name: "membershipplan");

            migrationBuilder.DropTable(
                name: "facilityzone");

            migrationBuilder.DropTable(
                name: "classtype");

            migrationBuilder.DropTable(
                name: "coach");
        }
    }
}
