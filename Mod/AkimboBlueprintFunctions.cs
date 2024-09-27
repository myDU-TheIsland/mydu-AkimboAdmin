using Backend;
using Backend.AWS;
using Backend.Business;
using Backend.Database;
using Backend.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils;
using NQutils.Def;
using NQutils.Sql;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using NQutils.Net;
using static NQutils.Stats.AggregatedStats;
using System.Linq;

public class AkimboBlueprint
{
    public ulong id { get; set; }               // Changed from ulong to int
    public string name { get; set; }          // Remains the same
    public DateTime created_at { get; set; }  // Remains the same
    public ulong creator_id { get; set; }     // Remains the same; compatible with bigint
    public string creator_name { get; set; }  // Remains the same
    public bool free_deploy { get; set; }     // Remains the same
    public long max_use { get; set; }         // Changed from int to long

    public ulong? selectedPlayer; // Now nullable
}

public class AkimboImportBP 
{
    public string searchstr;
    public bool magicUse;
    public ulong? selectedPlayer; // Now nullable
}
public class AkimboBlueprintFunctions
{
    AkimboBlueprintFunctions()
    {

    }

    public static async Task ImportBlueprint(ModAction action, IServiceProvider isp, IClusterClient orleans, HttpClient client, ulong playerId)
    {

        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<AkimboImportBP>(js);
        string url = data.searchstr;

        if (data.selectedPlayer.HasValue) {
            playerId = data.selectedPlayer.Value;
        }

        System.Memory<byte> payload = null;
        try
        {
            payload = await client.GetRaw(url);
        }
        catch (Exception e)
        {
            AkimboFileFunctions.LogInfo("Blueprint download failure on {url}" + url + e);
            await AkimboNotifications.Notify(orleans, playerId, "Download failure at " + url);
            throw;
        }
        BlueprintId bpId = 0;
        try
        {
            bpId = await isp.GetRequiredService<IDataAccessor>().BlueprintImport(payload.Span.ToArray(), new EntityId { playerId = playerId });
        }
        catch (Exception e)
        {
            AkimboFileFunctions.LogInfo("Blueprint import from {url} failure" + url + e);
            await AkimboNotifications.Notify(orleans, playerId, "Import failure at " + url);
            throw;
        }

        var bpInfo = await orleans.GetBlueprintGrain().GetBlueprintInfo(bpId);
        AkimboFileFunctions.LogInfo("bpInfo: " + bpInfo);
        var isql = isp.GetRequiredService<ISql>();
        var bpModel = await isql.Read(bpId);
        await isql.BlueprintSetMagic(bpId, null, data.magicUse);
        AkimboFileFunctions.LogInfo("bpModel: " + bpModel);
        var pig = orleans.GetInventoryGrain(playerId);
        var itemStorage = isp.GetRequiredService<IItemStorageService>();
        await using var transaction = await itemStorage.MakeTransaction(
            Tag.HttpCall("givebp") // use a dummy tag, those only serves for logging/tracing
            );
        var item = new ItemInfo
        {
            type = isp.GetRequiredService<IGameplayBank>().GetDefinition("Blueprint").Id,
            id = bpId
        };
        item.properties.Add("name", new PropertyValue { stringValue = bpInfo.name });
        item.properties.Add("size", new PropertyValue { intValue = (long)bpInfo.size.x });
        item.properties.Add("static", new PropertyValue { boolValue = bpInfo.kind != ConstructKind.DYNAMIC });
        item.properties.Add("kind", new PropertyValue { intValue = (int)bpInfo.kind });

        await pig.GiveOrTakeItems(transaction,
                    new List<ItemAndQuantity>() {
                        new ItemAndQuantity
                        {
                            item = item,
                            quantity = 1,
                        },
                    },
                    new());
        await transaction.Commit();
        await AkimboNotifications.Notify(orleans, playerId, "Bluepirnt '" + bpInfo.name + "' imported");
        AkimboFileFunctions.LogInfo("Imported blueprint {bpId} from {url} by player {playerId}" + bpId + url + playerId);
        GetBlueprints(isp, playerId, orleans);
    }

