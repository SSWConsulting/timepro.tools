using Microsoft.Extensions.DependencyInjection;
using SSW.TimePro.Cli.Features.Auth;
using SSW.TimePro.Cli.Features.Tenants;
using SSW.TimePro.Cli.Features.Timesheets;
using SSW.TimePro.Cli.Features.Users;
using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Infrastructure.Config;
using SSW.TimePro.Cli.Infrastructure.DependencyInjection;
using Spectre.Console.Cli;

// Configure DI
var services = new ServiceCollection();
services.AddSingleton<IConfigService, ConfigService>();
services.AddSingleton<ITenantProvider, DefaultTenantProvider>();
services.AddHttpClient<ITimeProApiClient, TimeProApiClient>();

var registrar = new TypeRegistrar(services);

// Build command tree
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("tp");
    config.SetApplicationVersion("0.1.0");

    // Auth
    config.AddCommand<LoginCommand>("login")
        .WithDescription("Authenticate with a TimePro tenant");
    config.AddCommand<LogoutCommand>("logout")
        .WithDescription("Remove stored credentials");

    // Tenant management
    config.AddBranch("tenant", tenant =>
    {
        tenant.SetDescription("Manage tenants");
        tenant.AddCommand<TenantSetCommand>("set")
            .WithDescription("Switch the active tenant");
        tenant.AddCommand<TenantInfoCommand>("info")
            .WithDescription("Show active tenant details");
        tenant.AddCommand<TenantListCommand>("list")
            .WithDescription("List all stored tenants");
    });

    // Timesheets (with alias)
    config.AddBranch("timesheet", ts =>
    {
        ts.SetDescription("Manage timesheets");
        ts.AddCommand<GetCommand>("get")
            .WithDescription("View timesheets for a day or week");
    });

    config.AddBranch("ts", ts =>
    {
        ts.SetDescription("Manage timesheets (alias)");
        ts.AddCommand<GetCommand>("get")
            .WithDescription("View timesheets for a day or week");
    });

    // User
    config.AddBranch("user", user =>
    {
        user.SetDescription("User information");
        user.AddCommand<MeCommand>("me")
            .WithDescription("Show current user info");
    });
});

return await app.RunAsync(args);
