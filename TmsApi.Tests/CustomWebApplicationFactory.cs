using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Tests;
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      // 1. Supply required test configuration (JWT secret, etc.)
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsASecretKeyForTestingPurposesOnly123456!",
                ["Jwt:Secret"] ="ThisIsASecretKeyForTestingPurposesOnly123456!",
                ["Jwt:Issuer"] = "TmsTestIssuer",
                ["Jwt:Audience"] = "TmsTestAudience"
            });
        });

      // 2. Remove production DbContext and register InMemory with isolatedinternal provider  
      builder.ConfigureServices(services =>
      {
          services.RemoveAll<DbContextOptions<TmsDb1Context>>();
          services.RemoveAll<DbContextOptions>();
          services.RemoveAll<TmsDb1Context>();

          var inMemoryProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase() 
          .BuildServiceProvider();

          services.AddDbContext<TmsDb1Context>(Options =>
          {
              Options.UseInMemoryDatabase("TmsTestDb");
              Options.UseInternalServiceProvider(inMemoryProvider);
          });
      });
    }
}