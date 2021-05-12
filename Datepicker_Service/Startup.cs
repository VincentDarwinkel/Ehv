﻿using System.Data;
using System.Text.Json.Serialization;
using Datepicker_Service.Dal;
using Datepicker_Service.Dal.Interfaces;
using Datepicker_Service.Logic;
using Datepicker_Service.Models.HelperFiles;
using Datepicker_Service.RabbitMq;
using Datepicker_Service.RabbitMq.Publishers;
using Datepicker_Service.RabbitMq.Rpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datepicker_Service
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
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new NoNullAllowedException();
            }

            services.AddDbContextPool<DataContext>(
                dbContextOptions => dbContextOptions
                                        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddControllers();
            AddDependencies(ref services);
        }

        public void AddDependencies(ref IServiceCollection services)
        {
            services.AddScoped<IPublisher, Publisher>();
            services.AddScoped<ControllerHelper>();
            services.AddScoped(service => new RabbitMqChannel().GetChannel());
            services.AddScoped<ControllerHelper>();
            services.AddScoped<JwtLogic>();
            services.AddScoped<LogLogic>();
            services.AddScoped<DatepickerLogic>();
            services.AddScoped<RpcClient>();
            services.AddScoped<IDatepickerDal, DatepickerDal>();
            services.AddSingleton(service => AutoMapperConfig.Config.CreateMapper());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
