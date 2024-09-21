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
using NQ.RDMS;
using NQ.Interfaces;
using NQutils;
using NQutils.Net;
using NQutils.Serialization;
using NQutils.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Runtime;
using Backend.AWS;
using NQ.Visibility;
using NQutils.Messages;
public class AkimboConstructFunctions
{
	public AkimboConstructFunctions()
	{
	}
    public static async void ConstructId(ModAction action, IServiceProvider isp, NQ.PlayerId playerId)
    {
        AkimboFileFunctions.LogInfo("getting construct id to popup");
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.PopupReceived(new PopupMessage
            {
                message = $"Constructid: {action.constructId}",
                target = playerId,
            }));
    }
    public static async void RemoveDrmProtection(ModAction action, IClusterClient orleans, IServiceProvider isp)
    {
        AkimboFileFunctions.LogInfo("removing drm");
        var js = action.payload;
        var data = JsonConvert.DeserializeObject<constructId>(js);
        ElementId coreId = await orleans.GetConstructGrain(data.id).GetCoreUnitId();
        var key = "drmProtected";
        var value = false;
        await orleans.GetConstructElementsGrain(data.id).UpdateElementProperty(
            new NQ.ElementPropertyUpdate
            {
                constructId = data.id,
                elementId = coreId,
                name = key,
                value = new PropertyValue(value),
                timePoint = TimePoint.Now(),
            });
    }
}
