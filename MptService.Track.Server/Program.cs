using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MptService.Track.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MptService.Track.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)   
            .UseWindowsService() // использование в качестве Windows-сервиса
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    // получаем строку подключения из файла конфигурации                    
                    string connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");                
                    
                    // работа с БД через DbContext (СУБД postgres)
                    services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString));           
                });
    }
}
