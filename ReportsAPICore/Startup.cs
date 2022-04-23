using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReportsLIBCore.Adomd;
using ReportsLIBCore.DTOs;
using ReportsLIBCore.Queries;
using ReportsLIBCore.Handlers;
using GoodWill_Libraries.Queries;
using GoodWill_Libraries.Queries.Dispatcher;

namespace ReportsAPICore
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
            AdomdContext.ConnectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddMemoryCache();

            services.AddControllers();

            services.AddScoped<IQueryHandler<SalesByManagerCategoryQuery, SalesByManagerCategoryDTO[]>, SalesByManagerCategoryHandler>();

            services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
