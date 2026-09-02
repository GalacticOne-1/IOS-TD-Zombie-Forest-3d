
using System.Collections.Generic;
using System.Linq;
using Galactic1;

public static class EventBus<T> where T : IEvent
{
    static readonly HashSet<IEventBinding<T>> bindings = new HashSet<IEventBinding<T>>();

    public static void Register(EventBinding<T> binding, bool clear_after_rise = false)
    {
        bindings.Add(binding);
        
        if (clear_after_rise) 
            binding.ClearAfterUsing = true;
    }

    public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

    public static void ClearAfterUsing(EventBinding<T> binding) => binding.ClearAfterUsing = true;


    public static void Raise(T @event, bool instantClear = false)
    {
        List<IEventBinding<T>> _clear = new List<IEventBinding<T>>();

        var snapshot = bindings.ToArray();

        foreach (var binding in snapshot)
        {
            binding.OnEvent.Invoke(@event);
            binding.OnEventNoArgs.Invoke();

            // собираем то что должно быть удалено
            if (instantClear || binding.ClearAfterUsing) 
                _clear.Add(binding);
        }

        // удаляем что для одного использования
        var l = _clear.Count;
        for (int i = l - 1; i >= 0; i--)
        {
            bindings.Remove(_clear[i]);
        }
    }
    
    

    public static void Clear()
    {
        DLog.Alert($">>>>>>     Clearing {typeof(T).Name} bindings", EDlogColor.ORANGE);
        bindings.Clear();
    }
}