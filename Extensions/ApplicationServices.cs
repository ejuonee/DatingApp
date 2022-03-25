using DatingApp.Data_Transfer_Object;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using DatingApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.EntityFrameworkCore.SqlServer;
using DatingApp.SignalR;
using DatingApp.DataTransferObject;

namespace DatingApp.Extensions
{
    public static class ApplicationServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<PresenceTracker>();
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // services.AddScoped<ILikesRepository, LikesRepository>();
            // services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPhotoService, PhotoService>();
            // services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperProfiles)));
            services.AddScoped<LogUserActivity>();
            services.AddDbContext<DataContext>(options => { options.UseSqlite(config.GetConnectionString("DefaultConnection")); });
            // services.AddDbContext<DataContext>(options => { options.UseSqlServer(config.GetConnectionString("DefaultConnection")); });

            return services;
        }
    }
}