using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace Galactic1.Code.Systems.Economy
{
    /// <summary>
    /// Runtime-логика валют.
    /// Не знает о UI.
    /// Не знает о Production.
    /// Работает только с Proxy.
    /// </summary>
    public sealed class CurrencyRuntime
    {
        public readonly ObservableList<BankResourceViewModel> Resources = new();
        private readonly Dictionary<EBankResourceType, BankResourceViewModel> _resources = new();

        public event Action OnChanged;

        public CurrencyRuntime(ObservableList<BankResourceProxy> proxies)
        {
            proxies.ForEach(CreateResourceViewModel);
            proxies.ObserveAdd().Subscribe(e => CreateResourceViewModel(e.Value));
            proxies.ObserveRemove().Subscribe(e => RemoveResourceViewModel(e.Value));
            
            // ! тестовая подписка !
            var l = Enum.GetNames(typeof(EBankResourceType)).Length;
            for (int i = 0; i < l; i++)
            {
                var _i = i;
                ObservResource((EBankResourceType)i)
                    .Subscribe(_ => DLog.Alert($"{(EBankResourceType)_i} : {_}", AppConstants.show_log_economics));
            }
            
        }
        
        
        public Observable<int> ObservResource(EBankResourceType resourceType)
        {
            if (_resources.TryGetValue(resourceType, out var resourceViewModel))
            {
                return resourceViewModel.Amount;
            }

            throw new Exception($"Bank resource type of [{resourceType}] not exist");
        }
        
        void CreateResourceViewModel(BankResourceProxy resource)
        {
            var resourceViewModel = new BankResourceViewModel(resource);
            _resources[resource.BankResourceType] = resourceViewModel;
            
            Resources.Add(resourceViewModel);
        }

        void RemoveResourceViewModel(BankResourceProxy resource)
        {
            if (_resources.TryGetValue(resource.BankResourceType, out var resourceViewModel))
            {
                Resources.Remove(resourceViewModel);
                _resources.Remove(resource.BankResourceType);
            }
        }
        
        
        

        public int GetBalance(EBankResourceType type)
        {
            return _resources.TryGetValue(type, out var proxy)
                ? proxy.Amount.Value
                : 0;
        }

        public void Add(EBankResourceType type, int amount)
        {
            if (amount <= 0)
                return;

            if (!_resources.TryGetValue(type, out var proxy))
                return;

            proxy.Amount.Value += amount;
            OnChanged?.Invoke();
        }

        public bool TrySpend(EBankResourceType type, int amount)
        {
            if (amount <= 0)
                return false;

            if (!_resources.TryGetValue(type, out var proxy))
                return false;

            if (proxy.Amount.Value < amount)
                return false;

            proxy.Amount.Value -= amount;
            OnChanged?.Invoke();
            return true;
        }
    }
}