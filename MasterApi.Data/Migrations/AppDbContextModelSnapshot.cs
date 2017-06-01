using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MasterApi.Data.EF7;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Auth.Enums;
using MasterApi.Core.Enums;

namespace MasterApi.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MasterApi.Core.Account.Models.ExternalLogin", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128);

                    b.Property<int>("UserId");

                    b.Property<DateTimeOffset>("LastLogin");

                    b.HasKey("LoginProvider", "ProviderKey", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ExternalLogin");
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.PasswordResetSecret", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Answer");

                    b.Property<Guid>("Guid");

                    b.Property<string>("Question");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("PasswordResetSecret");
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.TwoFactorAuthToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Issued");

                    b.Property<string>("Token");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("TwoFactorAuthToken");
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.UserAccount", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AccountClosed");

                    b.Property<int>("AccountTwoFactorAuthMode");

                    b.Property<DateTimeOffset>("Created")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<int>("CurrentTwoFactorAuthStatus");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254);

                    b.Property<int>("FailedLoginCount");

                    b.Property<int>("FailedPasswordResetCount");

                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("HashedPassword")
                        .HasMaxLength(200);

                    b.Property<bool>("IsAccountClosed");

                    b.Property<bool>("IsAccountVerified");

                    b.Property<bool>("IsLoginAllowed");

                    b.Property<DateTimeOffset?>("LastFailedLogin");

                    b.Property<DateTimeOffset?>("LastFailedPasswordReset");

                    b.Property<DateTimeOffset?>("LastLogin");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("MobileCode")
                        .HasMaxLength(100);

                    b.Property<DateTimeOffset?>("MobileCodeSent");

                    b.Property<string>("MobilePhoneNumber")
                        .HasMaxLength(20);

                    b.Property<DateTimeOffset?>("MobilePhoneNumberChanged");

                    b.Property<DateTimeOffset?>("PasswordChanged");

                    b.Property<bool>("RequiresPasswordReset");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(254);

                    b.Property<string>("VerificationKey")
                        .HasMaxLength(100);

                    b.Property<DateTimeOffset?>("VerificationKeySent");

                    b.Property<int?>("VerificationPurpose");

                    b.Property<string>("VerificationStorage")
                        .HasMaxLength(100);

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("MobilePhoneNumber")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("UserAccount");
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.UserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(400);

                    b.Property<int>("UserId");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(400);

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaim");
                });

            modelBuilder.Entity("MasterApi.Core.Auth.Models.Audience", b =>
                {
                    b.Property<string>("ClientId")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50);

                    b.Property<bool>("Active");

                    b.Property<string>("AllowedOrigin")
                        .HasMaxLength(100);

                    b.Property<int>("ApplicationType");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<int>("RefreshTokenLifeTime");

                    b.Property<string>("Secret")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("ClientId");

                    b.ToTable("Audience");
                });

            modelBuilder.Entity("MasterApi.Core.Auth.Models.RefreshToken", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<DateTime>("ExpiresUtc");

                    b.Property<DateTime>("IssuedUtc");

                    b.Property<string>("ProtectedTicket")
                        .IsRequired();

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("RefreshToken");
                });

            modelBuilder.Entity("MasterApi.Core.Models.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("DataTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("EntityId")
                        .HasMaxLength(32);

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<string>("Event")
                        .HasMaxLength(200);

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLog");
                });

            modelBuilder.Entity("MasterApi.Core.Models.Country", b =>
                {
                    b.Property<string>("Iso2")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(2)")
                        .HasMaxLength(2);

                    b.Property<string>("Capital")
                        .HasMaxLength(200);

                    b.Property<string>("Currency")
                        .HasColumnType("char(3)")
                        .HasMaxLength(3);

                    b.Property<string>("Iso3")
                        .IsRequired()
                        .HasColumnType("char(3)")
                        .HasMaxLength(3);

                    b.Property<string>("Latitude")
                        .HasMaxLength(20);

                    b.Property<string>("Longitude")
                        .HasMaxLength(20);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<int>("NumericCode");

                    b.Property<string>("OficialName")
                        .HasMaxLength(200);

                    b.Property<string>("PhoneCode")
                        .HasMaxLength(20);

                    b.Property<string>("PostalCodeFormat")
                        .HasMaxLength(200);

                    b.Property<string>("PostalCodeRegex")
                        .HasMaxLength(200);

                    b.HasKey("Iso2");

                    b.HasIndex("Iso2")
                        .IsUnique();

                    b.ToTable("Country");
                });

            modelBuilder.Entity("MasterApi.Core.Models.EnabledCountry", b =>
                {
                    b.Property<string>("Iso2")
                        .HasColumnType("char(2)")
                        .HasMaxLength(2);

                    b.Property<DateTimeOffset>("Created");

                    b.Property<bool>("Enabled");

                    b.Property<DateTimeOffset?>("Updated");

                    b.HasKey("Iso2");

                    b.ToTable("EnabledCountry");
                });

            modelBuilder.Entity("MasterApi.Core.Models.ExceptionLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("Created")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<int>("HResult");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(800);

                    b.Property<string>("Method")
                        .HasMaxLength(20);

                    b.Property<string>("RequestUri")
                        .HasMaxLength(200);

                    b.Property<string>("Source")
                        .HasMaxLength(400);

                    b.Property<string>("StackTrace");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ExceptionLog");
                });

            modelBuilder.Entity("MasterApi.Core.Models.Language", b =>
                {
                    b.Property<string>("Code")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Code");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("Language");
                });

            modelBuilder.Entity("MasterApi.Core.Models.LanguageCountry", b =>
                {
                    b.Property<string>("LanguageCode")
                        .HasMaxLength(20);

                    b.Property<string>("Iso2")
                        .HasColumnType("char(2)")
                        .HasMaxLength(2);

                    b.Property<bool>("Default");

                    b.HasKey("LanguageCode", "Iso2");

                    b.HasIndex("Iso2");

                    b.ToTable("LanguageCountry");
                });

            modelBuilder.Entity("MasterApi.Core.Models.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("Created")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("Message")
                        .HasMaxLength(200);

                    b.Property<int>("Type");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Notification");
                });

            modelBuilder.Entity("MasterApi.Core.Models.ProvinceState", b =>
                {
                    b.Property<string>("Iso2")
                        .HasColumnType("char(2)")
                        .HasMaxLength(2);

                    b.Property<string>("Code")
                        .HasColumnType("char(3)")
                        .HasMaxLength(3);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.HasKey("Iso2", "Code");

                    b.ToTable("ProvinceState");
                });

            modelBuilder.Entity("MasterApi.Core.Models.UserProfile", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<string>("Avatar")
                        .HasMaxLength(200);

                    b.Property<string>("City")
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset>("Created")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("DisplayName")
                        .HasMaxLength(80);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(40);

                    b.Property<string>("Iso2")
                        .HasColumnType("char(2)")
                        .HasMaxLength(2);

                    b.Property<string>("LastName")
                        .HasMaxLength(200);

                    b.Property<string>("ProvinceState")
                        .HasColumnType("char(3)")
                        .HasMaxLength(3);

                    b.Property<DateTimeOffset>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.HasKey("UserId");

                    b.HasIndex("Iso2", "ProvinceState");

                    b.ToTable("UserProfile");
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.ExternalLogin", b =>
                {
                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "User")
                        .WithMany("ExternalLoginCollection")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.PasswordResetSecret", b =>
                {
                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "User")
                        .WithMany("PasswordResetSecretCollection")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.TwoFactorAuthToken", b =>
                {
                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "User")
                        .WithMany("TwoFactorAuthTokenCollection")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Account.Models.UserClaim", b =>
                {
                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "User")
                        .WithMany("ClaimCollection")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Auth.Models.RefreshToken", b =>
                {
                    b.HasOne("MasterApi.Core.Auth.Models.Audience", "Audience")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("ClientId");
                });

            modelBuilder.Entity("MasterApi.Core.Models.AuditLog", b =>
                {
                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "User")
                        .WithMany("AuditLogs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Models.EnabledCountry", b =>
                {
                    b.HasOne("MasterApi.Core.Models.Country", "Country")
                        .WithOne("EnabledCountry")
                        .HasForeignKey("MasterApi.Core.Models.EnabledCountry", "Iso2");
                });

            modelBuilder.Entity("MasterApi.Core.Models.ExceptionLog", b =>
                {
                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "User")
                        .WithMany("ExceptionsRaised")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Models.LanguageCountry", b =>
                {
                    b.HasOne("MasterApi.Core.Models.Country", "Country")
                        .WithMany("Languages")
                        .HasForeignKey("Iso2")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MasterApi.Core.Models.Language", "Language")
                        .WithMany("Countries")
                        .HasForeignKey("LanguageCode")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Models.Notification", b =>
                {
                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "User")
                        .WithMany("NotificationsSent")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Models.ProvinceState", b =>
                {
                    b.HasOne("MasterApi.Core.Models.Country", "Country")
                        .WithMany("ProvinceStates")
                        .HasForeignKey("Iso2")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MasterApi.Core.Models.UserProfile", b =>
                {
                    b.HasOne("MasterApi.Core.Models.Country")
                        .WithMany("UserResidences")
                        .HasForeignKey("Iso2");

                    b.HasOne("MasterApi.Core.Account.Models.UserAccount", "UserAccount")
                        .WithOne("Profile")
                        .HasForeignKey("MasterApi.Core.Models.UserProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MasterApi.Core.Models.ProvinceState", "ResidenceProvinceState")
                        .WithMany("UserProfiles")
                        .HasForeignKey("Iso2", "ProvinceState");
                });
        }
    }
}
