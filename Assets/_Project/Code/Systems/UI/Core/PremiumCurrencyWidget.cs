using Galactic1.Code.Systems.Economy;
using R3;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.Core
{
    public class PremiumCurrencyWidget : ReactiveWidget<int>
    {
        [SerializeField] private TMP_Text label;

        protected override Observable<int> GetObservable()
            => ServiceLocator.Current.Get<IEconomyService>().ObservResource(EBankResourceType.CurrencyPremium);

        protected override void Refresh(int value) => label.text = $"{value}";
    }
}