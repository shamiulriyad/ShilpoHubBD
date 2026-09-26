using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourismContentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageCredit",
                table: "Villages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceLabel",
                table: "Villages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Villages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitTips",
                table: "Villages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                table: "LocalCuisines",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNationwide",
                table: "LocalCuisines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "LocalCuisines",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "LocalCuisines",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageCredit",
                table: "HeritagePlaces",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KnownFor",
                table: "HeritagePlaces",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "HeritagePlaces",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Districts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageCredit",
                table: "Districts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Districts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KnownFor",
                table: "Districts",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Districts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000008"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000009"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000010"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000011"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000012"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000013"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000014"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000015"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000016"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000017"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000018"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000019"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000020"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000021"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000022"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000023"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000024"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000025"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000026"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000027"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000028"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000029"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000030"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000031"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000032"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000033"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000034"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000035"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000036"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000037"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000038"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000039"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000040"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000041"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000042"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000043"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000044"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000045"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000046"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000047"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000048"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000049"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000050"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000051"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000052"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000053"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000054"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000055"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000056"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000057"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000058"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000059"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000060"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000061"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000062"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000063"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000064"),
                columns: new[] { "Description", "ImageCredit", "ImageUrl", "KnownFor", "SourceUrl" },
                values: new object[] { null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageCredit",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "SourceLabel",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "VisitTips",
                table: "Villages");

            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "LocalCuisines");

            migrationBuilder.DropColumn(
                name: "IsNationwide",
                table: "LocalCuisines");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "LocalCuisines");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "LocalCuisines");

            migrationBuilder.DropColumn(
                name: "ImageCredit",
                table: "HeritagePlaces");

            migrationBuilder.DropColumn(
                name: "KnownFor",
                table: "HeritagePlaces");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "HeritagePlaces");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "ImageCredit",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "KnownFor",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Districts");
        }
    }
}
