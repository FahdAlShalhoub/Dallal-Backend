using System;
using System.Linq;
using System.Security.Claims;
using Dallal.Otps;
using Microsoft.EntityFrameworkCore.Migrations;
using Volo.Abp.Auditing;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;

#nullable disable

namespace Dallal.Migrations
{
    /// <inheritdoc />
    public partial class MoreDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Otps",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer"
            );
            MigrateEnums<OtpStatusEnum>(migrationBuilder, "Otps", "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Otps",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer"
            );
            MigrateEnums<OtpReasonEnum>(migrationBuilder, "Otps", "Reason");

            migrationBuilder.AddColumn<decimal>(
                name: "AreaInMetersSq",
                table: "Listings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<int>(
                name: "BathroomCount",
                table: "Listings",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "BedroomCount",
                table: "Listings",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "ListingType",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerContract",
                table: "Listings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerYear",
                table: "Listings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<string>(
                name: "PropertyType",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "RentalContractPeriod",
                table: "Listings",
                type: "text",
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "MultiTenancySide",
                table: "AbpPermissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint"
            );

            MigrateEnums<MultiTenancySides>(migrationBuilder, "AbpPermissions", "MultiTenancySide");

            migrationBuilder.AlterColumn<string>(
                name: "ChangeType",
                table: "AbpEntityChanges",
                type: "text",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint"
            );

            MigrateEnums<EntityChangeType>(migrationBuilder, "AbpEntityChanges", "ChangeType");

            migrationBuilder.AlterColumn<string>(
                name: "ValueType",
                table: "AbpClaimTypes",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer"
            );
            // MigrateEnums<ClaimValueType>(migrationBuilder, "AbpClaimTypes", "ValueType");
            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "AbpBackgroundJobs",
                type: "text",
                nullable: false,
                defaultValue: "Normal",
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldDefaultValue: (byte)15
            );
            MigrateEnums<BackgroundJobPriority>(migrationBuilder, "AbpBackgroundJobs", "Priority");
        }

        private static void MigrateEnums<T>(
            MigrationBuilder migrationBuilder,
            string tableName,
            string colName
        )
            where T : struct, Enum
        {
            foreach (var enumValue in Enum.GetValues<T>().Cast<T>().ToList())
                migrationBuilder.Sql(
                    $"UPDATE \"{tableName}\" SET \"{colName}\" = '{enumValue}' WHERE \"{colName}\" = '{Convert.ToInt32(enumValue)}'"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AreaInMetersSq", table: "Listings");

            migrationBuilder.DropColumn(name: "BathroomCount", table: "Listings");

            migrationBuilder.DropColumn(name: "BedroomCount", table: "Listings");

            migrationBuilder.DropColumn(name: "Currency", table: "Listings");

            migrationBuilder.DropColumn(name: "ListingType", table: "Listings");

            migrationBuilder.DropColumn(name: "PricePerContract", table: "Listings");

            migrationBuilder.DropColumn(name: "PricePerYear", table: "Listings");

            migrationBuilder.DropColumn(name: "PropertyType", table: "Listings");

            migrationBuilder.DropColumn(name: "RentalContractPeriod", table: "Listings");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Otps",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );
            MigrateEnums<OtpStatusEnum>(migrationBuilder, "Otps", "Status");

            migrationBuilder.AlterColumn<int>(
                name: "Reason",
                table: "Otps",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );
            MigrateEnums<OtpReasonEnum>(migrationBuilder, "Otps", "Reason");
            migrationBuilder.AlterColumn<byte>(
                name: "MultiTenancySide",
                table: "AbpPermissions",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            MigrateEnums<MultiTenancySides>(migrationBuilder, "AbpPermissions", "MultiTenancySide");
            migrationBuilder.AlterColumn<byte>(
                name: "ChangeType",
                table: "AbpEntityChanges",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            MigrateEnums<EntityChangeType>(migrationBuilder, "AbpEntityChanges", "ChangeType");
            migrationBuilder.AlterColumn<int>(
                name: "ValueType",
                table: "AbpClaimTypes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );
            // MigrateEnums<ClaimValueType>(migrationBuilder, "AbpClaimTypes", "ValueType");

            migrationBuilder.AlterColumn<byte>(
                name: "Priority",
                table: "AbpBackgroundJobs",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)15,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Normal"
            );
            MigrateEnums<BackgroundJobPriority>(migrationBuilder, "AbpBackgroundJobs", "Priority");
        }
    }
}
