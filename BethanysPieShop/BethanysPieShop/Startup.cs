using BethanysPieShop.Auth;
using BethanysPieShop.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<AppDbContext>();

            services.AddTransient<IPieRepository, PieRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddScoped<ShoppingCart>(sp => ShoppingCart.GetCart(sp));
            services.AddTransient<IPieReviewRepository, PieReviewRepository>();

            //services.AddHttpContextAccessor();

            services.AddAntiforgery();
            //services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            //services.AddMvc();
            //global filter
            //services.AddMvc
            //    (
            //        config => { config.Filters.AddService(typeof(TimerAction)); }
            //    )
            //    .AddViewLocalization(
            //        LanguageViewLocationExpanderFormat.Suffix,
            //        opts => { opts.ResourcesPath = "Resources"; })
            //    .AddDataAnnotationsLocalization();
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            services.AddMvc();
            //Claims-based
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
                options.AddPolicy("DeletePie", policy => policy.RequireClaim("Delete Pie", "Delete Pie"));
                options.AddPolicy("AddPie", policy => policy.RequireClaim("Add Pie", "Add Pie"));
                options.AddPolicy("MinimumOrderAge", policy => policy.Requirements.Add(new MinimumOrderAgeRequirement(18)));
            });

            services.AddSession();
            services.AddMemoryCache();
            //services.AddControllersWithViews();
            //services.AddRazorPages();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            //logging
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug(LogLevel.Debug);

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseGoogleAuthentication(new GoogleOptions
            //{
            //    ClientId = "888527057729-951v5feb1hjjg1c07fbgi09g7213p1f1.apps.googleusercontent.com",
            //    ClientSecret = "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"
            //});


            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });


        }
    }
}
