using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Newtonsoft.Json.Serialization;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Config;
using MasterApi.Data.EF7;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Data.DataContext;
using MasterApi.Core.Infrastructure.Crypto;
using MasterApi.Core.Services;
using MasterApi.Infrastructure.Crypto;
using MasterApi.Services;
using MasterApi.Services.Account;
using MasterApi.Services.Domain;
using MasterApi.Web.Filters;
using Hangfire;
using HybridModelBinding;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.Options;
using NetEscapades.AspNetCore.SecurityHeaders;
using RazorLight;
using MasterApi.Core.Auth.Services;
using MasterApi.Core.Messaging.Email;
using MasterApi.Core.Messaging.Sms;
using MasterApi.Data.Seeders;
using MasterApi.Infrastructure.Messaging;
using MasterApi.Services.Auth;
using MasterApi.Web.Identity;
using MasterApi.Web.SignalR.Connections;
using System.Reflection;
using MasterApi.Services.Extensions;
using System.Linq;
using MasterApi.Core.Extensions;
using Microsoft.Extensions.Configuration.UserSecrets;
using Swashbuckle.AspNetCore.Swagger;

//http://stackoverflow.com/questions/40704760/invalidoperationexception-could-not-find-usersecretsidattribute-on-assembly
[assembly: UserSecretsId("aspnet-TestApp-ce345b64-19cf-4972-b34f-d16f2e7976ed")]
namespace MasterApi
{
    public partial class Startup
    {
        private const string AuthSchema = JwtBearerDefaults.AuthenticationScheme;
        private readonly AppSettings _appSettings;
        private static string _contentRootPath = string.Empty;
        private const string SettingsSectionKey = "AppSettings";
        private const string LoggingSectionKey = "Logging";

        private IUserAccountService _userService;
        private IAuthService _authService;
        private ICrypto _crypto;

        public IConfigurationRoot Configuration { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        public Startup(IHostingEnvironment env)
        {
            //_applicationPath = env.WebRootPath;
            _contentRootPath = env.ContentRootPath;
            _appSettings = new AppSettings();

            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(_contentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC services to the services container.
            // Build the intermediate service provider
           
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddScoped<ValidateMimeMultipartContentFilter>();

            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add our Config object so it can be injected
            var appSettingsSection = Configuration.GetSection(SettingsSectionKey);
            services.Configure<AppSettings>(appSettingsSection);
            services.Configure<AppSettings>(settings =>
            {
                if (HttpContextAccessor.HttpContext == null) return;
                var request = HttpContextAccessor.HttpContext.Request;
                settings.Urls.Api = $"{request.Scheme}://{request.Host.ToUriComponent()}";
            });

            appSettingsSection.Bind(_appSettings);

            // *If* you need access to generic IConfiguration this is **required**
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton(Configuration);

            ConfigAuth(services);

            // Enable Cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                );
            });

            services.AddMvc(options => 
                {
                    //http://www.dotnetcurry.com/aspnet/1314/aspnet-core-globalization-localization
                    options.Conventions.Add(new HybridModelBinderApplicationModelConvention());
                    options.Conventions.Add(new NameSpaceVersionRoutingConvention());
                    // Make authentication compulsory across the board (i.e. shut
                    // down EVERYTHING unless explicitly opened up).
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireClaim(ClaimTypes.Name)
                        .RequireClaim(ClaimTypes.NameIdentifier)
                        .Build();

                    options.Filters.Add(new AuthorizeFilter(policy));
                }
            ).AddJsonOptions(opts => 
                {
                    // Force Camel Case to JSON
                    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }
            )
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();

            //https://www.billbogaiv.com/posts/hybrid-model-binding-in-aspnet-core-10-rc2
            services.Configure<MvcOptions>(options =>
            {
                /**
                 * This is needed since the provider uses the existing `BodyModelProvider`.
                 * Ref. https://github.com/aspnet/Mvc/blob/1.0.0-rc2/src/Microsoft.AspNetCore.Mvc.Core/ModelBinding/Binders/BodyModelBinder.cs
                 */
                var readerFactory = services.BuildServiceProvider().GetRequiredService<IHttpRequestStreamReaderFactory>();

                options.ModelBinderProviders.Insert(0, new DefaultHybridModelBinderProvider(options.InputFormatters, readerFactory));
                // options.Filters.Add(new CorsAuthorizationFilterFactory("CorsPolicy"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = _appSettings.ApplicationName, Version = _appSettings.Version });
            });

            // Injection
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<RequestLocalizationOptions>(
                options => {
                    var supportedCultures = new List<CultureInfo> {
                            new CultureInfo("en"),
                            new CultureInfo("en-US"),
                            new CultureInfo("pt"),
                            new CultureInfo("pt-BR")
                        };
                    var defaultCulture = supportedCultures[1].Name;
                    options.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultCulture);
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                }
            );
   
            // SignalR
            var authorizer = new HubAuthorizeAttribute(_tokenOptions);
            var module = new AuthorizeModule(authorizer, authorizer);

            services.AddSignalR(options =>
            {
                options.EnableJSONP = true;
                options.Hubs.EnableJavaScriptProxies = true;
                options.Hubs.EnableDetailedErrors = true;
                options.Hubs.PipelineModules.Add(module);
            });

