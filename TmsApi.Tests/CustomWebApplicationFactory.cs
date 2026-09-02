using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Tests;
public class CustomWebApplicationFactory: WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      //1. Supple required test configuratiion (JWT secret, ect.)
      builder.ConfigureAppConfiguration((context, config) =>
      {
          config.AddInMemoryCollection(new Dictionary<string, string?>
          {
             ["Jwt:Key"] = "ThisIsASecretKeyForTestingPurposesOnly",
             ["Jwt:Secret"] = "ThisIsASecretKeyForTestingPurposesOnly",
             ["Jwt:Issuer"] = "TestIssuer",
             ["Jwt:Audience"] = "TestAudience"
          });
      });

    //2. Remove production Db1Context and register  InMemory with isolated internal provider
    builder.ConfigureServices(services =>
    {
        services.RemoveAll<DbContextOptions<TmsDb1Context>>();
        services.RemoveAll<DbContextOptions>();
        services.RemoveAll<TmsDb1Context>();

        var inMemoryProvider=new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
        services.AddDbContext<TmsDb1Context>(options =>
        {
            options.UseInMemoryDatabase("TmsTestDb");
            options.UseInternalServiceProvider(inMemoryProvider);
        });
    });
    }
}