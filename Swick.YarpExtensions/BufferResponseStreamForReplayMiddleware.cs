namespace Swick.YarpExtensions;

internal sealed class BufferResponseStreamForReplayMiddleware
{
    private readonly RequestDelegate _next;

    public BufferResponseStreamForReplayMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        // Buffer response stream so we can compare it
        using var stream = new BufferingReadWriteStream();
        var current = context.Response.Body;
        context.Response.Body = stream;

        await _next(context);

        stream.Position = 0;
        await stream.CopyToAsync(current);
    }
}