using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasterApi.Core.Auth.Models;
using MasterApi.Core.Models;

namespace MasterApi.Data.EF7
{
    public static partial class ModelBuilderExtensions
    {
        private static ModelBuilder _modelBuilder;

        public static void ConfigureDomainModels(this ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;

            ConfigClients();
            ConfigRefreshToken();

            ConfigExceptionLog();
            ConfigAuditLog();
            ConfigNotification();

            ConfigCountry();
            ConfigEnabledCountries();
            ConfigLanguage();
            ConfigLanguageCountry();
            ConfigProvinceState();

            ConfigUserProfile();
            ConfigNotebook();
        }

        private static void ConfigClients()
        {
            var entity = _modelBuilder.Entity<Audience>();

            entity
                .HasKey(e => e.ClientId);

            entity
                .Property(e => e.ClientId)
                .HasMaxLength(50);

            entity.Property(p => p.Secret)
                .IsRequired()
                .HasMaxLength(200);

            entity
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity
                .Property(p => p.AllowedOrigin)
                .HasMaxLength(100);          
        }

        private static void ConfigRefreshToken()
        {
            var entity = _modelBuilder.Entity<RefreshToken>();

            entity
                .Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(p => p.Subject)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.ProtectedTicket)
                .IsRequired();

            entity
                .Property(p => p.ClientId)
                .IsRequired()
                .HasMaxLength(50);

