using k8s.Models;
using MetricsApp.AppHost.OpenTelemetryCollector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = DistributedApplication.CreateBuilder(args);

var startAfter = DateTime.Now.AddMinutes(1); // Set the start time to 1 minute from now

builder.Services.AddHealthChecks().AddCheck("mycheck", () =>
{
    return DateTime.Now > startAfter ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
});

//builder.Services.Configure<DistributedApplicationOptions>(options => {
//    options. = TimeSpan.FromMinutes(5);
//});

//var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v3.2.1")
//    .WithBindMount("./Config/prometheus.yaml", "/etc/prometheus", isReadOnly: true)
//    .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yaml")
//    .WithHttpEndpoint(targetPort: 9090, name: "prometheus-http");

//var grafana = builder.AddContainer("grafana", "grafana/grafana")
//    //.WithBindMount("./Config/grafana.yaml", "etc/grafana", isReadOnly: true)
//    .WithBindMount("./Config/grafana-dashboard", "/var/lib/grafana/dashboards", isReadOnly: true)
//    .WithEnvironment("PROMETHEUS_ENPOINT", prometheus.GetEndpoint("prometheus-http"))
//    .WithHttpEndpoint(targetPort: 3000, name: "grafana-http");


//var zipkin = builder.AddContainer("zipkin", "openzipkin/zipkin")
//    .WithEndpoint(9411, 9411, "zipkin-http");


var iLoveIbadahIdentityServer = builder.AddPostgres("iLoveIbadahIdentityServer")
    .WithDataVolume(isReadOnly: false)
    .WithHealthCheck("mycheck");
var iLoveIbadahIdentityDB = iLoveIbadahIdentityServer.AddDatabase("iLoveIbadahIdentityDB");

var iLoveIbadahWebsiteServer = builder.AddPostgres("iLoveIbadahWebsiteServer")
    .WithDataVolume(isReadOnly: false)
    .WithHealthCheck("mycheck");
var iLoveIbadahWebsiteDB = iLoveIbadahWebsiteServer.AddDatabase("iLoveIbadahWebsiteDB");


var api = builder.AddProject<Projects.iLoveIbadah_API>("iloveibadah-api")
    .WithHttpEndpoint(5003, 80, "iloveibadah-api-http")
    .WithHttpEndpoint(5004, 443, "iloveibadah-api-https")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
//.WithReference(zipkin);


builder.AddProject<Projects.iLoveIbadah_Website_UI>("iloveibadah-website-ui")
    .WithHttpEndpoint(5001, 8080, "iloveibadah-website-http")
    .WithHttpEndpoint(5002, 8081, "iloveibadah-website-https")
    .WithReference(iLoveIbadahIdentityDB)
    .WithReference(iLoveIbadahWebsiteDB)
    .WaitFor(iLoveIbadahIdentityDB)
    .WaitFor(iLoveIbadahWebsiteDB);

//.WithReference(zipkin);

//var website = builder.AddProject<Projects.iLoveIbadah_Website>("iloveibadah-website")
//    .WithHttpEndpoint(5001, 8080, "iloveibadah-website-http")
//    .WithHttpEndpoint(5002, 8081, "iloveibadah-website-https")
//    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

//var webapp = builder.AddProject<Projects.iLoveIbadah_Blazor_WebApp>("iloveibadah-blazor-webapp")
//    .WithHttpEndpoint(5005, 8082, "iloveibadah-blazor-webapp-http")
//    .WithHttpEndpoint(5006, 8083, "iloveibadah-blazor-webapp-https");




//builder.AddProject<Projects.iLoveIbadah_WebApp>("iloveibadah-webapp");
//.WithReference(zipkin);


//var webapp = builder.AddProject<Projects.iLoveIbadah_Blazor_WebApp>("iloveibadah-blazor-webapp")
//    .WithHttpEndpoint(5005, 8082, "iloveibadah-blazor-webapp-http")
//    .WithHttpEndpoint(5006, 8083, "iloveibadah-blazor-webapp-https");


//var webapp = builder.AddProject<Projects.iLoveIbadah_Blazor_WebApp>("iloveibadah-blazor-webapp")
//    .WithHttpEndpoint(5005, 8082, "iloveibadah-blazor-webapp-http")
//    .WithHttpEndpoint(5006, 8083, "iloveibadah-blazor-webapp-https");




//builder.AddProject<Projects.iLoveIbadah_WebApp>("iloveibadah-webapp");
//.WithReference(zipkin);


//var webapp = builder.AddProject<Projects.iLoveIbadah_Blazor_WebApp>("iloveibadah-blazor-webapp")
//    .WithHttpEndpoint(5005, 8082, "iloveibadah-blazor-webapp-http")
//    .WithHttpEndpoint(5006, 8083, "iloveibadah-blazor-webapp-https");




builder.Build().Run();


//var openTelemetryCollector = builder
//    .AddOpenTelemetryCollector("opentelemetrycollector", "./OpenTelemetryCollector/config.yaml")
//    .WithEnvironment("PROMETHEUS_ENPOINT", $"{prometheus.GetEndpoint("prometheus-http")}/api/v1/otlp");

//var blazorServer = builder.AddProject<Projects.iLoveIbadah_Blazor_Server_UI>("ilobeibadah-blazor-ui")
//    .WithHttpEndpoint(5005, 8082, "iloveibadah-blazor-ui-http")
//    .WithHttpEndpoint(5006, 8083, "iloveibadah-blazor-ui-https");

//var blazorServer = builder.AddProject<Projects.iLoveIbadah_Blazor_Server_UI>("ilobeibadah-blazor-ui")
//    .WithHttpEndpoint(5005, 8082, "iloveibadah-blazor-ui-http")
//    .WithHttpEndpoint(5006, 8083, "iloveibadah-blazor-ui-https");


//api.WithReference(zipkin);
//webapp.WithReference(zipkin);
//website.WithReference(zipkin);