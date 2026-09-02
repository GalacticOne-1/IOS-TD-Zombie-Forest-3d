using System;
using Galactic1;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.GameLoop.Tactical;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEngine;


public delegate void DFunc();
public delegate GameObject DFuncObj();
public delegate GameObject[] DFuncObjAr();
public delegate bool DFuncResponse();
public delegate void DFuncBoolOut(out bool y);
public delegate void DFuncBool2(bool y);
public delegate void DFunc1(int value);
public delegate void DFuncId(byte index);
public delegate void DFuncTr(Transform tr);
public delegate void DFuncGo(GameObject g);
public delegate float DFunc4();

public delegate bool DFuncIdResponse(int index);
public delegate bool DFuncOut(out bool y);
public delegate string DFuncString(int value);

public delegate Vector3 DFuncGetCoord(out bool y);
public delegate void DFuncSetCoord(Vector3 coord);



public interface IEvent { }


// *** SOUND
public struct SoundUnitsDisableEvent : IEvent {}
public struct SoundUnitsEnableEvent : IEvent {}


// *** глобально для разных экранов

/// <summary>
/// Отписыватся не нужно!
/// <br/>Единственное событие по готовности кор сцены
/// </summary>
public struct LoadAndStartCoreEvent : IEvent {}

/// <summary>
/// Нужно отписыватся !
/// <br/>После вызова не очищается
/// <br/>Событие когда все в сцене загружено
/// <br/>Вызов в SceneSessionManager
/// </summary>
public struct SceneReadyEvent : IEvent {}

/// <summary>
/// Отписыватся не нужно!
/// <br/>После вызова очищается
/// <br/>Событие когда все в сцене загружено
/// <br/>Вызов в SceneSessionManager
/// </summary>
public struct SceneActivateEvent : IEvent {}
/// <summary>
/// Отписыватся не нужно!
/// <br/>После вызова очищается
/// <br/>UI в сцене инициализированы и готовы принимать события
/// <br/>Вызов в SceneSessionManager
/// </summary>
public struct SceneUIReadyEvent : IEvent {}
/// <summary>
/// Отписыватся не нужно!
/// <br/>После вызова очищается
/// <br/>Вызов перед загрузкой новой сцены
/// </summary>
public struct SceneClearEvent : IEvent {}
/// <summary>
/// Отписыватся не нужно!
/// <br/>После вызова очищается
/// <br/>Вызов перед загрузкой новой сцены
/// </summary>
public struct SceneServicesClearEvent : IEvent {}
/// <summary>
/// Нужно отписыватся !
/// <br/>После вызова не очищается
/// <br/>Вызов перед загрузкой новой сцены
/// </summary>
public struct SceneServicesResetReusableEvent : IEvent {}


// =================================== SCENE LOADER

public struct WorldMapSceneRequestEvent : IEvent {}
public struct HomeSceneRequestEvent : IEvent { public bool ResetRootPlayerScene; }
public struct LocationSceneRequestEvent : IEvent { public int LocationId; }
public struct CampDefenseRequestEvent : IEvent { }

// ===================================


public struct ScreenLoadRegularEvent : IEvent {}
public struct ScreenClearRegularEvent : IEvent {}

public struct ScreenLoadMapEvent : IEvent {}
public struct ScreenClearMapEvent : IEvent {}


// ***      СОСТОЯНИЕ В ЛЮБОМ МЕСТЕ ИГРЫ
public struct StartLevelEvent : IEvent {}               // старт лвл, переход в игру
public struct LoadLevelEvent : IEvent {}
public struct PauseGameEvent : IEvent {}                // пауза в игре
public struct ResumeGameEvent : IEvent {}               // продолжение в игре
public struct FinishLevelEvent : IEvent {}              // окончание лвл
public struct ExitLevelEvent : IEvent {}                // переход в лобби после лвл
public struct ClearLevelEvent : IEvent {}
public struct IsFinishBattleEvent : IEvent {}           // вышли из лвл, экран зугрузки убран
public struct IsFinishRaidEvent : IEvent {}             // после зачистки, находясь в локации


/// <summary>
/// Global visual explosion event.
///
/// USED BY:
/// - camera shake
/// - explosion FX
/// - audio systems
/// - decals
/// </summary>
public struct ExplosionVisualEvent : IEvent
{
    public readonly Vector3 Position;
    public readonly float Radius;
    public readonly float Intensity;

    public ExplosionVisualEvent(Vector3 position, float radius, float intensity)
    {
        Position = position;
        Radius = radius;
        Intensity = intensity;
    }
}

public readonly struct SuppressionVisualEvent : IEvent
{
    public readonly float Intensity;

    public SuppressionVisualEvent(float intensity)
    {
        Intensity = intensity;
    }
}


