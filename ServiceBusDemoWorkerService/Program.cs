using ServiceBusDemoWorkerService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<ServiceBusConfigOption>(builder.Configuration.GetSection(ServiceBusConfigOption.SectionName));
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.AddSingleton<ServiceBusClientFactory, ServiceBusClientFactory>();

builder.Logging.ClearProviders();
var appInsightConnection = builder.Configuration["ApplicationInsights:ConnectionString"];
builder.Logging.AddApplicationInsights(configureTelemetryConfiguration: config => config.ConnectionString = appInsightConnection, configureApplicationInsightsLoggerOptions: (options) => { });

var host = builder.Build();
host.Run();
