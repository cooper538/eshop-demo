using EShop.AnalyticsService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddYamlConfiguration("analytics");
builder.AddServiceDefaults();
builder.AddSerilog();

builder.AddAnalyticsServices();

var host = builder.Build();
host.Run();
