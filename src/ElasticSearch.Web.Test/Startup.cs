using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alvin.Core;
using Alvin.Core.Infrastructure;
using Alvin.Data;
using BLun.ETagMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace ElasticSearch.Web.Test
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<AlvinObjectContext>(optionsBuilder =>
            {
                var connection = @"Data Source=.;Initial Catalog=NopCommerce;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=123456";
                optionsBuilder.UseLazyLoadingProxies().UseSqlServer(connection).ConfigureWarnings(warnings => warnings.Throw(CoreEventId.IncludeIgnoredWarning)); ;
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                             .AddJsonOptions(
                                 options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                             );

            // Add ETagOption with own global configurations
            services.AddETag(new ETagOption()
            {
                // algorithmus
                // SHA1         = default
                // SHA265
                // SHA384
                // SHA512
                // MD5
                ETagAlgorithm = ETagAlgorithm.SHA265,

                // etag validator
                // Strong       = default
                // Weak
                ETagValidator = ETagValidator.Weak,

                // body content length
                // 40 * 1024    = default
                BodyMaxLength = 20 * 1024
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var provider = new FileExtensionContentTypeProvider
            {
                Mappings = { [".txt"] = MimeTypes.TextPlain }
            };
            var staticfile = new StaticFileOptions();
            staticfile.FileProvider = new PhysicalFileProvider(@"F:\xhw\Projects\Elasticsearch.Test\src\ElasticSearch.Web.Test\wwwroot\dic");
            //staticfile.RequestPath = new PathString("/dic");
            staticfile.ContentTypeProvider = provider;
            app.UseStaticFiles(staticfile);
            app.UseCookiePolicy();

            // Add a Middleware for each Controller Request
            // Atention: add app.UseETag after app.UseStaticFiles, the order is important for performance
            app.UseETag();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
