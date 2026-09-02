using System.Collections.Generic;
using Gameplay;
using Galactic1.AbstractFactory;
using UnityEngine;


namespace Galactic1
{
    public static class EntityRepository
    {

        // private static Dictionary<string, _Object_> entities = new();
        //
        //
        // public static void AddEntity(_Object_ entity)
        // {
        //     if (!entities.ContainsKey(entity.UniqueId))
        //     {
        //         entities.Add(entity.UniqueId, entity);
        //         DLog.Alert($"[Repository] Added {entity.GetType().Name} with ID: {entity.UniqueId}");
        //     }
        //     else
        //     {
        //         Debug.LogWarning($"[Repository] Entity with ID {entity.UniqueId} already exists!");
        //     }
        // }
        //
        // public static void RemoveEntity(string uniqueId)
        // {
        //     if (entities.ContainsKey(uniqueId))
        //     {
        //         entities.Remove(uniqueId);
        //         DLog.Alert($"[Repository] Removed entity with ID: {uniqueId}", EDlogColor.YELLOW);
        //     }
        // }
        //
        // public static _Object_ GetEntityById(string uniqueId)
        // {
        //     entities.TryGetValue(uniqueId, out _Object_ entity);
        //     return entity;
        // }
        //
        // public static List<T> GetAllOfType<T>() where T : _Object_
        // {
        //     List<T> result = new List<T>();
        //     foreach (var kvp in entities)
        //     {
        //         if (kvp.Value is T typedEntity)
        //             result.Add(typedEntity);
        //     }
        //     return result;
        // }
        //
        // public static void ClearAll()
        // {
        //     entities.Clear();
        // }
        //
        //
        //
        //
        //
        //
        // // ***          SAVING          ***
        //
        // public static void GetSaveData(string entityId, out CGridEntity sd)
        //     => sd = new SAVE().DataGameplay().MapData[GAME.DataGameplay().CurrentMapData].GridStructures
        //         .Find(s => s.Key == entityId).Value;
        //
        //
        // public static void Save(CGridEntity savingData)
        // {
        //     var item = ListSaver.Get<ObjectEntry<CGridEntity>, CGridEntity>(savingData.Id,
        //         ref new SAVE().DataGameplay().MapData[GAME.DataGameplay().CurrentMapData].GridStructures);
        //
        //     item = savingData;
        //     
        //     ListSaver.Set(savingData.Id, item, ref new SAVE().DataGameplay().MapData[GAME.DataGameplay().CurrentMapData].GridStructures);
        // }
        //
        // public static bool RemoveSaving(string entityId, string logMessage)
        // {
        //     if (ListSaver.Remove<ObjectEntry<CGridEntity>, CGridEntity>(entityId,
        //             new SAVE().DataGameplay().MapData[GAME.DataGameplay().CurrentMapData].GridStructures))
        //     {
        //         RepositoryUtility.I.AddLog(entityId, logMessage);
        //         DLog.Alert($"Saving clear for {entityId} entity", EDlogColor.YELLOW);
        //         return true;
        //     }
        //
        //     return false;
        // }
    }
}