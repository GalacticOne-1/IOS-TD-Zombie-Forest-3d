using System;
using System.Collections.Generic;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Runtime registry for quick module access.
    /// Avoids repeated GetModule calls.
    /// </summary>
    public class ModuleRegistry
    {
        private readonly Dictionary<Type, ItemModule> _modules = new();

        /// <summary>
        /// Registers module instance.
        /// Throws if module already registered.
        /// </summary>
        public void Register(ItemModule module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            var type = module.GetType();

            if (_modules.ContainsKey(type))
                throw new InvalidOperationException(
                    $"Module of type {type.Name} already registered.");

            _modules.Add(type, module);
        }

        /// <summary>
        /// Returns module of given type or throws if missing.
        /// </summary>
        public T Get<T>() where T : ItemModule
        {
            var type = typeof(T);

            if (_modules.TryGetValue(type, out var module))
                return (T)module;

            throw new KeyNotFoundException($"Module {type.Name} is not registered.");
        }

        /// <summary>
        /// Attempts to get module safely.
        /// </summary>
        public bool TryGet<T>(out T module) where T : ItemModule
        {
            if (_modules.TryGetValue(typeof(T), out var value))
            {
                module = (T)value;
                return true;
            }

            module = null;
            return false;
        }

        /// <summary>
        /// Checks if module exists.
        /// </summary>
        public bool Has<T>() where T : ItemModule
        {
            return _modules.ContainsKey(typeof(T));
        }
    }
}