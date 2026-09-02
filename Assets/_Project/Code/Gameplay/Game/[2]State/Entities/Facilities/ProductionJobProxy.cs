using Galactic1.Game.Runtime.Production;
using R3;

namespace Galactic1.Game.Buildings.Proxy
{
    public class ProductionJobProxy
    {
        public readonly ProductionJobData Origin;

        public readonly ReactiveProperty<string> JobId;
        public readonly ReactiveProperty<string> RecipeId;
        public readonly ReactiveProperty<ProcessingMode> Mode;
        public readonly ReactiveProperty<int> TotalHours;
        public readonly ReactiveProperty<int> StartWorldHour;
        public readonly ReactiveProperty<int> MaxStack;
        public readonly ReactiveProperty<int> CurrentStack;
        public readonly ReactiveProperty<int> CompletedStack;
        public readonly ReactiveProperty<int> Amount;
        public readonly ReactiveProperty<ProductionJobState> State;

        public ProductionJobProxy(ProductionJobData data)
        {
            Origin = data;

            JobId = new(data.JobId);
            RecipeId = new(data.RecipeId);
            Mode = new((ProcessingMode)data.Mode);
            TotalHours = new(data.TotalHours);
            StartWorldHour = new(data.StartWorldHour);
            MaxStack = new(data.MaxStack);
            CurrentStack = new(data.CurrentStack);
            CompletedStack = new(data.CompletedStack);
            Amount = new(data.Amount);
            State = new((ProductionJobState)data.State);

            // подписка на Origin
            JobId.Skip(1).Subscribe(v => Origin.JobId = v);
            RecipeId.Skip(1).Subscribe(v => Origin.RecipeId = v);
            Mode.Skip(1).Subscribe(v => Origin.Mode = (byte)v);
            TotalHours.Skip(1).Subscribe(v => Origin.TotalHours = v);
            StartWorldHour.Skip(1).Subscribe(v => Origin.StartWorldHour = v);
            MaxStack.Skip(1).Subscribe(v => Origin.MaxStack = v);
            CurrentStack.Skip(1).Subscribe(v => Origin.CurrentStack = v);
            CompletedStack.Skip(1).Subscribe(v => Origin.CompletedStack = v);
            Amount.Skip(1).Subscribe(v => Origin.Amount = v);
            State.Skip(1).Subscribe(v => Origin.State = (byte)v);
        }


    }
}