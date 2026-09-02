using System;
using System.Collections.Generic;
using Galactic1.Core;

namespace Galactic1
{
    public class CommandProcessor : ICommandProcessor
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly Dictionary<Type, object> _handlesMap = new();

        public CommandProcessor(IGameStateProvider gameStateProvider)
        {
            _gameStateProvider = gameStateProvider;
        }


        public void RegisterHandle<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand
        {
            _handlesMap[typeof(TCommand)] = handler;
        }

        public bool Process<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (_handlesMap.TryGetValue(typeof(TCommand), out var handler))
            {
                var typedHandler = (ICommandHandler<TCommand>)handler;
                var result = typedHandler.Handle(command);

                // при каждом действии будет сохранение игры
                if (result)
                {
                    _GameState.Save();
                }

                return result;
            }

            return false;
        }
    }
}