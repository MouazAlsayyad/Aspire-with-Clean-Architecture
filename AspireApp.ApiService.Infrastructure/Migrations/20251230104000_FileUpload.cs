using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireApp.ApiService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FileUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileType = table.Column<int>(type: "int", nullable: false),
                    StorageType = table.Column<int>(type: "int", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploads", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_CreationTime",
                table: "FileUploads",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_FileName",
                table: "FileUploads",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_FileType",
                table: "FileUploads",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_Hash",
                table: "FileUploads",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_StorageType",
                table: "FileUploads",
                column: "StorageType");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_StorageType_FileType",
                table: "FileUploads",
                columns: new[] { "StorageType", "FileType" });

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_UploadedBy",
                table: "FileUploads",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_UploadedBy_CreationTime",
                table: "FileUploads",
                columns: new[] { "UploadedBy", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileUploads");
        }
    }
}
