using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDelta.AspNetCore3.Example.Data;

namespace MongoDelta.AspNetCore3.Example
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
            services.AddControllers();
            services.AddUnitOfWork<IUnitOfWork, UnitOfWork>();

            #if CI
            var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_DELTA_CONNECTION_STRING");
            #else
            var mongoConnectionString = "mongodb://localhost:27017/?retryWrites=false";
            #endif

            var client = new MongoClient(mongoConnectionString);
            var database = client.GetDatabase("AspNetCore3_Example");
            services.AddSingleton(database);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseUnitOfWork<UnitOfWork>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