    public async static void GetBlueprints(IServiceProvider isp, NQ.PlayerId playerId, IClusterClient orleans)
    {
        try
        {
            var isql = isp.GetRequiredService<ISql>();

            // Execute query and retrieve rows as dynamic
            var result = await isql.Query<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>(
                @"SELECT b.id AS Id, b.name AS Name, b.created_at AS CreatedAt, b.creator_id AS CreatorId, 
              p.display_name AS CreatorName, b.free_deploy AS FreeDeploy, b.max_use AS MaxUse 
              FROM public.blueprint b 
              JOIN public.player p ON b.creator_id = p.id 
              WHERE b.name NOT LIKE '%<SNAPSHOT>%';"
            );

            // Create a list to hold the final mapped blueprint data
            var blueprintList = new List<Dictionary<string, object>>();

            // Loop through each row in the result set
            foreach (var row in result)
            {
                // Create a dictionary to map each field
                var blueprint = new Dictionary<string, object>
            {
                {"Id", row.Item1},
                {"Name", row.Item2},
                {"Created At", row.Item3},
                {"Creator Id", row.Item4},
                {"Creator Name", row.Item5},
                {"free to deploy", row.Item6},
                {"max use", row.Item7}
            };

                // Add the mapped blueprint to the list
                blueprintList.Add(blueprint);
            }

            // Sort the blueprint list by Id
            blueprintList = blueprintList
                .OrderBy(b => (int)b["Id"]) // Ensure Id is treated as an integer
                .ToList();

            // Serialize the result into JSON
            string allBlueprintsJson = JsonConvert.SerializeObject(blueprintList);

            // Log the JSON result
            //AkimboFileFunctions.LogInfo("All blueprints JSON: " + allBlueprintsJson);

            // Notify the player with the blueprint data
            await isp.GetRequiredService<IPub>().NotifyTopic(
                Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(new ModTriggerHudEvent
                {
                    eventName = "AkimboAdminHud.setBlueprints",
                    eventPayload = allBlueprintsJson,
                })
            );
        }
        catch (Exception ex)
        {
            // Log the error details
            AkimboFileFunctions.LogError("Error in GetBlueprints: " + ex.Message);
            AkimboFileFunctions.LogError("Stack Trace: " + ex.StackTrace);
        }
    }

    public static async void ToggleMagicBlueprint(ModAction action, IServiceProvider isp, IClusterClient orleans, HttpClient client, ulong playerId)
    {
        var isql = isp.GetRequiredService<ISql>();
        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<AkimboBlueprint>(js);
        await isql.BlueprintSetMagic(data.id, null, data.free_deploy);
        
        await AkimboNotifications.ErrorNotif(isp, playerId, "Magic blueprint toggled");
        GetBlueprints(isp, playerId, orleans);   
    }


    public static async void AddBpToInventory(ModAction action, IServiceProvider isp, IClusterClient orleans, ulong playerId) 
    {
        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<AkimboBlueprint>(js);
        if (data.selectedPlayer.HasValue)
        {
            playerId = data.selectedPlayer.Value;
        }
        var bpInfo = await orleans.GetBlueprintGrain().GetBlueprintInfo(data.id);
        AkimboFileFunctions.LogInfo("bpInfo: " + bpInfo);
        var pig = orleans.GetInventoryGrain(playerId);
        var itemStorage = isp.GetRequiredService<IItemStorageService>();
        await using var transaction = await itemStorage.MakeTransaction(
            Tag.HttpCall("givebp") // use a dummy tag, those only serves for logging/tracing
            );
        var item = new ItemInfo
        {
            type = isp.GetRequiredService<IGameplayBank>().GetDefinition("Blueprint").Id,
            id = data.id
        };
        item.properties.Add("name", new PropertyValue { stringValue = bpInfo.name });
        item.properties.Add("size", new PropertyValue { intValue = (long)bpInfo.size.x });
        item.properties.Add("static", new PropertyValue { boolValue = bpInfo.kind != ConstructKind.DYNAMIC });
        item.properties.Add("kind", new PropertyValue { intValue = (int)bpInfo.kind });

        await pig.GiveOrTakeItems(transaction,
                    new List<ItemAndQuantity>() {
                        new ItemAndQuantity
                        {
                            item = item,
                            quantity = 1,
                        },
                    },
                    new());
        await transaction.Commit();
        await AkimboNotifications.Notify(orleans, playerId, "Blueprint '" + bpInfo.name + "' added to inventory");
    }


    public static async void DeleteBlueprint(ModAction action, IServiceProvider isp, IClusterClient orleans, HttpClient client, ulong playerId)
    {
        var isql = isp.GetRequiredService<ISql>();
        var js = action.payload;
        AkimboFileFunctions.LogInfo($"data logged: {js}");
        var data = JsonConvert.DeserializeObject<AkimboBlueprint>(js);
        await isql.DeleteBlueprint(data.id);
        await AkimboNotifications.ErrorNotif(isp, playerId, "Blueprint deleted");
        GetBlueprints(isp, playerId, orleans);
    }

}
