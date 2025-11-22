#:package Aspire.Hosting.Docker@13.0.0-preview.1.25560.3
#:sdk Aspire.AppHost.Sdk@13.0.0
#:project backend.api/backend.api.root/backend.api.root.csproj

var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.backend_api_root>("api");
builder.Build().Run();
