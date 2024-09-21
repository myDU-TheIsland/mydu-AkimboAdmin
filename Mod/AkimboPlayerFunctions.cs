using Orleans;
using System;
using System.Collections.Generic;
using Backend;
using NQ;
using NQ.Interfaces;
using NQutils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class AkimboPlayerFunctions
{
	public AkimboPlayerFunctions()
	{
	}
    public static async void PlayerId(ModAction action, IServiceProvider isp, NQ.PlayerId playerId)
    {
        await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                    new NQutils.Messages.PopupReceived(new PopupMessage
                    {
                        message = $"PlayerId: {action.playerId}",
                        target = playerId,
                    }));
    }

    public static async void GetPlayers(ModAction action, IClusterClient orleans, IServiceProvider isp, ILogger logger, NQ.PlayerId playerId)
    {
            var js = action.payload;
            logger.LogInformation($"data logged: {js}");
            var data = JsonConvert.DeserializeObject<playerName>(js);
            logger.LogInformation($"name logged: {data.name}");
            PlayerDescList allplayers = await orleans.GetPlayerDirectoryGrain().FindPlayers(new PlayerName { name = data.name });
            logger.LogInformation(allplayers.players.Count.ToString());
            List<NQ.PlayerId> players = new List<NQ.PlayerId> { };
            await orleans.GetPlayerDirectoryGrain().FilterConnectedPlayers(players);
            logger.LogInformation(players.Count.ToString());
            // Convert the allplayers list to a JSON string
            string allPlayersJson = JsonConvert.SerializeObject(allplayers);
            logger.LogInformation(allPlayersJson);
            await isp.GetRequiredService<IPub>().NotifyTopic(Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "AkimboAdminHud.setPlayers",
                    eventPayload = allPlayersJson,
                }));

    }
}
