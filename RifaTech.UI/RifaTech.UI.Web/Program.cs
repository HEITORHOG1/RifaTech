using MudBlazor.Services;
using RifaTech.UI.Shared.Services;
using RifaTech.UI.Web.Components;
using RifaTech.UI.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices(); // Add this line
builder.Services.AddMudServices(); // Add this line to register MudBlazor services

// Add device-specific services used by the RifaTech.UI.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
// Add this to your existing Program.cs where services are configured
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5024")
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(RifaTech.UI.Shared._Imports).Assembly,
        typeof(RifaTech.UI.Web.Client._Imports).Assembly);

app.Run();
