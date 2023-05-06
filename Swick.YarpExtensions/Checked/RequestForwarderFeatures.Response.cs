using System.IO.Pipelines;

namespace Swick.YarpExtensions.Checked;

partial class RequestForwarderFeatures :
    IHttpResponseBodyFeature,
    IHttpResponseFeature
{
    Stream IHttpResponseBodyFeature.Stream => _responseStream;

    PipeWriter IHttpResponseBodyFeature.Writer => throw new NotImplementedException();

    Task IHttpResponseBodyFeature.CompleteAsync()
        => throw new NotImplementedException();

    void IHttpResponseBodyFeature.DisableBuffering()
    {
    }

    Task IHttpResponseBodyFeature.SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    Task IHttpResponseBodyFeature.StartAsync(CancellationToken cancellationToken)
        => throw new NotImplementedException();

    int IHttpResponseFeature.StatusCode { get; set; } = StatusCodes.Status200OK;

    string? IHttpResponseFeature.ReasonPhrase { get; set; }

    IHeaderDictionary IHttpResponseFeature.Headers { get; set; } = new HeaderDictionary();

    [Obsolete]
    Stream IHttpResponseFeature.Body
    {
        get => _responseStream;
        set => throw new NotImplementedException();
    }

    bool IHttpResponseFeature.HasStarted => false;

    void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        => throw new NotImplementedException();

    void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
        => throw new NotImplementedException();
}

