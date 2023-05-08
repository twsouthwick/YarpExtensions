namespace Swick.YarpExtensions.Checked;

internal interface ICheckedForwarderMetadata
{
    string Destination { get; }

    RequestDelegate Request { get; }

    RequestDelegate Comparison { get; }
}