// ***      БАЗОВЫЕ СОБЫТИЯ НА УРОВНЕ
public struct AnyKeyDownEvent : IEvent {}
public struct ReachNewPlayerLevelEvent : IEvent         // прогресс игрока
{
    public int new_level;
}            
public struct ReachNewStageEvent : IEvent               // прогресс игры
{
    public int new_stage;
}
public struct AdViewEvent: IEvent {}

public struct AdRewardedEvent : IEvent
{
    public EStateAd state;
    public string limit;
}


// **       SOFT/HARD  CURRENCY
public struct StateSoftCurrencyEvent : IEvent           // изменения основной валюты
{
    public long value;
}

public struct StateSoftCurrencyTempEvent : IEvent       // изменения основной валюты (Временная)
{
    public long value;
} 
public struct StateHardCurrencyEvent : IEvent           // изменения платной валюты
{
    public long value;
}
public struct StateHardCurrencyTempEvent : IEvent       // изменения платной валюты (Временная)
{
    public long value;
}
public struct EarnSoftCurrencyEvent : IEvent            // добыть основную валюту
{
    public long add_value;
}
public struct EarnHardCurrencyEvent : IEvent            // добыть платную валюту
{
    public int add_value;
}



// *** GLOBAL MAP
public readonly struct HordeAttackMissedEvent : IEvent
{
}



// ***      СМЕННЫЕ СОБЫТИЯ НА УРОВНЕ


public struct StateSilverCurrencyTempEvent : IEvent     // изменения боевой валюты (Временная)
{
    public long value;
} 


public struct StateStatSoulEvent : IEvent               // изменения валюты SOUL
{
    public long value;
}





// === события для статуса миссии
public struct BuildingDestroyedEvent : IEvent
{
    public bool IsHeadquarters;
}

public struct WaveCompletedEvent : IEvent
{
    public bool AllWavesCompleted;
}
public struct ExitReachedEvent : IEvent{}

public struct MissionCompletedEvent : IEvent
{
    public Type NextState;
    public MissionResult Result;
}

// === События смерти юнитов игрока
public struct UnitKilledEvent : IEvent
{
    public readonly SurvivorInstance Unit;
    public UnitKilledEvent(SurvivorInstance unit) => Unit = unit;
}

public struct UnitReadyForDespawnEvent : IEvent
{
    public readonly SurvivorInstance Unit;
    public UnitReadyForDespawnEvent(SurvivorInstance unit) => Unit = unit;
}
//


/// <summary>
/// Поднимается при смерти ЛЮБОГО врага (ambient/wave/director —
/// источник различается через Runtime.SpawnSource). Единая точка входа
/// для всех подписчиков (WaveSystem, будущий killed-counter для
/// RaidResultProxy, аналитика), вместо приватного EnemyRuntime.OnDeath
/// на каждого подписчика по отдельности.
/// </summary>
public sealed class EnemyKilledEvent : IEvent
{
    public readonly EnemyRuntime Runtime;

    public EnemyKilledEvent(EnemyRuntime runtime)
    {
        Runtime = runtime;
    }
}


public struct SurvivorStatusChangedEvent : IEvent
{
    public readonly string UnitId;
    public readonly bool IsHungry;
    public readonly bool IsThirsty;

    public SurvivorStatusChangedEvent(string unitId, bool isHungry, bool isThirsty)
    {
        UnitId = unitId;
        IsHungry = isHungry;
        IsThirsty = isThirsty;
    }
}

/// <summary>
/// Поднимается WaveSystem один раз, когда завершена последняя волна.
/// Отдельно от WaveCompletedEvent (который поднимается после каждой волны) —
/// чтобы MissionObjectiveService мог однозначно отличить "волна закончилась"
/// от "все волны закончились", не пересчитывая это косвенно.
/// </summary>
public sealed class AllWavesCompletedEvent : IEvent {}




// -- события с объектами взаимодействия


public struct ItemPickedEvent : IEvent                 
{
    public ItemConfig Item;
    public int Amount;
    public Vector3 WorldPos;
    
    public ItemPickedEvent(ItemConfig item, int amount, Vector3 pos)
    {
        Item = item;
        Amount = amount;
        WorldPos = pos;
    }
}              

public struct ToolBrokenEvent : IEvent 
{
    public ItemConfig Tool;
    public Vector3 WorldPos;
    
    public ToolBrokenEvent(ItemConfig tool, Vector3 pos)
    {
        Tool = tool;
        WorldPos = pos;
    }
}

public struct RequirementFailedEvent : IEvent 
{
    public string Message;
    public Vector3 WorldPos;
    
    public RequirementFailedEvent(string message, Vector3 pos)
    {
        Message = message;
        WorldPos = pos;
    }
}



public struct ProductionOrderCompletedEvent : IEvent
{
    public string JobId;
    public string StationId;
    public RuntimeId RecipeId;
    
    public int Orders;
    public int Amount;
}

public struct ProductionOrderAutoCollectedEvent : IEvent
{
    public string JobId;
    public string StationId;
    public RuntimeId RecipeId;
    
    public int Orders;
    public int Amount;
}