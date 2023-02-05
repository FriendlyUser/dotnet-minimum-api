using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Ip Api",
        Description = "An ASP.NET Core Web API for managing calling ip addresses",
        // TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Friendlyuser",
            Url = new Uri("friendlyuser.github.io")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});

// builder.Services.AddRazorPages(options =>
// {
//     // options.Conventions.AuthorizeFolder("/MyPages/Admin");
// })
//   .WithRazorPagesRoot("/Pages");

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// app.MapRazorPages();    

app.MapGet("/", () => "Hello World!").WithName("Hello World")
.WithOpenApi();

var ipItems = app.MapGroup("/ip");

ipItems.MapGet("/", GetIpAddressAsync).WithName("GetIpAddress")
.WithOpenApi();

// info with argument for ip address
ipItems.MapGet("/{ip}", async (string ip) =>
{
    return await GetIpInfoAsync(ip);
}).WithName("GetIp").WithOpenApi();

// info 
ipItems.MapGet("/info", async (HttpContext context) =>
{
    // get ip for requester
    string? ip;
    ip =   context?.Connection?.RemoteIpAddress.ToString();
    if (string.IsNullOrEmpty(ip))
    {
        ip =  context.Request.Headers["X-Forwarded-For"].ToString();
    }
    return await GetIpInfoAsync(ip);
}).WithName("Get Server Ip Info").WithOpenApi();

// read port variable from PORT env var
var port = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrEmpty(port))
{
    port = "7860";
} 
var url = $"https://0.0.0.0:{port}";
app.Run(url);

static async System.Threading.Tasks.Task<string> GetIpAddressAsync()
{
    {
        try
        {
            using HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync("https://jsonip.com");
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            using StreamReader reader = new StreamReader(responseStream);

            string json = await reader.ReadToEndAsync();
            // exception here for minimum api
            if (json == null || json == "")
            {
                throw new Exception("Invalid response");
            }
            JsonIpResponse jsonResponse = JsonSerializer.Deserialize<JsonIpResponse>(json);
            if (jsonResponse == null)
            {
                throw new Exception("Invalid response");
            }
            return jsonResponse.ip;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw e;
        }
    }
}

static async System.Threading.Tasks.Task<IPResponse> GetIpInfoAsync(string ip)
{
    {
        try
        {
            using HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync($"https://iplist.cc/api/{ip}");
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            using StreamReader reader = new StreamReader(responseStream);

            string json = await reader.ReadToEndAsync();
            // exception here for minimum api
            if (json == null || json == "")
            {
                throw new Exception("Invalid response");
            }
            IPResponse jsonResponse = JsonSerializer.Deserialize<IPResponse>(json);
            if (jsonResponse == null)
            {
                throw new Exception("Invalid response");
            }
            // send as json
            return jsonResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw e;
        }
    }
}

public class JsonIpResponse
{
    public string ip { get; set; }
}
public class IPResponse
{
    public string ip { get; set; }
    public string registry { get; set; }
    public string countrycode { get; set; }
    public string countryname { get; set; }
    public Asn asn { get; set; }
    public bool spam { get; set; }
    public bool tor { get; set; }
    public string city { get; set; }
    public string detail { get; set; }
    public string[] website { get; set; }
}

public class Asn
{
    public string code { get; set; }
    public string name { get; set; }
    public string route { get; set; }
    public string start { get; set; }
    public string end { get; set; }
    public string count { get; set; }
}