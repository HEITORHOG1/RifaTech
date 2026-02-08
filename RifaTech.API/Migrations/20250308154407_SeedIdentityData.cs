using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RifaTech.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ID fixos para garantir consistência
            var adminRoleId = "1D213CFF-F1E7-4B1D-A0AB-89F30E6F6F72";
            var userRoleId = "9D49A7A0-E343-41E3-B2D8-B512097CCCB1";

            // Inserir role Admin
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[] { adminRoleId, "Admin", "ADMIN", Guid.NewGuid().ToString() }
            );

            // Inserir role User
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[] { userRoleId, "User", "USER", Guid.NewGuid().ToString() }
            );

            // ID fixo para o usuário admin
            var adminId = "22A8599E-41C5-4D3E-BD43-A8C26055AD89";

            // Usuário Admin predefinido
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] {
                    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                    "LockoutEnabled", "AccessFailedCount", "Name", "CPF", "EhAdmin"
                },
                values: new object[] {
                    adminId,
                    "admin@rifatech.com",
                    "ADMIN@RIFATECH.COM",
                    "admin@rifatech.com",
                    "ADMIN@RIFATECH.COM",
                    true, // EmailConfirmed
                    // Senha: Admin@123 - Este hash deve ser gerado com o ASP.NET Identity
                    "AQAAAAEAACcQAAAAEL0LJmH8Vx/fFrBvmVBRyomcQcMVfOAni0To62Ry2bxgk4pN9OWvV0xKy8UmGDovTQ==",
                    Guid.NewGuid().ToString(), // SecurityStamp
                    Guid.NewGuid().ToString(), // ConcurrencyStamp
                    "11999887766", // PhoneNumber
                    false, // PhoneNumberConfirmed
                    false, // TwoFactorEnabled
                    null, // LockoutEnd
                    false, // LockoutEnabled
                    0, // AccessFailedCount
                    "Administrador", // Name
                    "12345678900", // CPF
                    true // EhAdmin
                }
            );

            // Adicionar usuário à role Admin
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { adminId, adminRoleId }
            );

            // ID fixo para o usuário comum
            var userId = "FA39F1CC-F3F1-4CC1-9E70-9D63F9494A2A";

            // Usuário comum predefinido
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] {
                    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
                    "LockoutEnabled", "AccessFailedCount", "Name", "CPF", "EhAdmin"
                },
                values: new object[] {
                    userId,
                    "usuario@rifatech.com",
                    "USUARIO@RIFATECH.COM",
                    "usuario@rifatech.com",
                    "USUARIO@RIFATECH.COM",
                    true, // EmailConfirmed
                    // Senha: Usuario@123 - Este hash deve ser gerado com o ASP.NET Identity
                    "AQAAAAEAACcQAAAAEONHu+RKJXeNmP1XZZeKT73j6xGRIy14iC01mGfnTXkwsMjRLdwdXWIDfUaREkPDQA==",
                    Guid.NewGuid().ToString(), // SecurityStamp
                    Guid.NewGuid().ToString(), // ConcurrencyStamp
                    "11988776655", // PhoneNumber
                    false, // PhoneNumberConfirmed
                    false, // TwoFactorEnabled
                    null, // LockoutEnd
                    false, // LockoutEnabled
                    0, // AccessFailedCount
                    "Usuário Comum", // Name
                    "98765432100", // CPF
                    false // EhAdmin
                }
            );

            // Adicionar usuário à role User
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { userId, userRoleId }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover os usuários inseridos
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "22A8599E-41C5-4D3E-BD43-A8C26055AD89", "1D213CFF-F1E7-4B1D-A0AB-89F30E6F6F72" }
            );

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "FA39F1CC-F3F1-4CC1-9E70-9D63F9494A2A", "9D49A7A0-E343-41E3-B2D8-B512097CCCB1" }
            );

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22A8599E-41C5-4D3E-BD43-A8C26055AD89"
            );

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "FA39F1CC-F3F1-4CC1-9E70-9D63F9494A2A"
            );

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1D213CFF-F1E7-4B1D-A0AB-89F30E6F6F72"
            );

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9D49A7A0-E343-41E3-B2D8-B512097CCCB1"
            );
        }
    }
}