var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithRedisCommander();

builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
