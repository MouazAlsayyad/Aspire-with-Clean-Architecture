using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireApp.ApiService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreationTime",
                table: "Users",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsEmailConfirmed",
                table: "Users",
                column: "IsEmailConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastModificationTime",
                table: "Users",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_CreationTime",
                table: "UserRoles",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_CreationTime",
                table: "UserPermissions",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreationTime",
                table: "Roles",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_LastModificationTime",
                table: "Roles",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Type",
                table: "Roles",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CreationTime",
                table: "RolePermissions",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CreationTime",
                table: "RefreshTokens",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_IsRevoked",
                table: "RefreshTokens",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Action",
                table: "Permissions",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_CreationTime",
                table: "Permissions",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_LastModificationTime",
                table: "Permissions",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource",
                table: "Permissions",
                column: "Resource");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource_Action",
                table: "Permissions",
                columns: new[] { "Resource", "Action" });

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreationTime",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsEmailConfirmed",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastModificationTime",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_CreationTime",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_CreationTime",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_Roles_CreationTime",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_LastModificationTime",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Type",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CreationTime",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_CreationTime",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_IsRevoked",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Action",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_CreationTime",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_LastModificationTime",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Resource",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Resource_Action",
                table: "Permissions");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
