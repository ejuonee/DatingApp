using DatingApp.Extensions;
using DatingApp.MiddleWare;
using DatingApp.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace DatingApp
{
  public class Startup
  {
    private readonly IConfiguration _config;

    public Startup(IConfiguration config)
    {
      _config = config;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      services.AddApplicationServices(_config);
      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "DatingApp", Version = "v1" });
      });
      services.AddCors();
      services.AddIdentityServices(_config);
      services.AddAutoMapper(typeof(Startup));
      services.AddSignalR();
      services.AddHealthChecks();
      // services.AddHttpClient();
      // services.AddSoapExceptionTransformer((ex) => ex.Message);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseMiddleware<ExceptionMiddleware>();
      if (env.IsDevelopment())
      {
        // app.UseSoapEndpoint<>();
        //         app.UseSoapEndpoint<SimpleAuthenticationWebService>(
        // "/SimpleAuthWebService/SimpleAuth.asmx",
        // new BasicHttpBinding(),
        // SoapSerializer.XmlSerializer);
        //app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DatingApp v1"));
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseCors(policy => policy.AllowAnyMethod().AllowCredentials().WithHeaders().AllowAnyHeader().AllowAnyOrigin().WithOrigins("https://localhost:4200"));
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<PresenceHub>("hubs/presence");
        endpoints.MapHub<MessageHub>("hubs/message");
        endpoints.MapFallbackToController("Index", "Fallback");
      });
    }
  }
}