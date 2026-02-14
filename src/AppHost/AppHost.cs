var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
.WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres")
.WithImage("ankane/pgvector")
.WithImageTag("latest").WithPgAdmin(config =>
{
    config.WithImageTag("latest");
});

var identityDb = postgres.AddDatabase("identitydb");


var identityserver = builder.AddProject<Projects.OroIdentityServer_Server>("identityserver", "https");

identityserver
.WithReference(identityDb)
.WithReference(seq)
.WaitFor(identityDb);

builder.Build().Run();
