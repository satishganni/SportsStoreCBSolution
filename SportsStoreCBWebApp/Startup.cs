using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SportsStoreCBWebApp.Models;
using SportsStoreCBWebApp.Models.Abstract;
using SportsStoreCBWebApp.Models.Concrete;
using SportsStoreCBWebApp.Models.Services;

namespace SportsStoreCBWebApp
{
  public class Startup
  {
    public IConfiguration Configuration { get; }


    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<StorageUtility>(cfg => {
        cfg.StorageAccountName = Configuration["StorageAccountInformation:StorageAccountName"];
        cfg.StorageAccountAccessKey = Configuration["StorageAccountInformation:StorageAccountAccessKey"];
      });

      services.Configure<CosmosUtility>(cfg => {
        cfg.CosmosEndpoint = Configuration["CosmosConnectionString:CosmosEndpoint"];
        cfg.CosmosKey = Configuration["CosmosConnectionString:CosmosKey"];
      });

      services.AddMvc().AddRazorRuntimeCompilation();

      services.AddScoped<CosmosDbContext>();

      if (Configuration["EnableRedisCaching"] == "true")
      {
        services.AddDistributedRedisCache(cfg => {
          cfg.Configuration = Configuration["ConnectionStrings:RedisConnection"];
          cfg.InstanceName = "master";
        });
      }

      services.AddScoped<IProductRepository, CDbProductRepository>();
      services.AddScoped<IPhotoService, PhotoService>();

      services.AddApplicationInsightsTelemetry(cfg =>
      {
        cfg.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
      });

      services.AddLogging(cfg =>
      {
        cfg.AddApplicationInsights(Configuration["ApplicationInsights:InstrumentationKey"]);
        // Optional: Apply filters to configure LogLevel Information or above is sent to
        // ApplicationInsights for all categories.
        cfg.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);

        // Additional filtering For category starting in "Microsoft",
        // only Warning or above will be sent to Application Insights.
        //cfg.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
      var appInsightsFlag = app.ApplicationServices.GetService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();
      if (Configuration["EnableAppInsightsDisableTelemetry"] == "false")
      {
        appInsightsFlag.DisableTelemetry = false;
      }
      else
      {
        appInsightsFlag.DisableTelemetry = true;
      }

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseFileServer();
      // app.UseStaticFiles();

      using (var scope = app.ApplicationServices.CreateScope())
      {
        var context = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();
        var databaseFlag = context.CreateDatabaseAsync().GetAwaiter().GetResult();
        var containerFlag = context.CreateContainerAsync(partitionKeyName: "category").GetAwaiter().GetResult();
      }

      app.UseRouting();

      app.UseEndpoints(ConfigureEndpoints);
    }

    private void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
      endpoints.MapControllerRoute(name: "Default", pattern: "{controller=Product}/{action=List}/{id?}");
      //endpoints.MapDefaultControllerRoute();
      //endpoints.MapGet("/", async context =>
      //{
      //  await context.Response.WriteAsync("Hello World!");
      //});
    }
  }
}
