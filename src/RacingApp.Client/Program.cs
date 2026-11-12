using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RacingApp.Client;
using RacingApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddOptions();

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddHttpClient<ApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient<AuthService>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080/";
    client.BaseAddress = new Uri(baseUrl);
});

await builder.Build().RunAsync();
