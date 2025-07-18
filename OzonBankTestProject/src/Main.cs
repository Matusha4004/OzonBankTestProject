using Microsoft.EntityFrameworkCore;
using OzonBankTestProject.API;
using OzonBankTestProject.Domain;
using OzonBankTestProject.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace OzonBankTestProject;

public class main
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

               
                services.AddDbContext<ReportsContext>(options =>
                    options.UseNpgsql(connectionString));
                services.AddDbContext<InfoContext>(options =>
                    options.UseNpgsql(connectionString));

                services.AddGrpc();

                services.AddScoped<ICalculateRatio, BasicCalculateRatio>();
                services.AddScoped<GrpcService>();

                services.AddSingleton<ReportCache>();

                services.AddHostedService<Worker>();
            })
            .Build()
            .Run();
    }
}