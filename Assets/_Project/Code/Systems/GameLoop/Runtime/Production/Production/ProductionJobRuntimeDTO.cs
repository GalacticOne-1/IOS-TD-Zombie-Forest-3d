using System;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Slot-based production job.
    /// Хранит состояние (Queued / InProgress / Completed).
    /// </summary>
    public sealed class ProductionJobRuntimeDTO
    {
        public string JobId { get; }
        public RuntimeId RecipeId { get; }
        
        public int TotalDurationHours { get; }
        public int StartWorldHour { get; private set; }
        public ProductionJobState State { get; private set; }
        
        public int Amount { get; set; }
        public int CurrentStack { get; set; }
        public int CompletedStack { get; set; }
        
        
        
        public int FinishWorldHour => StartWorldHour + TotalDurationHours;

        public ProductionJobRuntimeDTO(
            string id,
            RuntimeId recipeId,
            int duration,
            int amount, 
            int currentStack,
            int completedStack)
        {
            JobId = id;
            RecipeId = recipeId;
            TotalDurationHours = duration;
            Amount = amount;
            CurrentStack = currentStack;
            CompletedStack = completedStack;
            State = ProductionJobState.Queued;
        }

        public void Start(int worldHour)
        {
            if (State != ProductionJobState.Queued)
                return;

            StartWorldHour = worldHour;
            State = ProductionJobState.InProgress;
        }

        public void MarkCompleted()
        {
            State = ProductionJobState.Completed;
        }

        public void ForceCompleteImmediate()
        {
            // Skip без искажения времени
            State = ProductionJobState.Completed;
        }

        public bool IsCompleted(int currentHour)
        {
            if (State != ProductionJobState.InProgress)
                return false;

            return currentHour >= FinishWorldHour;
        }

        public void RestoreState(
            ProductionJobState state,
            int startHour)
        {
            State = state;
            StartWorldHour = startHour;
        }
    }
}