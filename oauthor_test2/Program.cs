using Microsoft.AspNetCore.Mvc;
using oauthor_test2.Components;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();


var grant = app.MapGroup("/grant");
grant.MapGet("/google", (IConfiguration config, HttpContext httpContext) =>
{
    var google = config.GetSection("Authenticates:Google");
    var clientId = google.GetSection("ClientId").Value;
    var opAuthUri = google.GetSection("AuthUri").Value;
    var scope = "openid email profile";
    var responseType = "code";
    var redirectUri = google.GetSection("RedirectUri").Value;
    var location = $"{opAuthUri}?response_type={responseType}&scope={scope}" +
      $"&redirect_uri={redirectUri}&client_id={clientId}";

    httpContext.Response.Redirect(location);
});



var onConsent = app.MapGroup("/on-consent");
onConsent.MapGet("/google",async  (IConfiguration config,
    [FromQuery(Name = "code")] string authzCode) =>
{
    var google = config.GetSection("Authenticates:Google");

    var opTokenUri = google.GetSection("TokenUri").Value;
    var grantType = "authorization_code";
    var redirectUri = google.GetSection("RedirectUri").Value;
    var clientId = google.GetSection("ClientId").Value;
    var clientSecret = google.GetSection("ClientSecret").Value;

    var location = $"{opTokenUri}?&grant_type={grantType}&code={authzCode}" +
        $"&redirect_uri={redirectUri}&client_id={clientId}&client_secret={clientSecret}";

    var body = (await new HttpClient().PostAsync(location, null))
        .Content.ReadFromJsonAsync<dynamic>();
    // ID 토큰의 정보 확인
    // 자체 인증 티켓(신분증) 발급(쿠키 발급)
    return body;
});

//TESTTAG a
app.MapRazorComponents<App>();

app.Run();
