namespace Swick.YarpExtensions;

internal sealed class BufferResponseStreamForReplayMiddleware
{
    private readonly RequestDelegate _next;

    public BufferResponseStreamForReplayMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        context.Response.BufferResponseStreamToMemory();

        return _next(context);
    }
}
