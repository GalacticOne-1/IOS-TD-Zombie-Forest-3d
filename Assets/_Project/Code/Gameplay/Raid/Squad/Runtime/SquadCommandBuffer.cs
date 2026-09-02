using System;
using System.Collections.Generic;

public sealed class SquadCommandBuffer
{
    private readonly Queue<Action> _commands = new();

    public void Enqueue(Action command)
    {
        _commands.Enqueue(command);
    }

    public void Execute()
    {
        while (_commands.Count > 0)
            _commands.Dequeue().Invoke();
    }
}