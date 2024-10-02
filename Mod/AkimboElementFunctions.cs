using Orleans;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Backend;
using Backend.Business;
using Backend.Database;
using NQutils.Config;
using Backend.Storage;
using Backend.Scenegraph;
using NQ;
using NQ.Interfaces;
using NQutils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

public class AkimboElementFunctions
{
	public AkimboElementFunctions()
	{
	}
	public static async void ElementId(ModAction action, IServiceProvider isp, NQ.PlayerId playerId)
	{
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
               new NQutils.Messages.PopupReceived(new PopupMessage
               {
                   message = $"ElementId: {action.elementId}",
                   target = playerId,
               }));
    }

    public static async void RepairElement(ModAction action, IClusterClient orleans)
    {
        var js = action.payload;
        var data = JsonConvert.DeserializeObject<elementId>(js);
        await orleans.GetConstructElementsGrain(data.constructId)
                .UpdateElementProperty(new NQ.ElementPropertyUpdate
                {
                    timePoint = TimePoint.Now(),
                    elementId = data.id,
                    constructId = data.constructId,
                    name = "hitpointsRatio",
                    value = new PropertyValue(1.0),
                });
    }

    public static async void RemoveDrmProtection(ModAction action, IClusterClient orleans, IServiceProvider isp)
    {
        AkimboFileFunctions.LogInfo("Attempting to remove DRM protection");

        try
        {
            // Deserialize the payload
            var js = action.payload;
            var data = JsonConvert.DeserializeObject<elementId>(js);
            AkimboFileFunctions.LogInfo("elementId: " + data.id.ToString() +" constructId: "+ data.constructId.ToString());
            // Set DRM property values
            var key = "drmProtected";
            var value = false;

            // Attempt to update the element property
            await orleans.GetConstructElementsGrain(data.constructId).UpdateElementProperty(
                new NQ.ElementPropertyUpdate
                {
                    constructId = data.constructId,
                    elementId = data.id,
                    name = key,
                    value = new PropertyValue(value),
                    timePoint = TimePoint.Now(),
                });

            AkimboFileFunctions.LogInfo($"DRM protection removed for element ID {data.id}");
        }
        catch (JsonSerializationException ex)
        {
            AkimboFileFunctions.LogError("Failed to deserialize the payload: " + ex.Message);
            await AkimboNotifications.ErrorNotif(isp, action.playerId, "Failed to process DRM removal request: invalid data.");
        }
        catch (Exception ex)
        {
            AkimboFileFunctions.LogError("An error occurred while removing DRM protection: " + ex.Message);
            await AkimboNotifications.ErrorNotif(isp, action.playerId, "Failed to remove DRM protection: unexpected error.");
        }
    }
}
