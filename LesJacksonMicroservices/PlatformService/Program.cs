using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction()){
    Console.WriteLine("--> Using SqlServer DB");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}
else {
    Console.WriteLine("--> Using in memory");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>(); 
builder.Services.AddScoped<ICommandDataClient, HttpCommandDataClient>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

builder.Services.AddGrpc();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.MapControllers();
app.MapGrpcService<GrpcPlatformService>();
app.MapGet("Protos/platforms.proto", async context => {
        await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
    });

// app.UseEndpoints(ep => {
//     ep.MapControllers();
//     ep.MapGrpcService<GrpcPlatformService>();

//     ep.MapGet("Protos/platforms.proto", async context => {
//         await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
//     });
// });

DbInitializer.Populate(app, app.Environment);

app.Run();
