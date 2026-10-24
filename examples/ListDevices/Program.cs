// Example: list the devices in the token's team, and report a PlatformException.

using Qualithm.Platform;
using Qualithm.Platform.Api;

try
{
    using var platform = new PlatformClient();

    // An API token belongs to one team, so its team list has exactly that team.
    var teams = await platform.Api<TeamsApi>().ListTeamsAsync();
    var team = teams.Data.Items.FirstOrDefault();
    if (team is null)
    {
        Console.WriteLine("error: the token has no team");
        return 1;
    }

    var page = await platform.Api<DevicesApi>().ListDevicesAsync(team.Id, limit: 20);
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
