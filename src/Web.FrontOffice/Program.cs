using Web.FrontOffice.Components;
using Web.FrontOffice.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFrontOfficeHealthChecks(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapFrontOfficeHealthChecks();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
