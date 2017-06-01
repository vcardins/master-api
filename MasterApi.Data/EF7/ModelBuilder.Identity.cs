using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MasterApi.Core.Account.Models;

namespace MasterApi.Data.EF7
{
    public static partial class ModelBuilderExtensions
    {
        public static void ConfigureIdentityModels(this ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;

            ConfigUsersAccount();
            ConfigClaim();
            ConfigExternalLogin();
        }

        private static void ConfigClaim()
        {
            var entity = _modelBuilder.Entity<UserClaim>();

            entity
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity
                .Property(p => p.Type)
                .IsRequired()
                .HasMaxLength(400);

            entity
                .Property(p => p.Value)
                .IsRequired()
                .HasMaxLength(400);
        }

        private static void ConfigExternalLogin()
        {
            var entity = _modelBuilder.Entity<ExternalLogin>();

            entity
                .HasKey(x => new { x.LoginProvider, x.ProviderKey, x.UserId });

            entity
                .Property(p => p.LoginProvider)
                .IsRequired()
                .HasMaxLength(128);

            entity
                .Property(p => p.ProviderKey)
                .IsRequired()
                .HasMaxLength(128);
        }


        private static void ConfigTwoFactorAuthToken()
        {
            var entity = _modelBuilder.Entity<TwoFactorAuthToken>();

            entity
              .Property(e => e.Id)
              .ValueGeneratedOnAdd();

            entity
              .Property(p => p.Token)
              .IsRequired()
              .HasMaxLength(128);
        }

        private static void ConfigUsersAccount()
        {
            var entity = _modelBuilder.Entity<UserAccount>();

            entity
                .HasKey(x => x.UserId);

            entity
                .Property(e => e.UserId)
                .ValueGeneratedOnAdd();

            entity
                .Property(p => p.Username)
                .IsRequired()
                .HasMaxLength(254);

            entity
                .HasIndex(u => u.Username)
                .IsUnique();

            entity
                .Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(254);

            entity
                .HasIndex(u => u.Email)
                .IsUnique();

            entity
                .Property(p => p.MobileCode)
                .HasMaxLength(100);

            entity
                .Property(p => p.MobilePhoneNumber)
                .HasMaxLength(20);

            entity
                .HasIndex(u => u.MobilePhoneNumber)
                .IsUnique();

            entity
                .Property(p => p.VerificationKey)
                .HasMaxLength(100);

            entity
                .Property(p => p.VerificationStorage)
                .HasMaxLength(100);

            entity
                .Property(p => p.HashedPassword)
                .HasMaxLength(200);

            entity
                .HasMany(x => x.ClaimCollection)
                .WithOne(u => u.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
              .HasMany(x => x.ExternalLoginCollection)
              .WithOne(u => u.User)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            entity
              .HasMany(x => x.TwoFactorAuthTokenCollection)
              .WithOne(u => u.User)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            entity
              .HasMany(x => x.PasswordResetSecretCollection)
              .WithOne(u => u.User)
              .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(x => x.ExceptionsRaised)
                .WithOne(u => u.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
               .HasMany(x => x.AuditLogs)
               .WithOne(u => u.User)
               .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
              .HasMany(x => x.NotificationsSent)
              .WithOne(u => u.User)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            entity
                .Property(s => s.Created)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity
                .Property(s => s.LastUpdated)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity
               .Property(s => s.Guid)
               .HasDefaultValueSql("NEWID()");

        }

    }
}