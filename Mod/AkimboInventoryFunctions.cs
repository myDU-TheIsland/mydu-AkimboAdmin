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
using Backend.Storage;
using NQ;
using NQ.Interfaces;
using Microsoft.Extensions.DependencyInjection;


public class AkimboInventoryFunctions
{
    public static Dictionary<ulong, double> spawnItems = new();
	public AkimboInventoryFunctions()
	{
	}
	public static async void CustomFillInvetory(IClusterClient orleans, IServiceProvider isp,NQ.PlayerId playerId , CMC config)
	{
        await orleans.GetTalentGrain(playerId).Give(config.talentId, 1);
        AkimboFileFunctions.LogInfo("talentgiven");
        var bank = isp.GetRequiredService<IGameplayBank>();
        var entity = new NQ.EntityId { playerId = playerId };
        var itemsToGive = new List<ItemAndQuantity>();

        // First, populate the spawnItems dictionary
        foreach (var (k, v) in config.singleItems)
        {
            if (bank.GetDefinition(k).BaseObject.hidden)
                continue;
            spawnItems[bank.GetDefinition(k).Id] = v;
        }
        foreach (var (k, v) in config.recursiveSpawnGroups)
        {
            var baseEntry = bank.GetDefinition(k);
            if (baseEntry.BaseObject.hidden)
                continue;
            var childrenIds = baseEntry.GetChildrenIdsRecursive();

            foreach (var childId in childrenIds)
            {
                var entry = bank.GetDefinition(childId);
                if (entry.BaseObject.hidden)
                    continue;
                if (entry.GetChildren().Count() != 0)
                    continue;
                spawnItems[childId] = v;
            }
        }

        // Now, create a list of ItemAndQuantity objects with debug logging
        foreach (var (k, v) in spawnItems)
        {
            ItemInfo item = new ItemInfo { type = k };
            DeltaQuantity quantity = new DeltaQuantity { value = (long)v };
            ItemAndQuantity i = new ItemAndQuantity { item = item, quantity = quantity };
            itemsToGive.Add(i);

            // Debug log for each item being added
            AkimboFileFunctions.LogInfo($"Preparing to give item with ID: {k}, Quantity: {v}");
        }

        // Debug log before the operation
        var spawnItemsString = string.Join(", ", itemsToGive.Select(iq => $"ItemID: {iq.item.id}, Quantity: {iq.quantity.value}"));
        // AkimboFileFunctions.LogInfo($"SpawnItems prepared: {spawnItemsString}");

        await using var tr = await isp.GetRequiredService<IItemStorageService>().MakeTransaction(
            Tag.HttpCall("inserting items") // use a dummy tag, those only serve for logging/tracing
        );

        var container = await orleans.GetPlayerInventoryGrain(playerId).GetActivePrimaryContainer();

        // Pass the collection to the GiveOrTakeItems method
        await orleans.GetAbstractStorage(playerId, container).GiveOrTakeItems(tr, itemsToGive, new NQutils.Storage.OperationOptions
        {
            BypassLock = true,
            AllowOverload = true,
            AllowPartial = true,
            MakeManifest = false,
        });

        await tr.Commit();

        // Final debug log after commit
        AkimboFileFunctions.LogInfo("Items successfully given to player.");
    }

    public static async void ClearInventory(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId, CMC config)
    {
        // deactive the talent when a player deactives creative mode
        await orleans.GetTalentGrain(playerId).RemoveTalent(config.talentId);

        await using var tr = await isp.GetRequiredService<IItemStorageService>().MakeTransaction(
                  Tag.HttpCall("all items reset") // use a dummy tag, those only serves for logging/tracing
                  );
        await orleans.GetInventoryGrain(playerId).ResetToDefault(tr, true);
        var container = await orleans.GetPlayerInventoryGrain(playerId).GetActivePrimaryContainer();
        await orleans.GetAbstractStorage(playerId, container).Stack(tr);
        await tr.Commit();
        AkimboFileFunctions.LogInfo("items deleted");
    }

    public static async void AddItemToInventory(ModAction action, IClusterClient orleans, IServiceProvider isp, NQ.PlayerId playerId, ulong itemId , long quantityToAdd)
    {
        var bank = isp.GetRequiredService<IGameplayBank>();
        var entity = new NQ.EntityId { playerId = playerId };
        var itemsToGive = new List<ItemAndQuantity>();
        // Now, create a list of ItemAndQuantity objects with debug logging

            ItemInfo item = new ItemInfo { type = itemId };
            DeltaQuantity quantity = new DeltaQuantity { value = (long)quantityToAdd };
            ItemAndQuantity i = new ItemAndQuantity { item = item, quantity = quantity };
            itemsToGive.Add(i);

        // Debug log for each item being added
        AkimboFileFunctions.LogInfo($"Preparing to give item with ID: {itemId}, Quantity: {quantityToAdd}");

        // Debug log before the operation
        var spawnItemsString = string.Join(", ", itemsToGive.Select(iq => $"ItemID: {iq.item.id}, Quantity: {iq.quantity.value}"));
        // AkimboFileFunctions.LogInfo($"SpawnItems prepared: {spawnItemsString}");

        await using var tr = await isp.GetRequiredService<IItemStorageService>().MakeTransaction(
            Tag.HttpCall("inserting items") // use a dummy tag, those only serve for logging/tracing
        );

       // var container = await orleans.GetPlayerInventoryGrain(playerId).GetActivePrimaryContainer();
        var itemStorage = isp.GetRequiredService<IItemStorageService>();
        var inventory = StorageRef.PlayerInventoryWithoutPrimary(playerId);
        await itemStorage.GiveOrTakeItems(tr, inventory, itemsToGive, new NQutils.Storage.OperationOptions
        {
            BypassLock = true,
            AllowOverload = true,
            AllowPartial = true,
            MakeManifest = false,
        });
        // Pass the collection to the GiveOrTakeItems method -- OLD CODE maybe reuse it to add it to linked container instead of nanopack

        /*await orleans.GetAbstractStorage(playerId, container).GiveOrTakeItems(tr, itemsToGive, new NQutils.Storage.OperationOptions
        {
            BypassLock = true,
            AllowOverload = true,
            AllowPartial = true,
            MakeManifest = false,
        });*/

        await tr.Commit();

        // Final debug log after commit
        AkimboFileFunctions.LogInfo("Items successfully given to player.");
    }
}
