using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MasterApi.Core.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Auth.Models;

namespace MasterApi.Data.EF7
{
    public class AppDbContext : DataContext<AppDbContext>
    {
        public DbSet<Audience> Audiences { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserAccount> UsersAccounts { get; set; }
        public DbSet<UserClaim> Claims { get; set; }
        public DbSet<ExternalLogin> Logins { get; set; }
        public DbSet<PasswordResetSecret> PasswordResetSecrets { get; set; }
        public DbSet<TwoFactorAuthToken> TwoFactorAuthTokens { get; set; }
        public DbSet<UserProfile> UsersProfiles { get; set; }
        public DbSet<ExceptionLog> Exceptions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<EnabledCountry> EnabledCountries { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<LanguageCountry> CountryLanguages { get; set; }
        public DbSet<ProvinceState> ProvinceStates { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Note> Notes { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Makes table names be same as domain's
            var internalBuilder = modelBuilder.GetInfrastructure();
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                internalBuilder
                    .Entity(entity.Name, ConfigurationSource.Convention)
                    .Relational(ConfigurationSource.Convention)
                    .ToTable(entity.ClrType.Name);
            }

            modelBuilder.ConfigureIdentityModels();
            modelBuilder.ConfigureDomainModels();

        }       
    }
}
