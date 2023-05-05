using Microsoft.AspNetCore.Http.Features;
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
    .WithCheckedYarp("http://localhost:5276", new HttpContextDiffer());

app.Run();
