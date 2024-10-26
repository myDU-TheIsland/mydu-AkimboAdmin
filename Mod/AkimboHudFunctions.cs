using Orleans;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Backend;
using NQ;
using NQ.Interfaces;
using NQutils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

public class AkimboHudFunctions
{
	public AkimboHudFunctions()
	{
	}
    public static async void AddHudToScreen(NQ.PlayerId playerId, IServiceProvider isp, ConcurrentDictionary<ulong, bool> hasPanel, string jsContent)
    {
        if (!hasPanel.ContainsKey(playerId))
        {
            if (!string.IsNullOrEmpty(jsContent))
            {
                AkimboFileFunctions.LogInfo("injecting js & css");
                await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = jsContent,
                }));
                AkimboFileFunctions.LogInfo("injecting done");
            }
            await Task.Delay(1000);
            hasPanel.AddOrUpdate(playerId, true, (key, oldValue) => true);
        }
        AkimboFileFunctions.LogInfo("toggling panel");
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
        new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
        {
            eventName = "AkimboAdminHud.show",
            eventPayload = "1",
        }));
    }

    public static async void AddConstructHudToScreen(ModAction action, NQ.PlayerId playerId, IServiceProvider isp, IClusterClient orleans, ConcurrentDictionary<ulong, bool> hasPanel, ConcurrentDictionary<ulong, bool> hasConstructPanel, string jsContent)
    {
        AkimboFileFunctions.LogInfo("toggling construct panel");
        if (!hasPanel.ContainsKey(playerId))
        {
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
            {
                eventName = "modinjectjs",
                eventPayload = "CPPHud.addFailureNotification(\"This action depends on the AdminHud to be loaded...\");",
            }));
            return;
        }
        else if(!hasConstructPanel.ContainsKey(playerId))
        {
            AkimboFileFunctions.LogInfo("injecting js & css");
            jsContent = $"var currentConstructId = `{action.constructId}`;\n" + jsContent;
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
            {
                eventName = "modinjectjs",
                eventPayload = jsContent,
            }));
            AkimboFileFunctions.LogInfo("injecting done");
            await Task.Delay(1000);
            hasConstructPanel.AddOrUpdate(playerId, true, (key, oldValue) => true);
        }

        AkimboFileFunctions.LogInfo("toggling construct panel");
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
        new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
        {
            eventName = "AkimboAdminConstructHud.show",
            eventPayload = "1",
        })); 
        var infos = await orleans.GetConstructInfoGrain(action.constructId).Get();
        var cidOwner = await orleans.GetConstructGrain(action.constructId).GetOwner();
        AkimboFileFunctions.LogInfo(cidOwner.playerId.ToString());
        NamedEntity ownerName = new NamedEntity();
        if (cidOwner.playerId != 0)
        {
            ownerName = await orleans.GetPlayerGrain(cidOwner.playerId).GetName();
            AkimboFileFunctions.LogInfo($"organization name {ownerName}");
        }
        
        AkimboFileFunctions.LogInfo(cidOwner.organizationId.ToString());
        if (cidOwner.organizationId != 0) {
            ownerName = await orleans.GetOrganizationGrain(cidOwner.organizationId).GetName();
            AkimboFileFunctions.LogInfo($"organization name {ownerName}");
        }
       
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "AkimboAdminConstructHud.setConstructInfo",
                    eventPayload = JsonConvert.SerializeObject(new
                    {
                        Id = action.constructId.ToString(),
                        Name = infos.rData.name.ToString(),
                        Owner = ownerName,
                        // Add more properties as needed
                    }),
                }));
    }
    public static async void AddElementHudToScreen(ModAction action, NQ.PlayerId playerId, IServiceProvider isp, IClusterClient orleans, ConcurrentDictionary<ulong, bool> hasPanel, ConcurrentDictionary<ulong, bool> hasElementPanel, string jsContent, List<string> locations)
    {
        if (!hasPanel.ContainsKey(playerId))
        {
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
            {
                eventName = "modinjectjs",
                eventPayload = "CPPHud.addFailureNotification(\"This action depends on the AdminHud to be loaded...\");",
            }));
            return;
        }
        else if (!hasElementPanel.ContainsKey(playerId))
        {
            AkimboFileFunctions.LogInfo("injecting js & css");
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
            {
                eventName = "modinjectjs",
                eventPayload = jsContent,
            }));
            AkimboFileFunctions.LogInfo("injecting done");
            await Task.Delay(1000);
            hasElementPanel.AddOrUpdate(playerId, true, (key, oldValue) => true);
        }

        AkimboFileFunctions.LogInfo("toggling element panel");
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
        new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
        {
            eventName = "AkimboAdminElementHud.show",
            eventPayload = "1",
        }));
        bool hasDispenser = false;
        bool hasTeleport = false;
       var etype = await orleans.GetConstructElementsGrain(action.constructId).GetElement(action.elementId);
        if(etype.elementType == 1947803569) {
            hasDispenser = true;
        }
        if (etype.elementType == 3418287402 || etype.elementType == 3018061314 || etype.elementType == 868211900) {
            hasTeleport = true;
        }
        AkimboFileFunctions.LogInfo(etype.Location.position.ToString());
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "AkimboAdminElementHud.setElementInfo",
                    eventPayload = JsonConvert.SerializeObject(new
                    {
                        Id = action.elementId.ToString(),
                        ConstructId = action.constructId.ToString(),
                        Dispensers = hasDispenser,
                        Teleporters = hasTeleport,
                        locations = locations,
                        ElementLocation = etype.Location.position.ToString(),
    }),
                }));
    }
}
