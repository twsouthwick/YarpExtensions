using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.Features;
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

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

app.MapGet("/d", () => "here");

app.MapGet("/", () => "Hello")
    .WithCheckedResult("http://localhost:5276");

app.Run();


static class CheckedExtensions
{
    public static T WithCheckedResult<T>(this T builder, string destination)
        where T : IEndpointConventionBuilder
    {
        var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            ConnectTimeout = TimeSpan.FromSeconds(15),
        });

        builder.AddEndpointFilter(async (context, next) =>
        {
            var forwarder = context.HttpContext.RequestServices.GetRequiredService<IHttpForwarder>();
            var features = new TrackingFeatures(context.HttpContext.Features);
            var resultContext = new DefaultHttpContext(features);

            var error = await forwarder.SendAsync(resultContext, destination, httpClient);

            var writer = new StringWriter();
            features.WriteTo("TT", writer);
            var r = writer.ToString();
            return null;
        });

        return builder;
    }

    private class TrackingFeatures : IFeatureCollection
    {
        private readonly IFeatureCollection _other;

        public ICollection<Type> GetTypes { get; } = new HashSet<Type>();

        public ICollection<Type> SetTypes { get; } = new HashSet<Type>();

        public TrackingFeatures(IFeatureCollection features)
        {
            _other = features;
        }

        public object? this[Type key]
        {
            get
            {
                GetTypes.Add(key);
                return _other[key];
            }
            set
            {
                SetTypes.Add(key);
                _other[key] = value;
            }
        }

        public bool IsReadOnly => _other.IsReadOnly;

        public int Revision => _other.Revision;

        public TFeature? Get<TFeature>()
        {
            GetTypes.Add(typeof(TFeature));
            return _other.Get<TFeature>();
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return _other.GetEnumerator();
        }

        public void Set<TFeature>(TFeature? instance)
        {
            SetTypes.Add(typeof(TFeature));
            _other.Set(instance);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_other).GetEnumerator();
        }

        public void WriteTo(string className, TextWriter writer)
        {
            using var indented = new IndentedTextWriter(writer);


            indented.Write("class ");
            indented.Write(className);
            indented.WriteLine(" :");

            var total = GetTypes
                .Concat(SetTypes)
                .Distinct()
                .OrderBy(o => o.Name)
                .ToList();

            indented.Indent++;

            for (int i = 0; i < total.Count; i++)
            {
                indented.Write(total[i].FullName);

                if (i + 1 < total.Count)
                {
                    indented.WriteLine(',');
                }
                else
                {
                    indented.WriteLine();
                }
            }

            indented.Indent--;
            indented.WriteLine("{");
            indented.Indent++;

            indented.WriteLine("private readonly IFeatureCollection _other;");
            indented.WriteLine();

            indented.Write("public ");
            indented.Write(className);
            indented.WriteLine("(IFeatureCollection other) => _other = other;");

            indented.Indent--;
            indented.WriteLine("}");
        }
    }

}