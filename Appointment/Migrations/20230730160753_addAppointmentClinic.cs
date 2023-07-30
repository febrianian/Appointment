using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment.Migrations
{
    /// <inheritdoc />
    public partial class addAppointmentClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDoctorApproved",
                table: "AppointmentClinic");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "AppointmentClinic",
                newName: "UserModified");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "AppointmentClinic",
                newName: "DateModified");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "AppointmentClinic",
                newName: "UserIdPatient");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "AppointmentClinic",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "AppointmentClinic",
                newName: "IdSpesialis");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "AppointmentClinic",
                newName: "UserIdDoctor");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AppointmentClinic",
                newName: "UserCreated");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "AppointmentClinic",
                newName: "TimeAppointment");

            migrationBuilder.AddColumn<string>(
                name: "Age",
                table: "AppointmentClinic",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAppointment",
                table: "AppointmentClinic",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "AppointmentClinic",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HistoryOfSick",
                table: "AppointmentClinic",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IdStatus",
                table: "AppointmentClinic",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReasonOfSick",
                table: "AppointmentClinic",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AppointmentClinic",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "AppointmentClinic");

            migrationBuilder.DropColumn(
                name: "DateAppointment",
                table: "AppointmentClinic");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "AppointmentClinic");

            migrationBuilder.DropColumn(
                name: "HistoryOfSick",
                table: "AppointmentClinic");

            migrationBuilder.DropColumn(
                name: "IdStatus",
                table: "AppointmentClinic");

            migrationBuilder.DropColumn(
                name: "ReasonOfSick",
                table: "AppointmentClinic");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AppointmentClinic");

            migrationBuilder.RenameColumn(
                name: "UserModified",
                table: "AppointmentClinic",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "UserIdPatient",
                table: "AppointmentClinic",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "UserIdDoctor",
                table: "AppointmentClinic",
                newName: "DoctorId");

            migrationBuilder.RenameColumn(
                name: "UserCreated",
                table: "AppointmentClinic",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "TimeAppointment",
                table: "AppointmentClinic",
                newName: "AdminId");

            migrationBuilder.RenameColumn(
                name: "IdSpesialis",
                table: "AppointmentClinic",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "DateModified",
                table: "AppointmentClinic",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "AppointmentClinic",
                newName: "EndDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsDoctorApproved",
                table: "AppointmentClinic",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
