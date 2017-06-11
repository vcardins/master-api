using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using CsvHelper;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Auth.Enums;
using MasterApi.Core.Auth.Models;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Extensions;
using MasterApi.Core.Infrastructure.Crypto;
using MasterApi.Core.Models;
using MasterApi.Data.EF7;
using MasterApi.Core.Account.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MasterApi.Data.Seeders
{
    public class AppDbInitializer
    {
        private static AppDbContext _context;
        private static ICrypto _crypto;

        private static string _seederPath; 
        private static Assembly _assembly;
        private static bool _hasUpdates;

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            _context = (AppDbContext)serviceProvider.GetService(typeof(AppDbContext));
            _crypto = (ICrypto)serviceProvider.GetService(typeof(ICrypto));
            _assembly = typeof(AppDbInitializer).GetTypeInfo().Assembly;

            _seederPath = string.Format("{0}.Seeders.csv", _assembly.GetName().Name);

            await _context.Database.EnsureCreatedAsync();
            await _context.Database.MigrateAsync();

            SeedAudiences();
            SeedCountries();
            SeedProvinceStates();
            SeedLanguages();
            SeedAccounts();

            if (_hasUpdates)
            {
                await _context.SaveChangesAsync();
            }
        }

        private static string GetResourceFilename(string resouce)
        {
            return string.Format("{0}.{1}.csv", _seederPath, resouce);
        }

        public static void SeedAudiences()
        {
            if (_context.Audiences.Any()) return;

            new List<Audience> {
                new Audience
                {
                    ClientId = Guid.NewGuid().ToString(),
                    Name = "MasterApi SPA",
                    ApplicationType = ApplicationTypes.JavaScript,
                    Active = true,
                    RefreshTokenLifeTime = 7200,
                    Secret = _crypto.Hash("loc@lh0st"),
                    AllowedOrigin = "http://localhost:55000",
                    ObjectState = ObjectState.Added
                },
                 new Audience
                {
                    ClientId = Guid.NewGuid().ToString(),
                    Name = "Console",
                    ApplicationType = ApplicationTypes.NativeConfidential,
                    Active = true,
                    RefreshTokenLifeTime = 14400,
                    Secret = _crypto.Hash("c0ns0l3"),
                    AllowedOrigin = "*",
                    ObjectState = ObjectState.Added
                }
            }.ForEach(x => _context.Audiences.Add(x));
            _hasUpdates = true;
        }

        private static void SeedProvinceStates()
        {
            if (_context.ProvinceStates.Any()) return;

            var file = GetResourceFilename("province_states");
            using (var stream = _assembly.GetManifestResourceStream(file))
            {
                if (stream == null) return;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var csvReader = new CsvReader(reader);
                    csvReader.Configuration.WillThrowOnMissingField = false;
                    csvReader.Configuration.Delimiter = "|";
                    var records = csvReader.GetRecords<ProvinceState>().ToArray();
                    records.ForEach(r =>
                    {
                        r.ObjectState = ObjectState.Added;
                        r.Country = null;
                    });
                    _context.ProvinceStates.AddRange(records);
                    _hasUpdates = true;
                }
            }
        }

        private static void SeedCountries()
        {
            if (_context.Countries.Any()) return;

            var file = GetResourceFilename("countries");
            using (var stream = _assembly.GetManifestResourceStream(file))
            {
                if (stream == null) return;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var csvReader = new CsvReader(reader);
                    csvReader.Configuration.WillThrowOnMissingField = false;
                    csvReader.Configuration.Delimiter = "|";
                    var records = csvReader.GetRecords<Country>().ToArray();
                    records.ForEach(r =>
                    {
                        r.ObjectState = ObjectState.Added;
                        r.EnabledCountry = null;
                    });
                    _context.Countries.AddRange(records);
                    _hasUpdates = true;
                }
            }
        }

        private static void SeedLanguages()
        {
            if (_context.Languages.Any()) return;

            var file = GetResourceFilename("languages");
            using (var stream = _assembly.GetManifestResourceStream(file))
            {
                if (stream == null) return;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var csvReader = new CsvReader(reader);
                    csvReader.Configuration.WillThrowOnMissingField = false;
                    csvReader.Configuration.Delimiter = "|";
                    var records = csvReader.GetRecords<Language>().ToArray();
                    records.ForEach(r =>
                    {
                        r.ObjectState = ObjectState.Added;
                    });
                    _context.Languages.AddRange(records);
                    _hasUpdates = true;
                }
            }
        }

        private static void SeedAccounts()
        {
            var username = "admin";
            var hashedPassword = _crypto.HashPassword(username, 10);
            var user = _context.UsersProfiles.SingleOrDefault(x => x.UserAccount.Username == username);
            if (user == null)
            {
                _context.Set<UserAccount>().Add(
                    new UserAccount
                    {
                        Username = username,
                        Guid = Guid.NewGuid(),
                        HashedPassword = hashedPassword,
                        Email = "admin@masterapi.com",
                        LastUpdated = DateTimeOffset.Now,
                        IsLoginAllowed = true,
                        IsAccountVerified = true,
                        Created = DateTimeOffset.UtcNow,
                        ClaimCollection = new UserClaimCollection
                        {
                            new UserClaim(ClaimTypes.Role, UserAccessLevel.Admin.ToString()) {ObjectState = ObjectState.Added}
                        },
                        Profile = new UserProfile
                        {
                            FirstName = "System",
                            LastName = "Admin",
                            Iso2 = "CA",
                            ProvinceState = "BC",
                            Created = DateTimeOffset.UtcNow,
                            ObjectState = ObjectState.Added
                        },
                        ObjectState = ObjectState.Added
                    }
                );
                _hasUpdates = true;
            }

            username = "demo";
            hashedPassword = _crypto.HashPassword(username, 10);
            user = _context.UsersProfiles.SingleOrDefault(x => x.UserAccount.Username == username);
            if (user != null) return;

            _context.Set<UserAccount>().Add(
                new UserAccount
                {
                    Username = username,
                    Guid = Guid.NewGuid(),
                    HashedPassword = hashedPassword,
                    Email = "demo@masterapi.com",
                    LastUpdated = DateTimeOffset.Now,
                    IsLoginAllowed = true,
                    IsAccountVerified = true,
                    Created = DateTimeOffset.UtcNow,
                    ClaimCollection = new UserClaimCollection
                    {
                        new UserClaim(ClaimTypes.Role, UserAccessLevel.User.ToString()) {ObjectState = ObjectState.Added}
                    },
                    Profile = new UserProfile
                    {
                        FirstName = "Demo",
                        LastName = "user",
                        Iso2 = "CA",
                        ProvinceState = "BC",
                        Created = DateTimeOffset.UtcNow,
                        ObjectState = ObjectState.Added
                    },
                    ObjectState = ObjectState.Added
                }
            );
            _hasUpdates = true;
        }
    }
}
