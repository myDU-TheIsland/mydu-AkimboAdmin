using Orleans;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Backend;
using NQ;
using NQ.Interfaces;
using NQutils;
using NQutils.Serialization;
using NQutils.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using NQ.Grains.Core;

public class TeleportName
{
    public string teleportName;
}
public class TeleportConfig
{
    public ulong id;
    public ulong constructId;
    public string teleportName;
    public string type;
}
public class playerName
{
    public string name;
}
public class playerId
{
    public ulong id;
}
public class constructId
{
    public ulong id;
}
public class elementId
{
    public ulong id;
    public ulong constructId;
}
public class AdditemId
{
    public ulong id;
    public ulong itemId;
    public long quantity;
}
public class playerPos
{
    public ulong id;
    public string pos;
}
public class TeleportTag
{
    public string tag;
}
public class CMC
{
    public Dictionary<string, double> singleItems = new();
    public Dictionary<string, double> recursiveSpawnGroups = new();
    public ulong talentId = 0;
}

public class IAM 
{
    public bool enabled = false;
    public int inactivityThreshold;
    public int timerCheck;
    public bool autoReset;
}

public class Config
{
    public bool Debug { get; set; }
    public List<string> Access { get; set; }
    public List<string> TeleportLocations { get; set; }
    
    public CMC itemFilter { get; set; }
    public IAM inactivityManager { get; set; }
}
//Mod class, must be called MyDuMod and implements IMod
public class MyDuMod : IMod, ISubObserver
{

    private IServiceProvider isp;
    private IClusterClient orleans;
    private ILogger logger;
    private ISql isql;
    private HttpClient client;
    private CMC itemFilter = new();
    private bool debugState = true;
    private ConcurrentDictionary<ulong, bool> hasPanel = new();
    private ConcurrentDictionary<ulong, bool> hasConstructPanel = new();
    private ConcurrentDictionary<ulong, bool> hasElementPanel = new();
    private List<string> locations = new List<string> { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Yota", "Omega" };
    private List<string> access = new List<string>();
    private string version = "v0.2.37";
    private AkimboInactivityManager akimboInactivity;
    public  void Debugger(string message)
    {
        if (!debugState)
            return;
        AkimboFileFunctions.LogInfo(message);
        //logger.LogInformation(message);
    }
    private void LoadConfigFromFile()
    {
        var filePath = AkimboFileFunctions.GetFilePath("config.json");
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            AkimboFileFunctions.LogInfo("config found");

            // Deserialize the JSON into a dynamic object or a specific class
            var config = JsonConvert.DeserializeObject<Config>(json);

            // Set the variables from the config
            if (config != null)
            {
                locations = config.TeleportLocations;
                debugState = config.Debug;
                itemFilter = config.itemFilter;
                access = config.Access;
                AkimboFileFunctions.debugEnabled = config.Debug;
                if (config.inactivityManager != null) 
                {
                    akimboInactivity.inactivityThreshold = config.inactivityManager.inactivityThreshold;
                    akimboInactivity.timerCheck = config.inactivityManager.timerCheck;
                    akimboInactivity.autoReset = config.inactivityManager.autoReset;
                    akimboInactivity.enabled = config.inactivityManager.enabled;
                    if (akimboInactivity.enabled)
                    {
                        akimboInactivity.StartTimer();
                    }
                    else
                    {
                        AkimboFileFunctions.LogInfo("Inactive Player checks disabled");
                    }

                }
                
            }
        }
        else
        {
            AkimboFileFunctions.LogError("config not found");
        }
    }

    // Helper method to check if the user has the specified role
    private bool HasRole(string role, string roles)
    {
        //AkimboFileFunctions.LogInfo($"checking: {role} in: {roles}");
        if (string.IsNullOrEmpty(roles))
            return false;
        var rolesArray = roles.Split(',');
        return rolesArray.Contains(role) && access.Contains(role);
    }

    // Helper method to remove panels for the player
    private void ClearPlayerPanels(ulong playerId)
    {
        AkimboFileFunctions.LogInfo($"clearing player panels: {playerId}");
        hasPanel.Remove(playerId, out _);
        hasConstructPanel.Remove(playerId, out _);
        hasElementPanel.Remove(playerId, out _);
    }

    // Helper method to create the default ModInfo
    private ModInfo CreateDefaultModInfo()
    {
        AkimboFileFunctions.LogInfo("Creating default ModInfo");
        return new ModInfo
        {
            name = "AkimboAdmin",
            actions = new List<ModActionDefinition>()
        };
    }

