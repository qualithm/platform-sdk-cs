// Example: list the workspace's devices, and report a PlatformException.

using Qualithm.Platform;
using Qualithm.Platform.Api;

try
{
    using var platform = new PlatformClient();
    var page = await platform.Api<DevicesApi>().ListDevicesAsync(limit: 20);
    foreach (var device in page.Data.Items)
    {
        Console.WriteLine($"{device.Id}  {device.SpaceName}  {device.Name}");
    }
}
catch (PlatformException e)
{
    Console.WriteLine($"error: {e.Message} ({e.Code})");
    return 1;
}

return 0;
