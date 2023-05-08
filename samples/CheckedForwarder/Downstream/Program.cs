var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Map("/", () => "Hello world!");

// Manually writing to ensure formatting is different
app.Map("/obj/same", (HttpResponse response) =>
{
    response.ContentType = "application/json; charset=utf-8";
    return response.WriteAsync("""{ "A":  5 }""");
});

app.Map("/obj/different", (HttpResponse response) =>
{
    response.ContentType = "application/json; charset=utf-8";
    return response.WriteAsync("""{ "A":  5 }""");
});

app.Run();
