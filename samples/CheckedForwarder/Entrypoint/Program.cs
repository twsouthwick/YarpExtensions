using Swick.YarpExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpForwarder();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Status");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCheckedForwarder();

const string Destination = "http://localhost:5276";

app.Map("/", () => "Hello world!")
    .WithCheckedForwarder(Destination, builder =>
    {
        builder.UseWhen(ctx => ValueTask.FromResult(true));
        builder.UseStatusCode();
        builder.UseHeaders(context =>
        {
            context.IgnoreDefault();
        });
        builder.UseBody();
    });

// Manually writing to ensure formatting is different
var bodyComparisonGroup = app.MapGroup("/obj")
  .WithCheckedForwarder(Destination, builder =>
  {
      builder.UseStatusCode();
      builder.UseHeaders(context =>
      {
          context.IgnoreDefault();
      });
      builder.UseJsonBody<ResultObj>();
  });


bodyComparisonGroup.Map("/same", () => new ResultObj { A = 5 }); // Same, but different formatting
bodyComparisonGroup.Map("/different", () => new ResultObj { A = 6 }); // A=5 on the remote
bodyComparisonGroup.Map("/none", () => new ResultObj { A = 1 });

app.Run();

record class ResultObj
{
    public int A { get; set; }
}
