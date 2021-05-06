﻿using Event_Service.Dal;
using Event_Service.Dal.Interfaces;
using Event_Service.Logic;
using Event_Service.Models.HelperFiles;
using Event_Service.RabbitMq;
using Event_Service.RabbitMq.Publishers;
using Event_Service.RabbitMq.RPC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Linq;

namespace Event_Service
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
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextPool<DataContext>(
                dbContextOptions => dbContextOptions
                                        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            AddDependencies(ref services);
            services.AddControllers();
        }

        public void AddDependencies(ref IServiceCollection services)
        {
            services.AddScoped<IPublisher, Publisher>();
            services.AddScoped(service => new RabbitMqChannel().GetChannel());
            services.AddSingleton(service => AutoMapperConfig.Config.CreateMapper());
            services.AddScoped<LogLogic>();
            services.AddScoped<EventLogic>();
            services.AddScoped<IEventDal, EventDal>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var channel = app.ApplicationServices.GetService<IModel>();
            var eventLogic = app.ApplicationServices.GetService<EventLogic>();
            var logLogic = app.ApplicationServices.GetService<LogLogic>();

            new RpcServer(channel, RabbitMqQueues.ExistsEventQueue, eventLogic.Exists, logLogic);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            DataContext context = app.ApplicationServices.GetService<DataContext>();
            ApplyMigrations(context);
        }

        public void ApplyMigrations(DataContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }
    }
}
