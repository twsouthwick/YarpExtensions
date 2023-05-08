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

app.Map("/", () => "Hello world!")
    .WithCheckedForwarder("http://localhost:5276", builder =>
    {
        builder.IgnoreDefaultHeaders();
        builder.CompareHeaders();
        builder.CompareStatusCodes();
        builder.BodyMustBeEqual();
    });

// Manually writing to ensure formatting is different
var bodyComparisonGroup = app.MapGroup("/obj")
  .WithCheckedForwarder("http://localhost:5276", builder =>
  {
      builder.IgnoreDefaultHeaders();
      builder.CompareHeaders();
      builder.CompareStatusCodes();
      builder.CompareBody<ResultObj>();
  });


bodyComparisonGroup.Map("/same", () => new ResultObj { A = 5 }); // Same, but different formatting
bodyComparisonGroup.Map("/different", () => new ResultObj { A = 6 }); // A=5 on the remote
bodyComparisonGroup.Map("/none", () => new ResultObj { A = 1 });

app.Run();

record class ResultObj
{
    public int A { get; set; }
}