    public async Task<bool> CheckRoles(ulong playerId) {
        string roles = null;
        try
        {
            // Fetch user roles from the database
            roles = await isql.QueryRow<string>("SELECT roles from auth where id = (select community_id from player where id = @1);", playerId);
            bool hasEmployees = HasRole("Employees", roles);
            bool hasStaff = HasRole("staff", roles);
            bool hasBO = HasRole("bo", roles);
            bool hasAdmin = HasRole("Administrator", roles);
            bool hasVip = HasRole("vip", roles);
            if (hasEmployees || hasStaff || hasBO || hasAdmin || hasVip)
            {
                AkimboFileFunctions.LogInfo($"Player with {playerId} has permissions to open admin UI");
                return true;
            }
            else 
            {
                AkimboFileFunctions.LogInfo($"Player with {playerId} does not have permissions to open admin UI");
                return false;
            }
        }
        catch (Exception ex)
        {
            AkimboFileFunctions.LogError($"Error fetching roles for player {playerId}: {ex.Message}");
            return false;
        }
    }

    public string GetName()
    {
        return "AkimboAdmin";
    }
    public Task ElementUseStart(string _, NQ.ElementUse eu)
    {
        // wrap around a try statement to catch exceptions
        try
        {
            akimboInactivity.PlayerStartedUsingElement(eu.playerId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log the exception details for debugging
            AkimboFileFunctions.LogError($"An error occurred in ElementPropertyUpdate: {ex.Message}");
            AkimboFileFunctions.LogError(ex.StackTrace);
            return Task.CompletedTask;
        }
    }

    public Task ElementUseStop(string _, NQ.ElementUse eu)
    {
        // wrap around a try statement to catch exceptions
        try
        {
            akimboInactivity.PlayerStoppedUsingElement(eu.playerId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log the exception details for debugging
            AkimboFileFunctions.LogError($"An error occurred in ElementPropertyUpdate: {ex.Message}");
            AkimboFileFunctions.LogError(ex.StackTrace);
            return Task.CompletedTask;
        }
    }

    public Task Initialize(IServiceProvider isp)
    {
        this.isp = isp;
        this.orleans = isp.GetRequiredService<IClusterClient>();
        this.logger = isp.GetRequiredService<ILogger<MyDuMod>>();
        var bank = isp.GetRequiredService<IGameplayBank>();
        this.isql = isp.GetRequiredService<ISql>();
        this.akimboInactivity = new AkimboInactivityManager(isp);
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        this.client = new HttpClient(handler);
        LoadConfigFromFile();
        isp.GetRequiredService<IHookCallManager>().Register(
             "ConstructElementsGrain.ElementUseStart", HookMode.PreCall, this,
             "ElementUseStart");
        isp.GetRequiredService<IHookCallManager>().Register(
             "ConstructElementsGrain.ElementUseStop", HookMode.PreCall, this,
             "ElementUseStop");
        AkimboFileFunctions.LogInfo($"Akimbo AdminHUD {version}: Initialization Done");
        return Task.CompletedTask;
    }

    /// Return a ModInfo to a connecting client
    public async Task<ModInfo> GetModInfoFor(ulong playerId, bool admin)
    {
        // Remove panels for the player
        ClearPlayerPanels(playerId);
        // Create the ModInfo and add actions if the player is authorized
        var res = CreateDefaultModInfo();
        // Check if player can use the UI
        bool cUse = await CheckRoles(playerId);
        // check if player is a bot
        bool pbot = await orleans.GetPlayerGrain(playerId).IsBot();
        // if it aint a bot or an admin && inactivity checks have been abled add player to inactivity
        if (!pbot && akimboInactivity.enabled && !cUse)
        {
            AkimboFileFunctions.LogInfo("inactivity checks activated and player has no admin right, adding player to inactivity checklist");
            akimboInactivity.RemovePlayer(playerId);
            var ppos = await orleans.GetPlayerGrain(playerId).GetPositionUpdate();
            akimboInactivity.AddPlayer(playerId, ppos.universePosition);
        }
        AkimboFileFunctions.LogInfo($"if checks passed the menu will be populated now...");
        if (admin || cUse)
        res.actions.AddRange(new List<ModActionDefinition>
                {
                    new ModActionDefinition
                    {
                        id = 2001,
                        label = "AkimboAdmin\\Open Construct Hud",
                        context = ModActionContext.Construct,
                    },
                    new ModActionDefinition
                    {
                        id = 2002,
                        label = "AkimboAdmin\\Open Element Hud",
                        context = ModActionContext.Element,
                    },
                    new ModActionDefinition
                    {
                        id = 2003,
                        label = "AkimboAdmin\\Players\\Get player id",
                        context = ModActionContext.Avatar,
                    },
                    new ModActionDefinition
                    {
                        id = 2011,
                        label = "AkimboAdmin\\Admin Hud",
                        context = ModActionContext.Global,
                    },
                });
        return await Task<ModInfo>.FromResult(res);
    }

    public async Task TriggerAction(ulong playerId, ModAction action)
    { // Called when a player clicks on one of you Mod's popup entries

        // Always recheck admin status as a bot could force-invoke an action
        // even if not received in your ModInfo
        bool cUse = await CheckRoles(playerId);
        if (!await orleans.GetPlayerGrain(playerId).IsAdmin() && !cUse)
        {
            AkimboFileFunctions.LogError(cUse.ToString());
            AkimboFileFunctions.LogError($"Failed To pass the check to open UI");
            return;
        }
            
        if (action.actionId == 888)
        {/* logData from the UI in Grains_dev.log */
            var js = action.payload;
            AkimboFileFunctions.LogInfo($"data logged: {js}");
        }
        else if (action.actionId == 2011)
        {/* Open HUD */
            var jsContent = "1";
            // if player has not loaded the screen yet inject the code first
            if (!hasPanel.ContainsKey(playerId))
            {
                jsContent = AkimboFileFunctions.LoadJsContent(locations);
            }
            AkimboHudFunctions.AddHudToScreen(playerId, isp, hasPanel, jsContent);
            AkimboBlueprintFunctions.GetBlueprints(isp, playerId, orleans);
        }
        else if (action.actionId == 2001)
        { /* gets the contstruct ID with right click menu */
            AkimboFileFunctions.LogInfo($"getting construct id and construct UI");
            var jsContent = AkimboFileFunctions.LoadConstructJsContent(locations);
            AkimboHudFunctions.AddConstructHudToScreen(action, playerId, isp, orleans, hasPanel, hasConstructPanel, jsContent);
        }
        else if (action.actionId == 2002)
        {/* gets the element ID with right click menu */
            AkimboFileFunctions.LogInfo($"getting Element id and element UI");
            var jsContent = AkimboFileFunctions.LoadElementJsContent(locations);
            AkimboHudFunctions.AddElementHudToScreen(action, playerId, isp, orleans, hasPanel, hasElementPanel, jsContent, locations);
        }
        else if (action.actionId == 2003)
        {/* gets the player ID with right click menu */
            AkimboPlayerFunctions.PlayerId(action, isp, playerId);
        }
        else if (action.actionId == 2004)
        { /* Disown a construct and make it pop */
            var js = action.payload;
            var data = JsonConvert.DeserializeObject<constructId>(js);
            await orleans.GetConstructGrain(data.id).ConstructSetOwner(0, new ConstructOwnerSet { ownerId = new EntityId() }, false);
        }
        else if (action.actionId == 2005)
        {/* repairs elements on a construct , do not use on destroyed cores */
            AkimboElementFunctions.RepairElement(action, orleans);
        }
        else if (action.actionId == 2006)
        { /* Bypassing a admin dispenser to allow all in inventory */
            AkimboFunctions.DispenserOverride(action, orleans, isp, playerId);
        }
        else if (action.actionId == 2007)
        { /* Reset the admin dispenser Bypass */
            AkimboFunctions.DispenserReset(action, orleans, isp, playerId);
        }
        else if (action.actionId == 2008)
        { /* Take over a construct */
            var js = action.payload;
            var data = JsonConvert.DeserializeObject<constructId>(js);
            await orleans.GetConstructGrain(data.id).ConstructSetOwner(playerId, new ConstructOwnerSet { ownerId = new EntityId { playerId = playerId } }, false);
        }
        else if (action.actionId == 2009)
        { /* Repair a construct */
            var js = action.payload;
            var data = JsonConvert.DeserializeObject<constructId>(js);
            await orleans.GetConstructElementsGrain(data.id).RepairAllAdmin();
        }
        else if (action.actionId == 2010)
        { /* Remove DRM from a construct */
            AkimboConstructFunctions.RemoveDrmProtection(action, orleans, isp);
        }
        else if (action.actionId == 2012) 
        {
            AkimboTeleportFunctions.SetTeleporterTag(action, orleans, isp, playerId, new Config { TeleportLocations = locations });
        }
        else if (action.actionId == 3001)
        { /* Add locations for teleporter nodes*/
            AkimboTeleportFunctions.AddTeleporterLocation(action, logger, locations);
        }
        else if (action.actionId == 3002)
        { /* claim an owned territory without searching for territory unit */
            AkimboFunctions.ClaimOwnedTerritory(orleans, isp, playerId);
        }
        else if (action.actionId == 3003)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboFunctions.ClaimUnOwnedTerritory(orleans, isp, playerId);
        }
        else if (action.actionId == 3004)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboPlayerFunctions.GetPlayers(action, orleans, isp, logger, playerId);
        }
        else if (action.actionId == 3005)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboTeleportFunctions.TeleportToPlayer(action, orleans, isp, playerId);
        }
        else if (action.actionId == 3006)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboTeleportFunctions.TeleportPlayerHere(action, orleans, isp, playerId);
        }
        else if (action.actionId == 3007)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboFileFunctions.LogInfo($"event fired: 3007 teleport to custom location");
            AkimboTeleportFunctions.TeleportToCustomLocation(action, orleans, isp, playerId);
        }
        else if (action.actionId == 3008)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboFileFunctions.LogInfo($"event fired: 3008 teleport to custom location");
            AkimboTeleportFunctions.TeleportPlayerToCustomLocation(action, orleans, isp, playerId);
        }
        else if (action.actionId == 3009)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboTeleportFunctions.TeleportToTag(action, orleans, isp, playerId);
        }
        else if (action.actionId == 3010)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboInventoryFunctions.CustomFillInvetory(orleans, isp, playerId, itemFilter);
        }
        else if (action.actionId == 3011)
        { /* claim an unowned territory without placing a territory unit  */
            var js = action.payload;
            AkimboFileFunctions.LogInfo($"data logged: {js}");
            var data = JsonConvert.DeserializeObject<playerId>(js);
            AkimboInventoryFunctions.CustomFillInvetory(orleans, isp, data.id, itemFilter);
        }
        else if (action.actionId == 3012)
        { /* claim an unowned territory without placing a territory unit  */
            AkimboInventoryFunctions.ClearInventory(action, orleans, isp, playerId, itemFilter);
        }
        else if (action.actionId == 3013)
        { /* claim an unowned territory without placing a territory unit  */
            var js = action.payload;
            AkimboFileFunctions.LogInfo($"data logged: {js}");
            var data = JsonConvert.DeserializeObject<playerId>(js);
            AkimboInventoryFunctions.ClearInventory(action, orleans, isp, data.id, itemFilter);
        }
        else if (action.actionId == 3014)
        { 
            /*TO DO ADD ITEM TO INVENTORY */
            var js = action.payload;
            AkimboFileFunctions.LogInfo($"data logged: {js}");
            var data = JsonConvert.DeserializeObject<AdditemId>(js);
            AkimboInventoryFunctions.AddItemToInventory(action, orleans, isp, playerId, data.itemId, data.quantity);
        }
        else if (action.actionId == 3015)
        { 
            /*TO DO ADD ITEM TO OTHER PLAYER INVENTORY */
            var js = action.payload;
            AkimboFileFunctions.LogInfo($"data logged: {js}");
            var data = JsonConvert.DeserializeObject<AdditemId>(js);
            AkimboInventoryFunctions.AddItemToInventory(action, orleans, isp, data.id, data.itemId, data.quantity);
        }
        else if (action.actionId == 3016)
        {
            /* Import Blueprint from URL */
           await AkimboBlueprintFunctions.ImportBlueprint(action, isp, orleans, client, playerId);
        }
        else if (action.actionId == 3017)
        {
            /* Import Blueprint from URL */
            AkimboBlueprintFunctions.ToggleMagicBlueprint(action, isp, orleans, client, playerId);
        }
        else if (action.actionId == 3018) 
        {
            AkimboBlueprintFunctions.AddBpToInventory(action, isp, orleans, playerId);
        }
        else if (action.actionId == 3019)
        {
            AkimboBlueprintFunctions.DeleteBlueprint(action,isp,orleans,client,playerId);
        }
        else if (action.actionId == 3020)
        {
            AkimboFileFunctions.LogInfo($"removing drm protection on Elements");
            AkimboElementFunctions.RemoveDrmProtection(action,  orleans, isp);
        }
    }

    public ConcurrentDictionary<ulong, DateTime> notifs = new();
    public string GetObserverKey() => "ModAdmin"; // From ISubObserver, UID
    public async Task OnSubscriptionMessageReceived(PubSubTopic topic, AbstractPacket message)
    {
        var parser = new NQutils.Messages.Parser(message);
        string extra = "";
        if (parser.ElementsChanged(out var ec)) //of type ElementOperation
        {
            extra = $"element change";
        }
        logger.LogInformation($"PUBSUB {topic.Exchange} {message.MessageType}");
        var cid = ulong.Parse(topic.RoutingKey);
        var cn = (await orleans.GetConstructInfoGrain(cid).Get()).rData.name;
        // rate limit
        if (notifs.TryGetValue(cid, out var dt) && dt >= DateTime.Now.Subtract(TimeSpan.FromMinutes(1)))
            return;
        notifs[cid] = DateTime.Now;
        // spawn external process, whose implementation is left as an exercise to the reader
        var opts = new ProcessStartInfo
        {
            FileName = "/OrleansGrains/Mods/external-notification",
            Arguments = $"'Something is happening on {cn}: {extra} {message.MessageType}'",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };
        try
        {
            using var proc = Process.Start(opts);

            await proc.WaitForExitAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Notification error");
        }
    }
}