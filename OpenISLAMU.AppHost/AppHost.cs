var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OpenISLAMU_Blazor>("openislamu-blazor");

builder.Build().Run();