            entity
                .HasOne(x => x.Audience)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.ClientId);
        }


        private static void ConfigUserProfile()
        {
            var entity = _modelBuilder.Entity<UserProfile>();

            entity
                .HasKey(e => e.UserId);

            entity
                .Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(40);

            entity
                .Property(p => p.DisplayName)
                .HasMaxLength(80);

            entity
                .Property(p => p.Avatar)
                .HasMaxLength(200);

            entity
                .Property(p => p.LastName)
                .HasMaxLength(200);

            entity
                .Property(p => p.Iso2)
                .HasColumnType("char(2)")
                .HasMaxLength(2);

            entity.Property(p => p.ProvinceState)
                .HasColumnType("char(3)")
                .HasMaxLength(3);

            entity
                .Property(p => p.City)
                .HasMaxLength(200);

            entity
                .Property(s => s.Created)
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity
                .HasOne(t => t.UserAccount)
                .WithOne(x => x.Profile)
                .HasForeignKey<UserProfile>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(a => a.ResidenceProvinceState)
                .WithMany(p => p.UserProfiles)
                .HasForeignKey(a => new {a.Iso2, a.ProvinceState});

            AddAuditProperties(entity);
        }

        private static void AddAuditProperties(EntityTypeBuilder entity, bool addModifiers = false)
        {
            entity
                .Property<DateTimeOffset>("Created")
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity
               .Property<DateTimeOffset>("Updated")
               .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            if (!addModifiers) return;

            entity
                .Property<int>("CreatedById");

            entity
                .Property<int>("UpdatedById");
        }

        private static void ConfigNotebook()
        {
            var entity = _modelBuilder.Entity<Note>();
            entity
               .Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(p => p.Title).IsRequired().HasMaxLength(200);

            entity.Property(p => p.Description).HasMaxLength(400);

            entity.Property(p => p.Meta).HasMaxLength(200);

            entity
               .HasOne(t => t.UserAccount)
               .WithMany(x => x.Notes)
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigCountry()
        {
            var entity = _modelBuilder.Entity<Country>();

            entity
                .HasKey(p => p.Iso2);

            entity
                .Property(p => p.Iso2)
                .HasColumnType("char(2)")
                .HasMaxLength(2);

            entity
                .HasIndex(u => u.Iso2)
                .IsUnique();

            entity
                .Property(p => p.Iso3)
                .HasColumnType("char(3)")
                .HasMaxLength(3)
                .IsRequired();

            entity
                .Property(p => p.Currency)
                .HasColumnType("char(3)")
                .HasMaxLength(3);

            entity
                .Property(p => p.Name)
                .HasMaxLength(200).IsRequired();

            entity
                .Property(p => p.Capital)
                .HasMaxLength(200);

            entity
                .Property(p => p.OficialName)
                .HasMaxLength(200);

            entity
                .Property(p => p.Latitude)
                .HasMaxLength(20);

            entity
                .Property(p => p.Longitude)
                .HasMaxLength(20);

            entity
                .Property(p => p.PhoneCode)
                .HasMaxLength(20);

            entity
                .Property(p => p.PostalCodeRegex)
                .HasMaxLength(200);

            entity
                .Property(p => p.PostalCodeFormat)
                .HasMaxLength(200);
        }

        private static void ConfigEnabledCountries()
        {
            var entity = _modelBuilder.Entity<EnabledCountry>();

            entity
                .HasKey(a => new { a.Iso2 });

            entity
                .Property(p => p.Iso2)
                .HasColumnType("char(2)")
                .HasMaxLength(2);

            entity
                .HasOne(a => a.Country)
                .WithOne(p => p.EnabledCountry)
                .HasForeignKey<EnabledCountry>(b => b.Iso2);
        }

        private static void ConfigLanguage()
        {
            var entity = _modelBuilder.Entity<Language>();

            entity
                .HasKey(p => p.Code);

            entity
                .Property(p => p.Code)
                .HasMaxLength(20);

            entity
               .HasIndex(u => u.Code)
               .IsUnique();

            entity
                .Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();
        }

        private static void ConfigLanguageCountry()
        {
            var entity = _modelBuilder.Entity<LanguageCountry>();

            entity
                .HasKey(a => new { a.LanguageCode, a.Iso2 });

            entity
                .Property(p => p.Iso2)
                .HasColumnType("char(2)")
                .HasMaxLength(2)
                .IsRequired();

            entity
                .Property(p => p.LanguageCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(a => a.Country)
                .WithMany(p => p.Languages)
                .HasForeignKey(a => a.Iso2)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Language)
                .WithMany(s => s.Countries)
                .HasForeignKey(a => a.LanguageCode)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigProvinceState()
        {
            var entity = _modelBuilder.Entity<ProvinceState>();

            entity
                .HasKey(k => new { k.Iso2, k.Code });

            entity.Property(p => p.Iso2)
                .IsRequired()
                .HasColumnType("char(2)")
                .HasMaxLength(2);

            entity.Property(p => p.Code)
                .IsRequired()
                .HasColumnType("char(3)")
                .HasMaxLength(3);

            entity
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(a => a.Country)
                .WithMany(p => p.ProvinceStates)
                .HasForeignKey(a => a.Iso2)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigExceptionLog()
        {
            var entity = _modelBuilder.Entity<ExceptionLog>();

            entity
                .Property(e => e.Id).ValueGeneratedOnAdd();

            entity
              .Property(s => s.Created)
              .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity
                .Property(p => p.Message)
                .IsRequired()
                .HasMaxLength(800);

            entity
                .Property(p => p.Source)
                .HasMaxLength(400);

            entity
                .Property(p => p.RequestUri)
                .HasMaxLength(200);

            entity
                .Property(p => p.Method)
                .HasMaxLength(20);
        }

        private static void ConfigAuditLog()
        {
            var entity = _modelBuilder.Entity<AuditLog>();

            entity
                .Property(e => e.Id).ValueGeneratedOnAdd();

            entity
              .Property(s => s.DataTime)
              .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity
                .Property(p => p.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity
                .Property(p => p.EntityId)
                .HasMaxLength(32);

            entity
                .Property(p => p.Event)
                .HasMaxLength(200);
        }

        private static void ConfigNotification()
        {
            var entity = _modelBuilder.Entity<Notification>();

            entity
                .Property(e => e.Id).ValueGeneratedOnAdd();

            entity
              .Property(s => s.Created)
              .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity
                .Property(p => p.Message)
                .HasMaxLength(200);

        }
    }
}