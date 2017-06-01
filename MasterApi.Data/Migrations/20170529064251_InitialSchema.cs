using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MasterApi.Data.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccount",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountClosed = table.Column<DateTimeOffset>(nullable: true),
                    AccountTwoFactorAuthMode = table.Column<int>(nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CurrentTwoFactorAuthStatus = table.Column<int>(nullable: false),
                    Email = table.Column<string>(maxLength: 254, nullable: false),
                    FailedLoginCount = table.Column<int>(nullable: false),
                    FailedPasswordResetCount = table.Column<int>(nullable: false),
                    Guid = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    HashedPassword = table.Column<string>(maxLength: 200, nullable: true),
                    IsAccountClosed = table.Column<bool>(nullable: false),
                    IsAccountVerified = table.Column<bool>(nullable: false),
                    IsLoginAllowed = table.Column<bool>(nullable: false),
                    LastFailedLogin = table.Column<DateTimeOffset>(nullable: true),
                    LastFailedPasswordReset = table.Column<DateTimeOffset>(nullable: true),
                    LastLogin = table.Column<DateTimeOffset>(nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    MobileCode = table.Column<string>(maxLength: 100, nullable: true),
                    MobileCodeSent = table.Column<DateTimeOffset>(nullable: true),
                    MobilePhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    MobilePhoneNumberChanged = table.Column<DateTimeOffset>(nullable: true),
                    PasswordChanged = table.Column<DateTimeOffset>(nullable: true),
                    RequiresPasswordReset = table.Column<bool>(nullable: false),
                    Username = table.Column<string>(maxLength: 254, nullable: false),
                    VerificationKey = table.Column<string>(maxLength: 100, nullable: true),
                    VerificationKeySent = table.Column<DateTimeOffset>(nullable: true),
                    VerificationPurpose = table.Column<int>(nullable: true),
                    VerificationStorage = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccount", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Audience",
                columns: table => new
                {
                    ClientId = table.Column<string>(maxLength: 50, nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    AllowedOrigin = table.Column<string>(maxLength: 100, nullable: true),
                    ApplicationType = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    RefreshTokenLifeTime = table.Column<int>(nullable: false),
                    Secret = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audience", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Iso2 = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    Capital = table.Column<string>(maxLength: 200, nullable: true),
                    Currency = table.Column<string>(type: "char(3)", maxLength: 3, nullable: true),
                    Iso3 = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    Latitude = table.Column<string>(maxLength: 20, nullable: true),
                    Longitude = table.Column<string>(maxLength: 20, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    NumericCode = table.Column<int>(nullable: false),
                    OficialName = table.Column<string>(maxLength: 200, nullable: true),
                    PhoneCode = table.Column<string>(maxLength: 20, nullable: true),
                    PostalCodeFormat = table.Column<string>(maxLength: 200, nullable: true),
                    PostalCodeRegex = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Iso2);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLogin",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    LastLogin = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogin", x => new { x.LoginProvider, x.ProviderKey, x.UserId });
                    table.ForeignKey(
                        name: "FK_ExternalLogin_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetSecret",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Answer = table.Column<string>(nullable: true),
                    Guid = table.Column<Guid>(nullable: false),
                    Question = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetSecret", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetSecret_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwoFactorAuthToken",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Issued = table.Column<DateTime>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorAuthToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorAuthToken_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaim",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(maxLength: 400, nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaim_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DataTime = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    EntityId = table.Column<string>(maxLength: 32, nullable: true),
                    EntityType = table.Column<string>(maxLength: 100, nullable: false),
                    Event = table.Column<string>(maxLength: 200, nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    HResult = table.Column<int>(nullable: false),
                    Message = table.Column<string>(maxLength: 800, nullable: false),
                    Method = table.Column<string>(maxLength: 20, nullable: true),
                    RequestUri = table.Column<string>(maxLength: 200, nullable: true),
                    Source = table.Column<string>(maxLength: 400, nullable: true),
                    StackTrace = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionLog_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Message = table.Column<string>(maxLength: 200, nullable: true),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(maxLength: 50, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(nullable: false),
                    IssuedUtc = table.Column<DateTime>(nullable: false),
                    ProtectedTicket = table.Column<string>(nullable: false),
                    Subject = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Audience_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Audience",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnabledCountry",
                columns: table => new
                {
                    Iso2 = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Updated = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnabledCountry", x => x.Iso2);
                    table.ForeignKey(
                        name: "FK_EnabledCountry_Country_Iso2",
                        column: x => x.Iso2,
                        principalTable: "Country",
                        principalColumn: "Iso2",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProvinceState",
                columns: table => new
                {
                    Iso2 = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    Code = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvinceState", x => new { x.Iso2, x.Code });
                    table.ForeignKey(
                        name: "FK_ProvinceState_Country_Iso2",
                        column: x => x.Iso2,
                        principalTable: "Country",
                        principalColumn: "Iso2",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanguageCountry",
                columns: table => new
                {
                    LanguageCode = table.Column<string>(maxLength: 20, nullable: false),
                    Iso2 = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    Default = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageCountry", x => new { x.LanguageCode, x.Iso2 });
                    table.ForeignKey(
                        name: "FK_LanguageCountry_Country_Iso2",
                        column: x => x.Iso2,
                        principalTable: "Country",
                        principalColumn: "Iso2",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageCountry_Language_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Language",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfile",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    Avatar = table.Column<string>(maxLength: 200, nullable: true),
                    City = table.Column<string>(maxLength: 200, nullable: true),
                    Created = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    DisplayName = table.Column<string>(maxLength: 80, nullable: true),
                    FirstName = table.Column<string>(maxLength: 40, nullable: false),
                    Iso2 = table.Column<string>(type: "char(2)", maxLength: 2, nullable: true),
                    LastName = table.Column<string>(maxLength: 200, nullable: true),
                    ProvinceState = table.Column<string>(type: "char(3)", maxLength: 3, nullable: true),
                    Updated = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfile", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfile_Country_Iso2",
                        column: x => x.Iso2,
                        principalTable: "Country",
                        principalColumn: "Iso2",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfile_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProfile_ProvinceState_Iso2_ProvinceState",
                        columns: x => new { x.Iso2, x.ProvinceState },
                        principalTable: "ProvinceState",
                        principalColumns: new[] { "Iso2", "Code" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogin_UserId",
                table: "ExternalLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetSecret_UserId",
                table: "PasswordResetSecret",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuthToken_UserId",
                table: "TwoFactorAuthToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_Email",
                table: "UserAccount",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_MobilePhoneNumber",
                table: "UserAccount",
                column: "MobilePhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_Username",
                table: "UserAccount",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaim_UserId",
                table: "UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_ClientId",
                table: "RefreshToken",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Country_Iso2",
                table: "Country",
                column: "Iso2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionLog_UserId",
                table: "ExceptionLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Language_Code",
                table: "Language",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LanguageCountry_Iso2",
                table: "LanguageCountry",
                column: "Iso2");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_Iso2_ProvinceState",
                table: "UserProfile",
                columns: new[] { "Iso2", "ProvinceState" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLogin");

            migrationBuilder.DropTable(
                name: "PasswordResetSecret");

            migrationBuilder.DropTable(
                name: "TwoFactorAuthToken");

            migrationBuilder.DropTable(
                name: "UserClaim");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "EnabledCountry");

            migrationBuilder.DropTable(
                name: "ExceptionLog");

            migrationBuilder.DropTable(
                name: "LanguageCountry");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "UserProfile");

            migrationBuilder.DropTable(
                name: "Audience");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "UserAccount");

            migrationBuilder.DropTable(
                name: "ProvinceState");

            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
