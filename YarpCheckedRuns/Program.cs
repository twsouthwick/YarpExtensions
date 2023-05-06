using Microsoft.Net.Http.Headers;
using Swick.YarpExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCheckedForwarder(options =>
{
    options.IgnoredHeaders.Add(HeaderNames.Server);
    //options.IgnoredHeaders.Add(HeaderNames.Date);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCheckedForwarder();

app.MapGet("/d", () => "here");

app.Map("/", () => "Hello world!")
    .WithCheckedYarp("http://localhost:5276");

app.Run();
