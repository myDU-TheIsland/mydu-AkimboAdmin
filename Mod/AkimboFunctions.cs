using Orleans;
using System;
using Backend;
using NQ;
using NQ.RDMS;
using NQ.Interfaces;
using NQutils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;


public class AkimboFunctions
{
    public AkimboFunctions()
    {
    }
   

    public static async void ClaimOwnedTerritory(IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId)
    {
        // Get The current Position from the player
        var playerPosition = await orleans.GetPlayerGrain(playerId).GetPositionUpdate();

        // Extract the local position from the player
        NQ.Vec3 playerLocalPos = playerPosition.localPosition.position;

        // get the current planet
        var planetInfo = await orleans.GetPlayerGrain(playerId).GetPlanet();

        // get the current planet information
        var constructInfo = await orleans.GetConstructInfoGrain(planetInfo.constructId).Get();


        // calculate the tile index using planetPos , radius and tilesize
        var tileIndex1 = await isp.GetRequiredService<IPlanetList>().GetTileIndex(planetInfo.constructId, playerLocalPos);

        // DEBUG: get total tiles calculated from radius and tilesize
        var numberOfTiles = NQutils.Core.Shared.Tile.NQGetNbTiles(constructInfo.planetProperties.altitudeReferenceRadius, constructInfo.planetProperties.territoryTileSize);

        // DEBUG: log all values to grain_dev.log
        AkimboFileFunctions.LogInfo(tileIndex1.ToString() + ' ' + numberOfTiles + ' ' + playerLocalPos + ' ' + planetInfo.constructId + ' ' + constructInfo.planetProperties.altitudeReferenceRadius + ' ' + constructInfo.planetProperties.territoryTileSize);

        // Update the territory information and take ownership
        await orleans.GetPlanetTerritoryGrain(planetInfo.constructId)
        .UpdateTerritory(playerId, new TerritoryUpdate { tileIndex = ((uint)tileIndex1), ownerId = new NQ.EntityId { playerId = playerId } });
    }

    public static async void ClaimUnOwnedTerritory(IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId)
    {
        // Get The current Position from the player
        var playerPosition = await orleans.GetPlayerGrain(playerId).GetPositionUpdate();

        // Extract the local position from the player
        NQ.Vec3 playerLocalPos = playerPosition.localPosition.position;

        // get the current planet
        var planetInfo = await orleans.GetPlayerGrain(playerId).GetPlanet();

        // get the current planet information
        var constructInfo = await orleans.GetConstructInfoGrain(planetInfo.constructId).Get();

        // calculate the tile index using planetPos , radius and tilesize
        var tileIndex1 = await isp.GetRequiredService<IPlanetList>().GetTileIndex(planetInfo.constructId, playerLocalPos);

        // DEBUG: get total tiles calculated from radius and tilesize
        var numberOfTiles = NQutils.Core.Shared.Tile.NQGetNbTiles(constructInfo.planetProperties.altitudeReferenceRadius, constructInfo.planetProperties.territoryTileSize);

        // DEBUG: log all values to grain_dev.log
        AkimboFileFunctions.LogInfo(tileIndex1.ToString() + ' ' + numberOfTiles + ' ' + playerLocalPos + ' ' + planetInfo.constructId + ' ' + constructInfo.planetProperties.altitudeReferenceRadius + ' ' + constructInfo.planetProperties.territoryTileSize);
        var itemId = planetInfo.constructId == 27 ? 3954055294 : planetInfo.constructId == 26 ? 3954055294 : 1358842892;
        var Entity = new NQ.EntityId { playerId = playerId };
        //  take ownership off an unclaimed tile in god mode 
        await orleans.GetPlanetTerritoryGrain(planetInfo.constructId)
            .ClaimTerritoryBySpecialOwner(playerId, new TerritoryClaim { item = new ItemId { ownerId = Entity, typeId = itemId }, planetId = planetInfo.constructId, position = playerLocalPos, ownerId = Entity, name = "My Unclaimed Terrotiry" });
    }

    public static async void DispenserOverride(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId)
    {
        var js = action.payload;
        var data = JsonConvert.DeserializeObject<elementId>(js);
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
        var key = "bypassPrimaryContainer";
        var value = true;
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
                    eventPayload = "CPPHud.addFailureNotification(\"Bypass added successful\");",
                }));
    }
    public static async void DispenserReset(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId) 
    {
        var js = action.payload;
        var data = JsonConvert.DeserializeObject<elementId>(js);
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
        var key = "bypassPrimaryContainer";
        var value = false;
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
                    eventPayload = "CPPHud.addFailureNotification(\"Bypass removed successful\");",
                }));
    }
}
