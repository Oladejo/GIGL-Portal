using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GIGLCustomerPortal.Data;
using GIGLCustomerPortal.Services;
using System;
using System.Threading.Tasks;

namespace GIGLCustomerPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("GiglDbConnectionString")));

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                
                // Lockout settings  
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings  
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings  
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/denied";
                options.SlidingExpiration = true;
            });


            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AllowAnonymousToPage("/Public/TrackShipment");

                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                });
            
            services.AddSession((options) =>
            {
                // Set a short timeout for easy testing.
                options.Cookie.Name = ".Gigl.Session";
                options.IdleTimeout = TimeSpan.FromDays(200);
                options.Cookie.HttpOnly = true;
            });

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Account}/{action=Login}");
            });

            //Comment this method once we push it online
            //create admin user if it does not exist in the database and assign admin role 
            InitializeAdminUser(serviceProvider).Wait();
        }

        private async Task InitializeAdminUser(IServiceProvider serviceProvider)
        {
            var _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            const string name = "admin@gigl.com";
            const string password = "gigl@123456";
            const string roleName = "Admin";
            
            //Create Role Admin if it does not exist
            var roleCheck = await _roleManager.RoleExistsAsync(roleName);
            if (!roleCheck)
            {
                //create the roles and seed them to the database  
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            //Create Admin User if it does not exist
            var user = await _userManager.FindByEmailAsync(name);
            if(user == null)
            {
                user = new ApplicationUser
                {
                    UserName = name,
                    Email = name
                };
                var result = await _userManager.CreateAsync(user, password);
                result = await _userManager.SetLockoutEnabledAsync(user, false);
            }

            // Add user admin to Role Admin if not already added
            var rolesForUser = await _userManager.GetRolesAsync(user);
            if (!rolesForUser.Contains(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
