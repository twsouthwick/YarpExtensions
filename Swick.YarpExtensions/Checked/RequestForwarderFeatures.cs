using System.Security.Claims;

namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures
{
    private readonly IFeatureCollection _other;

    public RequestForwarderFeatures(HttpContext context)
    {
        _other = context.Features;

        InitializeResponse(context);
    }

    private TFeature GetFeature<TFeature>() => _other.Get<TFeature>()!;
}

class MyHttp : HttpContext
{
    public override IFeatureCollection Features => throw new NotImplementedException();

    public override HttpRequest Request => throw new NotImplementedException();

    public override HttpResponse Response => throw new NotImplementedException();

    public override ConnectionInfo Connection => throw new NotImplementedException();

    public override WebSocketManager WebSockets => throw new NotImplementedException();

    public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override IDictionary<object, object?> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Abort()
    {
        throw new NotImplementedException();
    }
}