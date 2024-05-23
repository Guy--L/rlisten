using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using rlisten.Managers;
using rlisten.Services;
using static System.Runtime.InteropServices.RuntimeInformation;
using static System.Runtime.InteropServices.OSPlatform;
using Reddit.AuthTokenRetriever;

namespace rlisten;

class Program
{
    static void Main(string[] args)
    {
        var browserPath =
            IsOSPlatform(OSX) ? @"/Applications/Google\\ Chrome.app" :
            IsOSPlatform(Windows) ? @"C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe" :
            IsOSPlatform(Linux) ? "/usr/bin/google-chrome-stable" :
            IsOSPlatform(FreeBSD) ? "/usr/bin/google-chrome-stable/" : "Unknown OS";

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.SetBasePath(env.ContentRootPath);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
                config.AddEnvironmentVariables();
                if (env.IsDevelopment())
                {
                    config.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
                }
            })
            .ConfigureServices((context, services) =>
            {
                string rApiID = null;
                string rApiSecret = null;

                rApiID = context.Configuration["RedditOAuth:ClientId"];
                rApiSecret = context.Configuration["RedditOAuth:ClientSecret"];

                if (string.IsNullOrEmpty(rApiID) || string.IsNullOrEmpty(rApiSecret))
                {
                    if (args.Count() != 2)
                        throw new ArgumentException("Usage: rlisten <ClientId> <ClientSecret>\n");

                    rApiID = args[0];
                    rApiSecret = args[1];
                }

                // Pass the secrets to rListener or any other service that needs them
                services.AddSingleton<IFileProvider, PhysicalFileProvider>();
                services.AddSingleton<ISettingsManager, SettingsManager>();
                services.AddSingleton(provider => context.Configuration);
                services.AddSingleton(provider => new AuthTokenRetrieverLib(rApiID, rApiSecret, 8080));
            })
            .Build();

        host.Dispose();
        Environment.Exit(0);
    }
}
