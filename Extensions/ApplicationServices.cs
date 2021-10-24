using DatingApp.Data_Transfer_Object;
using DatingApp.Interfaces;
using DatingApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Helpers;
using System.Reflection;

namespace DatingApp.Extensions
{
    public static class ApplicationServices
    {
        public static  IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration config)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperProfiles)));
            services.AddDbContext<DataContext>(options => { options.UseSqlite(config.GetConnectionString("DefaultConnection")); });

            return services;
        }
    }
}
