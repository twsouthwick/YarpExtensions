namespace Swick.YarpExtensions.Checked;

internal interface ICheckedForwarderMetadata
{
    string Destination { get; }

    RequestDelegate MainContext { get; }

    RequestDelegate ForwardedContext { get; }

    RequestDelegate Comparison { get; }
}