using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swick.YarpExtensions.Checked;

namespace Swick.YarpExtensions.Comparer;

internal class ContextComparerBuilder : IContextComparerBuilder
{
    public ContextComparerBuilder(string destination, IServiceProvider services)
    {
        Destination = destination;
        MainContext = new ForwarderAppBuilder(services);
        ForwardedContext = new ForwarderAppBuilder(services);
        Comparison = new ForwarderAppBuilder(services);
    }

    public IApplicationBuilder MainContext { get; }

    public IApplicationBuilder ForwardedContext { get; }

    public IApplicationBuilder Comparison { get; }

    public string Destination { get; }

    internal ICheckedForwarderMetadata Build() => new Built
    {
        Comparison = Comparison.Build(),
        Destination = Destination,
        ForwardedContext = ForwardedContext.Build(),
        MainContext = MainContext.Build(),
    };

    private sealed class Built : ICheckedForwarderMetadata
    {
        public required string Destination { get; init; }

        public required RequestDelegate MainContext { get; init; }

        public required RequestDelegate ForwardedContext { get; init; }

        public required RequestDelegate Comparison { get; init; }
    }

    private sealed class ForwarderAppBuilder : IApplicationBuilder
    {
        private const string ServerFeaturesKey = "server.Features";
        private const string ApplicationServicesKey = "application.Services";

        private readonly List<Func<RequestDelegate, RequestDelegate>> _components = new();

        public ForwarderAppBuilder(IServiceProvider serviceProvider)
        {
            Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
            ApplicationServices = serviceProvider;

            SetProperty(ServerFeaturesKey, new FeatureCollection());
        }

        private ForwarderAppBuilder(ForwarderAppBuilder builder)
        {
            Properties = new Dictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
        }

        public IServiceProvider ApplicationServices
        {
            get => GetProperty<IServiceProvider>(ApplicationServicesKey)!;
            set => SetProperty<IServiceProvider>(ApplicationServicesKey, value);
        }

        public IFeatureCollection ServerFeatures
        {
            get
            {
                return GetProperty<IFeatureCollection>(ServerFeaturesKey)!;
            }
        }

        public IDictionary<string, object?> Properties { get; }

        private T? GetProperty<T>(string key)
        {
            return Properties.TryGetValue(key, out var value) ? (T?)value : default(T);
        }

        private void SetProperty<T>(string key, T value) => Properties[key] = value;

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            _components.Add(middleware);
            return this;
        }

        public IApplicationBuilder New() => new ForwarderAppBuilder(this);

        public RequestDelegate Build()
        {
            var loggerFactory = ApplicationServices?.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<ForwarderAppBuilder>();

            RequestDelegate app = context => Task.CompletedTask;

            for (var c = _components.Count - 1; c >= 0; c--)
            {
                app = _components[c](app);
            }

            return app;
        }
    }
}