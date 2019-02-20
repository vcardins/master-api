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
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Threading.Tasks;
using MasterApi.Services.DomainServices;

//http://stackoverflow.com/questions/40704760/invalidoperationexception-could-not-find-usersecretsidattribute-on-assembly
[assembly: UserSecretsId("aspnet-TestApp-ce345b64-19cf-4972-b34f-d16f2e7976ed")]
namespace MasterApi
{
    public partial class Startup
    {
        private const string AuthSchema = JwtBearerDefaults.AuthenticationScheme;

        private IUserAccountService _userService;
        private IAuthService _authService;
        private ICrypto _crypto;

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; set; }

		/// <summary>
		/// Gets or sets the configuration.
		/// </summary>
		/// <value>
		/// The Application settings.
		/// </value>
		public AppSettings AppSettings { get; set; }

		private IHttpContextAccessor HttpContextAccessor { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Startup"/> class.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			AppSettings = new AppSettings();
		}

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services">The services.</param>
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
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            services.Configure<AppSettings>(settings =>
            {
                if (HttpContextAccessor.HttpContext == null) return;
                var request = HttpContextAccessor.HttpContext.Request;
                settings.Urls.Api = $"{request.Scheme}://{request.Host.ToUriComponent()}";
            });

            appSettingsSection.Bind(AppSettings);

            // *If* you need access to generic IConfiguration this is **required**
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
                c.SwaggerDoc("v1", new Info {
                    Title = AppSettings.Information.Name,
                    Version = AppSettings.Information.Version,
                    Description = AppSettings.Information.Description,
                    TermsOfService = AppSettings.Information.TermsOfService,
                    Contact = new Contact {
                        Name = AppSettings.Information.ContactName,
                        Email = AppSettings.Information.ContactEmail
                    },
                    License = new License {
                        Name = AppSettings.Information.LicenseName,
                        Url = AppSettings.Information.LicenseUrl
                    }
                });

                //Set the comments path for the swagger json and ui.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "MasterApi.xml");
                c.IncludeXmlComments(xmlPath);
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

            var connString = Configuration.GetConnectionString("DbConnection");

			//services.AddDbContext<DualAuthContext>(options =>
			//	options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			services.AddDbContext<AppDbContext>(options =>
            {
                switch (AppSettings.DbSettings.InMemoryProvider)
                {
                    case true:
                        options.UseInMemoryDatabase(connString);
                        break;
                    default:
                        options.UseSqlServer(connString, b => b.MigrationsAssembly("MasterApi.Data"));
                        break;
                }
            });

            services.AddHangfire(x => x.UseSqlServerStorage(connString));

            // Repositories
            services.AddScoped(typeof(IDataContextAsync), typeof(AppDbContext));
            services.AddScoped(typeof(IUnitOfWorkAsync), typeof(UnitOfWork));
            services.AddScoped(typeof(IRepositoryAsync<>), typeof(RepositoryAsync<>));
			services.AddTransient<DataSeeder>();

			//Services
			services.AddTransient(typeof(IService<>), typeof(Service<>));

            services.AddTransient(typeof(IAuthService), typeof(AuthService));
            services.AddTransient(typeof(IUserAccountService), typeof(UserAccountService));
            services.AddTransient(typeof(INotificationService), typeof(NotificationService));
            services.AddTransient(typeof(IGeoService), typeof(GeoService));
            services.AddTransient(typeof(IUserProfileService), typeof(UserProfileService));
            services.AddTransient(typeof(ILookupService), typeof(LookupService));

            //Infrastructure
            services.AddSingleton(typeof(ICrypto), typeof(DefaultCrypto));
            services.AddSingleton(typeof(ISmsSender), typeof(SmsSender));
            services.AddSingleton(typeof(IEmailSender), typeof(EmailSender));

			services.AddSingleton(typeof(IRazorLightEngine), s => EngineFactory.CreatePhysical($"{Directory.GetCurrentDirectory()}\\Views"));

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

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app">The application.</param>
		/// <param name="env">The env.</param>
		/// <param name="antiforgery">The antiforgery.</param>
		/// <param name="loggerFactory">The logger factory.</param>
		/// <param name="serviceProvider">The service provider.</param>
		/// <param name="seeder">The database seeder.</param>
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAntiforgery antiforgery, 
				ILoggerFactory loggerFactory, IServiceProvider serviceProvider, DataSeeder seeder)
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
            app.UseSwagger();

            // Middleware to expose interactive documentation
            app.UseSwaggerUI(c =>
            {
                c.ShowJsonEditor();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", AppSettings.Information.Name);
                c.DocExpansion("full");
                c.ShowRequestHeaders();
                c.SupportedSubmitMethods(new[] { "get", "post", "delete", "put", "patch" });
                c.InjectOnCompleteJavaScript("/swagger-ui/on-complete.js");
                c.InjectOnFailureJavaScript("/swagger-ui/on-failure.js");
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

			// seeder.SeedAsync().Wait();
        }
    }
}
