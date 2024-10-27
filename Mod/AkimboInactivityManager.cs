using Microsoft.Extensions.DependencyInjection;
using NQ;
using NQ.Interfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Timers;
using static AkimboPlayerFunctions;

public class PlayerActivityStatus
{
    public Vec3 Position { get; set; }
    public bool IsUsingElement { get; set; }
    public DateTime LastActivityTime { get; set; }

    public PlayerActivityStatus(Vec3 position)
    {
        Position = position;
        IsUsingElement = false;
        LastActivityTime = DateTime.UtcNow;
    }
}
public class AkimboInactivityManager
{
    private static readonly Dictionary<ulong, PlayerActivityStatus> onlinePlayers = new Dictionary<ulong, PlayerActivityStatus>();
    private Timer inactivityCheckTimer;
    public  int inactivityThreshold { get; set; } = 300; // Threshold in seconds (e.g., 5 minutes)
    public  int timerCheck { get; set; } = 60000;

    public  bool autoReset { get; set; } = true;
    public bool enabled { get; set; } = false;
    private static IServiceProvider isp;
    public AkimboInactivityManager(IServiceProvider serviceProvider)
	{
        isp = serviceProvider; // Initialize the service provider
        
    }
    public void StartTimer() 
    {
        inactivityCheckTimer = new Timer(timerCheck); // Check every minute (60000ms)
        inactivityCheckTimer.Elapsed += CheckForInactivePlayers;
        inactivityCheckTimer.AutoReset = autoReset;
        inactivityCheckTimer.Start();
    }
    // Call this when a player logs in
    public void AddPlayer(ulong playerId, Vec3 position)
    {
        AkimboFileFunctions.LogInfo($"player with {playerId} added to the activity list");
        onlinePlayers[playerId] = new PlayerActivityStatus(position);
    }

    // Call this when a player moves
    public void PlayerPositionUpdate(ulong playerId, Vec3 newPosition, bool updateTime = true)
    {
        if (onlinePlayers.TryGetValue(playerId, out var status))
        {
            status.Position = newPosition;
            status.LastActivityTime = DateTime.UtcNow;
        }
    }

    // Call this when a player starts using an element
    public void PlayerStartedUsingElement(ulong playerId)
    {
        if (onlinePlayers.TryGetValue(playerId, out var status))
        {
            AkimboFileFunctions.LogInfo($"Player {playerId} started using a element");
            status.IsUsingElement = true;
        }
    }

    // Call this when a player stops using an element
    public void PlayerStoppedUsingElement(ulong playerId)
    {
        if (onlinePlayers.TryGetValue(playerId, out var status))
        {
            AkimboFileFunctions.LogInfo($"Player {playerId} stopped using a element");
            status.IsUsingElement = false;
            status.LastActivityTime = DateTime.UtcNow; // Update activity time when they stop using the element
        }
    }

    private bool ArePositionsEqual(Vec3 pos1, Vec3 pos2, double tolerance = 0.01)
    {
        return Math.Abs(pos1.x - pos2.x) < tolerance &&
               Math.Abs(pos1.y - pos2.y) < tolerance &&
               Math.Abs(pos1.z - pos2.z) < tolerance;
    }

    private async void CheckForInactivePlayers(object sender, ElapsedEventArgs e)
    {
        AkimboFileFunctions.LogInfo("Inactive player check starting");

        try
        {
            if(isp == null) { return; }
            var now = DateTime.UtcNow;
            IClusterClient orleans = isp.GetRequiredService<IClusterClient>();

            // Iterate through a copy of the onlinePlayers dictionary
            foreach (var (playerId, status) in new Dictionary<ulong, PlayerActivityStatus>(onlinePlayers))
            {
                var ppos = await orleans.GetPlayerGrain(playerId).GetPositionUpdate();

                // Check if the player is not using an element and has been inactive for too long
                if (!status.IsUsingElement && (now - status.LastActivityTime).TotalSeconds > inactivityThreshold && ArePositionsEqual(status.Position, ppos.universePosition))
                {
                    DisconnectPlayer(playerId);
                    onlinePlayers.Remove(playerId);
                }

                // Alert the player about the remaining time before disconnection
                if (!status.IsUsingElement && ArePositionsEqual(status.Position, ppos.universePosition))
                {
                    // Calculate the remaining time before disconnection
                    double timeElapsed = (DateTime.UtcNow - status.LastActivityTime).TotalSeconds;
                    double timeRemaining = inactivityThreshold - timeElapsed;

                    // Convert the time remaining to minutes and seconds
                    int minutes = (int)(timeRemaining / 60);
                    int seconds = (int)(timeRemaining % 60);

                    // Format the message to include the remaining time
                    string message = timeRemaining > 0
                        ? $"Alert: inactivity detected. Time remaining before disconnect: {minutes} minutes and {seconds} seconds."
                        : "Alert: inactivity detected. Disconnection is imminent.";

                    await AkimboNotifications.ErrorNotif(isp, playerId, message);
                    AkimboFileFunctions.LogInfo($"Player with id {playerId} detected inactive disconnecting in {minutes} minutes and {seconds} seconds.");
                }
                else
                {
                    //AkimboFileFunctions.LogInfo($"Player position update after checking inactivity. Position: x = {Math.Round(ppos.universePosition.x, 2)}, y = {Math.Round(ppos.universePosition.y, 2)}, z = {Math.Round(ppos.universePosition.z, 2)}");
                    PlayerPositionUpdate(playerId, ppos.universePosition);
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Log at a lower severity level since this is expected during shutdown.
            AkimboFileFunctions.LogInfo("Service provider or dependency has been disposed, likely due to server shutdown.");
        }
        catch (Exception ex)
        {
            // Log the error and notify the system about the failure
            AkimboFileFunctions.LogError($"An error occurred during inactivity check: {ex.Message}");
            await AkimboNotifications.ErrorNotif(isp, 0, $"An error occurred during inactivity check: {ex.Message}");
        }
    }

    private async void DisconnectPlayer(ulong playerId)
    {
        try
        {
            AkimboFileFunctions.LogInfo($"Player {playerId} has been disconnected due to inactivity.");
            var orleans = isp.GetRequiredService<IClusterClient>();
            await orleans.GetPlayerDirectoryGrain().Disconnect(playerId, new DisconnectionNotification
            {
                reconnectionDelay = 5000,
                reason = DisconnectionStatus.TIMED_ACTION
            });
            
        }
        catch (Exception ex)
        {
            // Log the error for debugging purposes
            AkimboFileFunctions.LogError($"Error while disconnecting player {playerId}: {ex.Message}");
            // Optionally log the stack trace for more details
            AkimboFileFunctions.LogError($"Stack Trace: {ex.StackTrace}");
        }
    }

    public void RemovePlayer(ulong playerId)
    {
        if (onlinePlayers.ContainsKey(playerId))
        {
            AkimboFileFunctions.LogInfo($"player with id {playerId} has been removed from the activity list");
            onlinePlayers.Remove(playerId);
        }
    }
}
