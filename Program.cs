using Microsoft.Extensions.Options;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin();
        });
});

builder.Services.Configure<ReveHelper>(builder.Configuration.GetSection("Reve"));

builder.Services.AddTransient<AddHeaderHandler>();
builder.Services.AddHttpClient<ReveClient>(c =>
{
    c.DefaultRequestHeaders.Connection.Add("keep-alive");
    c.Timeout = TimeSpan.FromSeconds(10);
}).AddHttpMessageHandler<AddHeaderHandler>()
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler()
    {
        AutomaticDecompression = DecompressionMethods.GZip
    };
});

var app = builder.Build();

await ReveHelper.LoadCookie();
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetService<IOptionsMonitor<ReveHelper>>().CurrentValue.ToString();
}

// Configure the HTTP request pipeline.

app.UseCors();

app.MapGet("/", () => "Hello World!");

app.MapGet("/list", async (HttpContext ctx, ReveClient client) =>
{
    string dir = ctx.Request.QueryString.Value;
    if (dir.Length < 4)
    {
        return "hello";
    }

    dir = dir[1..];
    return await client.List(dir);
});

app.MapGet("/down/{id}", async (string id, ReveClient client) =>
{
    string url = await client.Down(id);
    return Results.Redirect(url);
});

app.Run();
