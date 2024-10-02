using Orleans;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Backend;
using Backend.Scenegraph;
using NQ;
using NQ.RDMS;
using NQ.Interfaces;
using NQutils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


public class AkimboTeleportFunctions
{
	public AkimboTeleportFunctions()
	{
	}
    public static async void SetTeleporterTag(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId, Config config)
    {
        var js = action.payload;
        var data = JsonConvert.DeserializeObject<TeleportConfig>(js);
        var cid = data.constructId;
        var eid = data.id;
        var right = await orleans.GetRDMSRightGrain(playerId).GetRightsForPlayerOnAsset(
            playerId,
            new AssetId
            {
                type = AssetType.Element,
                construct = cid,
                element = eid,
            },
            true);
        if (!right.rights.Contains(Right.ElementEdit))
        {
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = "CPPHud.addFailureNotification(\"You do not have permissions on this element to configure teleporter\");",
                }));
            return;
        }
        var key = data.type == "destination" ? "teleport_destination" : "gameplayTag";
        var value = data.teleportName;
        await orleans.GetConstructElementsGrain(cid).UpdateElementProperty(
            new NQ.ElementPropertyUpdate
            {
                constructId = cid,
                elementId = eid,
                name = key,
                value = new PropertyValue(value),
                timePoint = TimePoint.Now(),
            });
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = "CPPHud.addFailureNotification(\"Teleportation configuration successful\");",
                }));
    }

    public static void AddTeleporterLocation(ModAction action, ILogger logger, List<string> locations)
    {
        var js = action.payload;
        var data = JsonConvert.DeserializeObject<TeleportName>(js);
        AkimboFileFunctions.LogInfo($"Adding teleport name: {data.teleportName}");

        if (data != null && !string.IsNullOrEmpty(data.teleportName))
        {
            locations.Add(data.teleportName);
           AkimboFileFunctions.SaveNamesToFile(false,locations);  // Save updated list to file
            AkimboFileFunctions.LogInfo($"Added teleport name: {data.teleportName}");
        }
        else
        {
            AkimboFileFunctions.LogError("Failed to add teleport name: payload is null or teleportName is empty.");
        }
    }
    private static async void StartOrCancelTeleport(IClusterClient orleans, IServiceProvider isp, string tag, NQ.PlayerId pid, bool playerInzone, int delay)
    {
        AkimboFileFunctions.LogInfo(tag);

        LocationDescriptor descriptor = new LocationDescriptor
        {
            propertyName = "gameplayTag",
            propertyValue = tag,  // First attempt using tag directly
            algorithm = "closest"
        };

        string str = LocatorKeyHelper.GrainKey(descriptor);
        AkimboFileFunctions.LogInfo("teleportTag found: " + str);

        // Get the player's current position
        var playerPosition = await orleans.GetPlayerGrain(pid).GetPositionUpdate();
        NQ.DetailedLocation test = null;  // Initialize to handle scope issues

        try
        {
            // Try to get the locator with the initial propertyValue
            test = await orleans.GetLocatorGrain(descriptor).Get(pid, playerPosition.localPosition);
            AkimboFileFunctions.LogInfo("Location found using tag: " + test.ToString());
        }
        catch (Exception ex)
        {
            AkimboFileFunctions.LogError($"Failed to find location using tag: {tag}, trying fallback. Error: {ex.Message}");

            // Retry with fallback propertyValue
            descriptor.propertyValue = $"mod_teleporter_{pid}_" + tag;

            try
            {
                str = LocatorKeyHelper.GrainKey(descriptor);
                AkimboFileFunctions.LogInfo("Retrying with fallback teleportTag: " + str);

                // Retry with the modified propertyValue
                test = await orleans.GetLocatorGrain(descriptor).Get(pid, playerPosition.localPosition);
                AkimboFileFunctions.LogInfo("Location found using fallback: " + test.ToString());
            }
            catch (Exception fallbackEx)
            {
                AkimboFileFunctions.LogError("Fallback locator grain failed: " + fallbackEx.Message);
                await AkimboNotifications.ErrorNotif(isp, pid, "Failed to find teleport location after fallback.");
                return; // Exit the function, teleportation cannot proceed
            }
        }

        // Start the countdown if the locator is valid
        playerInzone = true;
        int countdown = delay; // Countdown time in seconds

        for (int i = countdown; i > 0; i--)
        {
            // Notify player of the countdown
            await AkimboNotifications.ErrorNotif(isp, pid, $"Teleportation in {i} seconds");
            await Task.Delay(1000); // Wait for 1 second

            // Check if the player is still in the zone
            if (!playerInzone)
            {
                await AkimboNotifications.ErrorNotif(isp, pid, "Teleportation cancelled");
                return; // Abort teleportation
            }
        }

        // Teleport the player if they are still in the zone after the countdown
        if (test != null && tag != "")
        {
            AkimboFileFunctions.LogInfo("Destination found, teleporting...");
            await AkimboNotifications.ErrorNotif(isp, pid, "Teleporting...");
            await orleans.GetPlayerGrain(pid).TeleportPlayer(test.relative);
        }
        else
        {
            AkimboFileFunctions.LogInfo("Error occurred while trying to teleport");
            await AkimboNotifications.ErrorNotif(isp, pid, "... Error ... Error ... Error ... ");
        }
    }


    public static async void TeleportToPlayer(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId)
    {
        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<playerId>(js);
        AkimboFileFunctions.LogInfo(data.id.ToString());
        var pidpos = await orleans.GetPlayerGrain(data.id).GetPositionUpdate();
        var pidlocpos = pidpos.localPosition;
        AkimboFileFunctions.LogInfo("destination found teleporting");
        await AkimboNotifications.ErrorNotif(isp, playerId, "Teleporting...");
        await orleans.GetPlayerGrain(playerId).TeleportPlayer(pidlocpos);
        AkimboFileFunctions.LogInfo("teleporting succesfull");
    }

    public static async void TeleportPlayerHere(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId)
    {
        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<playerId>(js);
        AkimboFileFunctions.LogInfo(data.id.ToString());
        var pidpos = await orleans.GetPlayerGrain(playerId).GetPositionUpdate();
        var pidlocpos = pidpos.localPosition;
        AkimboFileFunctions.LogInfo("destination found teleporting");
        await AkimboNotifications.ErrorNotif(isp, playerId, "Teleporting Player...");
        await AkimboNotifications.ErrorNotif(isp, data.id, "Teleporting...");
        await orleans.GetPlayerGrain(data.id).TeleportPlayer(pidlocpos);
        AkimboFileFunctions.LogInfo("teleporting succesfull");
    }

    public static async void TeleportToCustomLocation(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId)
    {
        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<playerPos>(js);
        AkimboFileFunctions.LogInfo(data.pos);
        if (data.pos.StartsWith("playertp"))
        {
            // Handle playertp format: playertp 2 planetId x y z
            var regex = new Regex(@"playertp\s(\d+)\s(-?[\d.]+)\s(-?[\d.]+)\s(-?[\d.]+)");
            var match = regex.Match(js);

            if (match.Success)
            {
                ulong planetId = ulong.Parse(match.Groups[1].Value); // Planet ID
                float x = float.Parse(match.Groups[2].Value); // X coordinate
                float y = float.Parse(match.Groups[3].Value); // Y coordinate
                float z = float.Parse(match.Groups[4].Value); // Z coordinate

                AkimboFileFunctions.LogInfo($"Custom Pos from playertp on pid: {planetId} ({x}, {y}, {z})");

                // Use the direct coordinates from playertp
                var absPosition = new NQ.Vec3
                {
                    x = x,
                    y = y,
                    z = z + 50
                };

                // Use the absolute position for teleportation
                var tploc = new NQ.RelativeLocation
                {
                    constructId = planetId,
                    position = absPosition,
                    rotation = NQ.Quat.Identity,
                };

                await orleans.GetPlayerGrain(playerId).TeleportPlayer(tploc);
            }
        }
        else
        {
            var regex = new Regex(@"::pos\{(\d+),(\d+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+)\}");
            var match = regex.Match(js);

            if (match.Success)
            {
                ulong val1 = ulong.Parse(match.Groups[1].Value);
                ulong planetId = ulong.Parse(match.Groups[2].Value); // Planet ID
                float lat = float.Parse(match.Groups[3].Value); // Latitude
                float lon = float.Parse(match.Groups[4].Value); // Longitude
                float alt = float.Parse(match.Groups[5].Value); // Altitude

                AkimboFileFunctions.LogInfo($"Custom Pos on pid: {planetId} ({lat}, {lon}, {alt})");
                if(planetId == 0)
                {
                    var spcPosition = new NQ.Vec3
                    {
                        x = lat,
                        y = lon,
                        z = alt
                    };
                    var tplocsp = new NQ.RelativeLocation
                    {
                        constructId = planetId,
                        position = spcPosition,
                        rotation = NQ.Quat.Identity,
                    };
                    await orleans.GetPlayerGrain(playerId).TeleportPlayer(tplocsp);
                    return;
                }
                // Convert latitude and longitude to radians
                float latRad = MathF.PI * lat / 180f;
                float lonRad = MathF.PI * lon / 180f;

                // planet Radius && planet centerWorld Position
                var pif = await isp.GetRequiredService<IPlanetList>().GetPlanet(planetId);
                var planetRadius = pif.planetProperties.altitudeReferenceRadius;
                AkimboFileFunctions.LogInfo(pif.constructId.ToString() +" "+ planetId);
                var sg = isp.GetRequiredService<IScenegraph>();
                var cidwp = await sg.GetConstructCenterWorldPosition(planetId);
                AkimboFileFunctions.LogInfo($"Planet center: ({cidwp.x}, {cidwp.y}, {cidwp.z}), Radius: {planetRadius}");

                float cosLat = MathF.Cos(latRad);
                var relativeVect = new NQ.Vec3
                {
                    x = (planetRadius + alt) * cosLat * MathF.Cos(lonRad),
                    y = (planetRadius + alt) * cosLat * MathF.Sin(lonRad),
                    z = (planetRadius + alt) * MathF.Sin(latRad)
                };
                AkimboFileFunctions.LogInfo($"Relative position: x={relativeVect.x}, y={relativeVect.y}, z={relativeVect.z}");
                // Calculate the absolute position by adding the planet's center to the relative vector
                var absPosition = new NQ.Vec3
                {
                    x = cidwp.x + relativeVect.x,
                    y = cidwp.y + relativeVect.y,
                    z = cidwp.z + relativeVect.z + 10
                };
                AkimboFileFunctions.LogInfo($"Absolute position: x={absPosition.x}, y={absPosition.y}, z={absPosition.z}");
                // Use the absolute position for teleportation
                var tploc = new NQ.RelativeLocation
                {
                    constructId = 0,
                    position = absPosition,
                    rotation = NQ.Quat.Identity,
                };
                //var wloc = await sg.ResolveWorldLocation(tploc);
                // Debugger("tploc" + tploc.ToString());
                //Debugger("wloc" + wloc.ToString());
                await orleans.GetPlayerGrain(playerId).TeleportPlayer(tploc);
            }
            else
            {
                await AkimboNotifications.ErrorNotif(isp, playerId, "Invalid location received... cancelling");
                return;
            }
        }
    }
    public static async void TeleportPlayerToCustomLocation(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId) {
        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<playerPos>(js);
        AkimboFileFunctions.LogInfo(data.pos);
        if (data.pos.StartsWith("playertp"))
        {
            // Handle playertp format: playertp 2 planetId x y z
            var regex = new Regex(@"playertp\s(\d+)\s(-?[\d.]+)\s(-?[\d.]+)\s(-?[\d.]+)");
            var match = regex.Match(js);

            if (match.Success)
            {
                ulong planetId = ulong.Parse(match.Groups[1].Value); // Planet ID
                float x = float.Parse(match.Groups[2].Value); // X coordinate
                float y = float.Parse(match.Groups[3].Value); // Y coordinate
                float z = float.Parse(match.Groups[4].Value); // Z coordinate

                AkimboFileFunctions.LogInfo($"Custom Pos from playertp on pid: {planetId} ({x}, {y}, {z})");

                // Use the direct coordinates from playertp
                var absPosition = new NQ.Vec3
                {
                    x = x,
                    y = y,
                    z = z + 50
                };

                // Use the absolute position for teleportation
                var tploc = new NQ.RelativeLocation
                {
                    constructId = planetId,
                    position = absPosition,
                    rotation = NQ.Quat.Identity,
                };

                await orleans.GetPlayerGrain(playerId).TeleportPlayer(tploc);
            }
        }
        else
        {
            var regex = new Regex(@"::pos\{(\d+),(\d+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+)\}");
            var match = regex.Match(js);

            if (match.Success)
            {
                ulong val1 = ulong.Parse(match.Groups[1].Value);
                ulong planetId = ulong.Parse(match.Groups[2].Value); // Planet ID
                float lat = float.Parse(match.Groups[3].Value); // Latitude
                float lon = float.Parse(match.Groups[4].Value); // Longitude
                float alt = float.Parse(match.Groups[5].Value); // Altitude

                AkimboFileFunctions.LogInfo($"Custom Pos on pid: {planetId} ({lat}, {lon}, {alt})");
                if (planetId == 0)
                {
                    var spcPosition = new NQ.Vec3
                    {
                        x = lat,
                        y = lon,
                        z = alt
                    };
                    var tplocsp = new NQ.RelativeLocation
                    {
                        constructId = planetId,
                        position = spcPosition,
                        rotation = NQ.Quat.Identity,
                    };
                    await orleans.GetPlayerGrain(playerId).TeleportPlayer(tplocsp);
                    return;
                }
                // Convert latitude and longitude to radians
                float latRad = MathF.PI * lat / 180f;
                float lonRad = MathF.PI * lon / 180f;

                // planet Radius && planet centerWorld Position
                var pif = await isp.GetRequiredService<IPlanetList>().GetPlanet(planetId);
                var planetRadius = pif.planetProperties.altitudeReferenceRadius;

                var sg = isp.GetRequiredService<IScenegraph>();
                var cidwp = await sg.GetConstructCenterWorldPosition(planetId);
                AkimboFileFunctions.LogInfo($"Planet center: ({cidwp.x}, {cidwp.y}, {cidwp.z}), Radius: {planetRadius}");

                float cosLat = MathF.Cos(latRad);
                var relativeVect = new NQ.Vec3
                {
                    x = (planetRadius + alt) * cosLat * MathF.Cos(lonRad),
                    y = (planetRadius + alt) * cosLat * MathF.Sin(lonRad),
                    z = (planetRadius + alt) * MathF.Sin(latRad)
                };
                AkimboFileFunctions.LogInfo($"Relative position: x={relativeVect.x}, y={relativeVect.y}, z={relativeVect.z}");
                // Calculate the absolute position by adding the planet's center to the relative vector
                var absPosition = new NQ.Vec3
                {
                    x = cidwp.x + relativeVect.x,
                    y = cidwp.y + relativeVect.y,
                    z = cidwp.z + relativeVect.z + 10
                };
                AkimboFileFunctions.LogInfo($"Absolute position: x={absPosition.x}, y={absPosition.y}, z={absPosition.z}");
                // Use the absolute position for teleportation
                var tploc = new NQ.RelativeLocation
                {
                    constructId = 0,
                    position = absPosition,
                    rotation = NQ.Quat.Identity,
                };
                //var wloc = await sg.ResolveWorldLocation(tploc);
                // Debugger("tploc" + tploc.ToString());
                //Debugger("wloc" + wloc.ToString());
                await orleans.GetPlayerGrain(data.id).TeleportPlayer(tploc);
            }
            else
            {
                await AkimboNotifications.ErrorNotif(isp, playerId, "Invalid location received... cancelling");
                return;
            }
        }
    }

    public static void TeleportToTag(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId)
    {
        var js = action.payload;
        var data = JsonConvert.DeserializeObject<TeleportTag>(js);
        AkimboFileFunctions.LogInfo($"received data: {data.tag}");
        StartOrCancelTeleport(orleans, isp ,data.tag, playerId,true,3);
    }
}
