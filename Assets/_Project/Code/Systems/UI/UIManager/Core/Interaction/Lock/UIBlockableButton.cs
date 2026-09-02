
using UnityEngine;
using Galactic1.Code.UI.Interaction;
using Galactic1.UI.Core;


/// <summary>
/// Вешать на кнопку для зависимости от глобального состояния
/// </summary>
public sealed class UIBlockableButton : MonoBehaviour, IUIBlockable
{
    [SerializeField] private UIBlockGroup group = UIBlockGroup.Global;

    private BaseUIButton _button;
    private UIBlockRegistry _registry;

    public UIBlockGroup Group => group;

    
    
    public void Register(UIBlockRegistry registry)
    {
        _button = GetComponent<BaseUIButton>();
        _registry = registry;
        _registry?.Register(this);
    }

    private void OnEnable()
    {
        _registry?.Register(this);
    }

    private void OnDisable()
    {
        _registry?.Unregister(this);
    }

    public void SetBlocked(bool blocked)
    {
        _button.SetInteractable(!blocked);
    }
}