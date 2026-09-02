using System;
using System.Collections.Generic;
using Galactic1;
using ObservableCollections;
using R3;

namespace Galactic1
{
    public class BankResourceService
    {
        public readonly ObservableList<BankResourceViewModel> Resources = new();

        private readonly Dictionary<EBankResourceType, BankResourceViewModel> _resourcesMap = new();
        private readonly ICommandProcessor _cmd;
        

        public BankResourceService(ObservableList<BankResourceProxy> resources, ICommandProcessor cmd)
        {
            _cmd = cmd;
            resources.ForEach(CreateResourceViewModel);
            resources.ObserveAdd().Subscribe(e => CreateResourceViewModel(e.Value));
            resources.ObserveRemove().Subscribe(e => RemoveResourceViewModel(e.Value));
        }

        public bool AddResource(EBankResourceType resourceType, int amount)
        {
            var command = new CmdBankResourcesAdd(resourceType, amount);

            return _cmd.Process(command);
        }
        
        public bool TrySpendResource(EBankResourceType resourceType, int amount)
        {
            var command = new CmdBankResourcesSpend(resourceType, amount);

            return _cmd.Process(command);
        }
        
        public bool IsEnoughtResource(EBankResourceType resourceType, int amount)
        {
            if (_resourcesMap.TryGetValue(resourceType, out var resourceViewModel))
            {
                return resourceViewModel.Amount.CurrentValue >= amount;
            }

            return false;
        }

        public Observable<int> ObservResource(EBankResourceType resourceType)
        {
            if (_resourcesMap.TryGetValue(resourceType, out var resourceViewModel))
            {
                return resourceViewModel.Amount;
            }

            throw new Exception($"Bank resource type of [{resourceType}] not exist");
        }

        void CreateResourceViewModel(BankResourceProxy resource)
        {
            var resourceViewModel = new BankResourceViewModel(resource);
            _resourcesMap[resource.BankResourceType] = resourceViewModel;
            
            Resources.Add(resourceViewModel);
        }

        void RemoveResourceViewModel(BankResourceProxy resource)
        {
            if (_resourcesMap.TryGetValue(resource.BankResourceType, out var resourceViewModel))
            {
                Resources.Remove(resourceViewModel);
                _resourcesMap.Remove(resource.BankResourceType);
            }
        }
    }
}