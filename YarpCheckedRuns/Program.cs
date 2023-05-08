using Microsoft.Net.Http.Headers;
using Swick.YarpExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCheckedForwarder();

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
        builder.CompareHeaders(HeaderNames.Server, HeaderNames.Date);
        builder.CompareStatusCodes();
        builder.BodyMustBeEqual();
    });

app.Run();
