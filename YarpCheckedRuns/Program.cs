using Microsoft.Net.Http.Headers;
using Swick.YarpExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpForwarder();

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
    .WithCheckedForwarder("http://localhost:5276", builder =>
    {
        builder.IgnoreDefaultHeaders();
        builder.CompareHeaders();
        builder.CompareStatusCodes();
        builder.BodyMustBeEqual();
    });

app.Run();
