using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;

namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures :
    IHttpResponseBodyFeature,
    IHttpResponseFeature
{
    private Stream _responseStream;
    private PipeWriter? _writer;
    private bool _hasStarted;
    private List<(Func<object, Task>, object?)>? _onCompleted;
    private List<(Func<object, Task>, object?)>? _onStarted;

    [MemberNotNull(nameof(_responseStream))]
    private void InitializeResponse(HttpContext context)
    {
        _responseStream = new MemoryStream();
        context.Response.OnCompleted(static stream => ((Stream)stream).DisposeAsync().AsTask(), _responseStream);
    }

    Stream IHttpResponseBodyFeature.Stream => _responseStream;

    PipeWriter IHttpResponseBodyFeature.Writer => _writer ??= PipeWriter.Create(_responseStream);

    Task IHttpResponseBodyFeature.CompleteAsync()
    {
        Debugger.Break();
        return InvokeList(_onCompleted);
    }

    void IHttpResponseBodyFeature.DisableBuffering()
    {
    }

    Task IHttpResponseBodyFeature.SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    async Task IHttpResponseBodyFeature.StartAsync(CancellationToken cancellationToken)
    {
        if (_hasStarted)
        {
            return;
        }

        Debugger.Break();
        _hasStarted = true;
        await InvokeList(_onStarted);
    }

    private async Task InvokeList(List<(Func<object, Task>, object?)>? list)
    {
        if (list is null)
        {
            return;
        }

        foreach (var s in list)
        {
            await s.Item1(s.Item2!);
        }
    }

    int IHttpResponseFeature.StatusCode { get; set; } = StatusCodes.Status200OK;

    string? IHttpResponseFeature.ReasonPhrase { get; set; }

    IHeaderDictionary IHttpResponseFeature.Headers { get; set; } = new HeaderDictionary();

    [Obsolete]
    Stream IHttpResponseFeature.Body
    {
        get => _responseStream;
        set => throw new NotImplementedException();
    }

    bool IHttpResponseFeature.HasStarted => _hasStarted;

    void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
    {
        (_onCompleted ??= new()).Add((callback, state));
    }

    void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
    {
        if (_hasStarted)
        {
            throw new InvalidOperationException("Already started");
        }

        (_onStarted ??= new()).Add((callback, state));
    }
}