            services.AddCustomHeaders();
            services.AddDbContext<AppDbContext>(options =>
            {
                switch (_appSettings.DbConnection.InMemoryProvider)
                {
                    case true:
                        options.UseInMemoryDatabase();
                        break;
                    default:
                        options.UseSqlServer(_appSettings.DbConnection.ConnectionString, b => b.MigrationsAssembly("MasterApi"));
                        break;
                }
            });

            services.AddHangfire(x => x.UseSqlServerStorage(_appSettings.DbConnection.ConnectionString));

            // Repositories
            services.AddScoped(typeof(IDataContextAsync), typeof(AppDbContext));
            services.AddScoped(typeof(IUnitOfWorkAsync), typeof(UnitOfWork));
            services.AddScoped(typeof(IRepositoryAsync<>), typeof(Repository<>));

            //Services
            services.AddScoped(typeof(IService<>), typeof(Service<>));

            services.AddScoped(typeof(IAuthService), typeof(AuthService));
            services.AddScoped(typeof(IUserAccountService), typeof(UserAccountService));
            services.AddScoped(typeof(INotificationService), typeof(NotificationService));
            services.AddScoped(typeof(IGeoService), typeof(GeoService));
            services.AddScoped(typeof(IUserProfileService), typeof(UserProfileService));

            //Infrastructure
            services.AddSingleton(typeof(ICrypto), typeof(DefaultCrypto));
            services.AddSingleton(typeof(ISmsSender), typeof(SmsSender));
            services.AddSingleton(typeof(IEmailSender), typeof(EmailSender));

            services.AddSingleton(typeof(IRazorLightEngine), s => EngineFactory.CreatePhysical($"{_contentRootPath}\\Views"));

            var asm = Assembly.GetEntryAssembly();
            var subjectFiles = asm.GetManifestResourceNames().Where(x => x.Contains("subjects.json"));

            var emailSubjects = new Dictionary<string, Dictionary<string, string>>();

            subjectFiles.ForEach(file => {
                var domain = file.Split('.');
                var subjects = asm.ParseFromJson(file);
                emailSubjects.Add(domain[3], subjects);
            });

            services.AddSingleton(typeof(IEmailSubjects), emailSubjects);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //Identity
            services.AddScoped(typeof(IUserInfo), s => new UserIdentityInfo(s.GetService<IHttpContextAccessor>().HttpContext.User));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
                              IAntiforgery antiforgery, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {

            HttpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();

            _userService = serviceProvider.GetService<IUserAccountService>();
            _authService = serviceProvider.GetService<IAuthService>();
            _crypto = serviceProvider.GetService<ICrypto>();

            app.UseCors("CorsPolicy");

            app.UseTokenProvider(_tokenOptions, _signingKey);

            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            localizationOptions.Value.RequestCultureProviders.Insert(0, new UrlRequestCultureProvider());
            app.UseRequestLocalization(localizationOptions.Value);

            // Show Index.html as default page
            app.UseDefaultFiles();

            app.UseStaticFiles();

            //app.UseHangfireDashboard();

            //app.UseHangfireServer();

            //const int maxAge = 60*60*24*365;
            var policyCollection = new HeaderPolicyCollection()
                .AddFrameOptionsDeny()
                .AddXssProtectionBlock()
                .AddContentTypeOptionsNoSniff()
                .AddStrictTransportSecurityMaxAge() // maxage = one year in seconds
                .RemoveServerHeader();
            //.AddCustomHeader("X-TOTAL-RECORDS", "Header value");

            app.UseCustomHeadersMiddleware(policyCollection);

            //RecurringJob.AddOrUpdate(() => Console.WriteLine("Minutely Job"), Cron.Minutely);           

            app.UseWebSockets();

            app.UseSignalR<RawConnection>("/signalr");

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                var cultureConstraint = new
                {
                    Culture = new RegexRouteConstraint("^[a-z]{{2}}-[A-Z]{{2}}$")
                };

                routes.MapRoute(
                    "apiVersionCulture",
                    "api/{version}/{culture}/{controller}/{action=Index}/{id?}",
                    cultureConstraint
                );

                routes.MapRoute(
                    "apiVersion",
                    "api/{version}/{controller}/{action=Index}/{id?}"
                );

                routes.MapRoute(
                    "apiCulture",
                    "api/{culture}/{controller}/{action=Index}/{id?}",
                    cultureConstraint
                );

                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}"
                );

            });

            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore
            // Middleware to expose the generated Swagger as JSON endpoint(s)
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{version}/swagger.json";
            });

            // Middleware to expose interactive documentation
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api-docs";
                c.DocExpansion("full");
                c.ShowRequestHeaders();
                c.InjectOnCompleteJavaScript("/swagger-ui/on-complete.js");
                c.InjectOnFailureJavaScript("/swagger-ui/on-failure.js");
                c.ShowJsonEditor();
                c.SwaggerEndpoint("/api-docs/{version}/swagger.json", _appSettings.ApplicationName);
            });

            app.Use(next => context =>
            {
                if (!string.Equals(context.Request.Path.Value, "/", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(context.Request.Path.Value, "/index.html", StringComparison.OrdinalIgnoreCase))
                {
                    return next(context);
                }

                var tokens = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false });
                return next(context);
            });

            loggerFactory.AddConsole(Configuration.GetSection(LoggingSectionKey));
            loggerFactory.AddDebug();

            AppDbInitializer.Initialize(app.ApplicationServices);
        }
    }
}
