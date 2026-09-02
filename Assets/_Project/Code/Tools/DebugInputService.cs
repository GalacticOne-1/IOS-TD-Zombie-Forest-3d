using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code
{
    /// <summary>
    /// Сервис быстрых тестов через клавиатуру.
    /// Регистрируется в ServiceLocator.
    /// Подписка из любого места — без MonoBehaviour.
    /// 
    /// Использование:
    ///   DebugInputService.On(KeyCode.F1, () => SpawnEnemy());
    ///   DebugInputService.On(KeyCode.F2, "Spawn loot", () => SpawnLoot());
    /// </summary>
    public sealed class DebugInputService : Singleton<DebugInputService>
    {
        private readonly Dictionary<KeyCode, List<DebugAction>> _bindings = new();


        // =========================
        // API
        // =========================
        public void On(KeyCode key, Action callback, string label = null)
        {
            if (!_bindings.TryGetValue(key, out var list))
            {
                list = new List<DebugAction>();
                _bindings[key] = list;
            }

            list.Add(new DebugAction(label ?? key.ToString(), callback));
        }

        public void Off(KeyCode key, Action callback)
        {
            if (!_bindings.TryGetValue(key, out var list)) return;
            list.RemoveAll(a => a.Callback == callback);
        }

        public void Clear() => _bindings.Clear();

        // =========================
        // Tick
        // =========================
        private void Update()
        {
            foreach (var kvp in _bindings)
            {
                if (!Input.GetKeyDown(kvp.Key)) continue;

                foreach (var action in kvp.Value)
                {
                    try
                    {
#if UNITY_EDITOR
                        DLog.Alert($"[DebugInput] {kvp.Key} → {action.Label}", EDlogColor.YELLOW);
#endif
                        
                        action.Callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DebugInput] {kvp.Key} error: {e.Message}");
                    }
                }
            }
        }

        // =========================
        // GUI — список активных биндингов
        // =========================
#if UNITY_EDITOR
        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                normal    = { textColor = new Color(0.6f, 1f, 0.6f) }
            };

            float y = 10f;
            GUI.Label(new Rect(10, y, 300, 20),
                "[DEBUG INPUT]", style);
            y += 18f;

            foreach (var kvp in _bindings)
            {
                foreach (var action in kvp.Value)
                {
                    GUI.Label(
                        new Rect(10, y, 300, 18),
                        $"  [{kvp.Key}]  {action.Label}",
                        style);
                    y += 16f;
                }
            }
        }
#endif

        // =========================
        // Inner
        // =========================
        private readonly struct DebugAction
        {
            public readonly string Label;
            public readonly Action Callback;

            public DebugAction(string label, Action callback)
            {
                Label    = label;
                Callback = callback;
            }
        }
    }
}