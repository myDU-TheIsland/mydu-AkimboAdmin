using Backend;
using Backend.AWS;
using Microsoft.Extensions.DependencyInjection;
using NQ;
using NQ.Interfaces;
using NQutils;
using Orleans;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

public class AkimboNotifications
{
	public AkimboNotifications()
	{
	}
    public static async Task ErrorNotif(IServiceProvider isp, NQ.PlayerId pid, string message)
    {
        var sanitizedMessage = message.Replace("\"", "\\\""); // Escape quotes if needed

        await isp.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"CPPHud.addFailureNotification(\"{sanitizedMessage}\");",
                }
            )
        );

        return;
    }

    // delay is in milisecconds , so 5000 is 5 seconds
    public static async Task NetworkNotif(IServiceProvider isp, NQ.PlayerId pid, string message , int delay)
    {
        var sanitizedMessage = message.Replace("\"", "\\\""); // Escape quotes if needed

        // Send the notification to display the message
        await isp.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"networkNotification.setMessage(\"{sanitizedMessage}\",10);",
                }
            )
        );

        // Add a delay to hide the message again
        await Task.Delay(delay); 

        // Hide the message after the delay
        await isp.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"networkNotification.show(false);",
                }
            )
        );

        return;
    }
    // posx and posy will possition the notification on screen
    // duration is in seconds so 5 will mean 5 seconds
    public static async Task HintNotif(IServiceProvider isp, NQ.PlayerId pid, string header, string body, string footer, int posx, int posy, int duration)
    {
        // Escape quotes in the message strings if necessary
        var sanitizedHeader = header.Replace("\"", "\\\"");
        var sanitizedBody = body.Replace("\"", "\\\"");
        var sanitizedFooter = footer.Replace("\"", "\\\"");

        // callable from debug panel : hintNotification.show('{"header":"Test Header","body":"This is the body of the notification.","footer":"Footer text here.","posx":1400,"posy":200,"duration":5}');
        // Construct the JSON payload for hintNotification.show
        var hintNotificationPayload = $"{{\"header\":\"{sanitizedHeader}\",\"body\":\"{sanitizedBody}\",\"footer\":\"{sanitizedFooter}\",\"posx\":{posx},\"posy\":{posy},\"duration\":{duration}}}";

        // Send the notification to display the hint
        await isp.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"hintNotification.show('{hintNotificationPayload}');"
                }
            )
        );

        return;
    }

    public static async Task Notify(IClusterClient orleans, ulong playerId, string message)
    {
        await orleans.GetChatGrain(2).SendMessage(
            new MessageContent
            {
                channel = new MessageChannel
                {
                    channel = MessageChannelType.PRIVATE,
                    targetId = playerId
                },
                message = message,
            });
    }
}
