using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace Galactic1.Game.Buildings.Proxy
{
    public class FacilityProxy : EntityProxy
    {
        
        public readonly ReactiveProperty<int> Level;
        
        public ReactiveProperty<bool> IsDead { get; }
        public Dictionary<StatId, ReactiveProperty<float>> Stats { get;}
        
        // --- Позиция ---
        public ReactiveProperty<float> PosX { get; }
        public ReactiveProperty<float> PosY { get; }
        public ReactiveProperty<float> PosZ { get; }
        public ReactiveProperty<int> Rotation { get; }
        
        
        // --- Tavern ---
        private readonly List<RecruitOfferProxy> _tavernOffers = new();
        public IReadOnlyList<RecruitOfferProxy> TavernOffers => _tavernOffers;
        public ReactiveProperty<int> NextRefreshDay { get; }
        
        // --- Garage ---
        public readonly ObservableList<string> UnlockedModules = new();
        
        // --- Производство ---
        public ReactiveProperty<bool> IsWorking;
        public ReactiveProperty<int> ActiveIndex;
        //public ObservableList<ProductionJobProxy> ProductionQueue;
        
        
        private readonly List<ProductionJobProxy> _productionQueue = new();
        public IReadOnlyList<ProductionJobProxy> ProductionQueue => _productionQueue;
        
        
        public FacilityProxy(FacilityData origin) : base(origin)
        {
            Level = new(origin.Level);
            Level.Skip(1).Subscribe(_ => origin.Level = _);
            
            IsDead = new(origin.IsDead);
            Stats = new();
            
            // --- Позиция ---
            PosX = new(origin.PosX);
            PosY = new(origin.PosY);
            PosZ = new(origin.PosZ);
            Rotation = new(origin.Rotation);
            
            // --- Tavern ---
            InitializeTavern();
            NextRefreshDay = new(origin.NextRefreshDay);
            NextRefreshDay.Skip(1).Subscribe(_ => origin.NextRefreshDay = _);
            
            // --- Производство ---
            IsWorking = new(origin.IsWorking);
            //ProductionQueue = new();
            ActiveIndex = new(origin.ActiveIndex);
            
            // subscription
            Initialize();
            InitializeGarage();
            InitializeProduction();
        }
        
        
        
        void Initialize()
        {
            var origin = Origin as FacilityData;
            origin.Stats.ForEach(s => Stats[s.Key] = new(s.Value));
            
            IsDead.Skip(1).Subscribe(_ => origin.IsDead = _);
            foreach (var stat in Stats)
            {
                // Находим индекс элемента с нужным ключом
                int index = origin.Stats.FindIndex(s => s.Key == stat.Key);

                if (index >= 0)
                {
                    stat.Value.Skip(1).Subscribe(_ =>
                    {
                        var kv = origin.Stats[index];
                        kv.Value = _;
                        origin.Stats[index] = kv; // Обновляем элемент в списке
                    });
                }
            }
            
            Level.Skip(1).Subscribe(_ => origin.Level = _);
            
            // --- Позиция ---
            PosX.Skip(1).Subscribe(_ => origin.PosX = _);
            PosY.Skip(1).Subscribe(_ => origin.PosY = _);
            PosZ.Skip(1).Subscribe(_ => origin.PosZ = _);
            Rotation.Skip(1).Subscribe(_ => origin.Rotation = _);
            
        }
        
        
        private void InitializeTavern()
        {
            var origin = (FacilityData)Origin;

            _tavernOffers.Clear();
            foreach (var data in origin.TavernOffers)
                _tavernOffers.Add(new RecruitOfferProxy(data));
        }
        
        
        public void InitializeGarage()
        {
            var origin = (FacilityData)Origin;

            foreach (var id in origin.UnlockedModules)
                UnlockedModules.Add(id);

            UnlockedModules.ObserveAdd()
                .Subscribe(e => origin.UnlockedModules.Add(e.Value));

            UnlockedModules.ObserveRemove()
                .Subscribe(e => origin.UnlockedModules.Remove(e.Value));
        }
        
        private void InitializeProduction()
        {
            var origin = (FacilityData)Origin;
            
            // --- Производство ---
            IsWorking.Skip(1).Subscribe(_ => origin.IsWorking = _);
            ActiveIndex.Skip(1) .Subscribe(v => origin.ActiveIndex = v);

            _productionQueue.Clear();
            foreach (var data in origin.ProductionQueue)
                _productionQueue.Add(new ProductionJobProxy(data));

            // foreach (var jobData in origin.ProductionQueue)
            //     ProductionQueue.Add(new ProductionJobProxy(jobData));
            //
            // Debug.LogError($"origin.ProductionQueue : {origin.ProductionQueue.Count}/ {ProductionQueue.Count}");
            //
            // // синхронизация со списком Origin.Inventory
            // ProductionQueue.ObserveAdd().Subscribe(e =>
            // {
            //     origin.ProductionQueue.Add(e.Value.Origin);
            // });
            //
            // ProductionQueue.ObserveRemove().Subscribe(e =>
            // {
            //     origin.ProductionQueue.Remove(e.Value.Origin);
            // });
            // ProductionQueue.ObserveReplace().Subscribe(e =>
            // {
            //     origin.ProductionQueue[e.Index] = e.NewValue.Origin;
            // });
        }
        
        
        
        
        public void SetTavernOffers(List<RecruitOfferData> offers)
        {
            var origin = (FacilityData)Origin;

            origin.TavernOffers = offers;

            _tavernOffers.Clear();
            foreach (var data in offers)
                _tavernOffers.Add(new RecruitOfferProxy(data));
        }

        public void RemoveOffer(RecruitOfferProxy proxy)
        {
            var origin = (FacilityData)Origin;

            origin.TavernOffers.Remove(proxy.Origin);
            _tavernOffers.Remove(proxy);
        }
        
        
        
        public void AddJob(ProductionJobData data)
        {
            var origin = (FacilityData)Origin;

            origin.ProductionQueue.Add(data);
            _productionQueue.Add(new ProductionJobProxy(data));
        }

        public void RemoveJob(ProductionJobProxy proxy)
        {
            var origin = (FacilityData)Origin;

            origin.ProductionQueue.Remove(proxy.Origin);
            _productionQueue.Remove(proxy);
        }

        public void Reorder(List<ProductionJobProxy> ordered)
        {
            var origin = (FacilityData)Origin;

            _productionQueue.Clear();
            _productionQueue.AddRange(ordered);

            origin.ProductionQueue = ordered
                .Select(p => p.Origin)
                .ToList();
        }
        
    }
}