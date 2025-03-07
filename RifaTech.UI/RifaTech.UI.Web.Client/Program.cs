using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RifaTech.UI.Shared.Services;
using RifaTech.UI.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the RifaTech.UI.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
