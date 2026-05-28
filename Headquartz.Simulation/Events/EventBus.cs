using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Events;

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>>
        _handlers = [];

    public void Subscribe<T>(
        Action<T> handler)
        where T : IGameEvent
    {
        var type = typeof(T);

        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = [];
        }

        _handlers[type].Add(handler);
    }

    public void Publish<T>(
        T gameEvent)
        where T : IGameEvent
    {
        var type = typeof(T);

        if (!_handlers.TryGetValue(
                type,
                out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            ((Action<T>)handler)
                .Invoke(gameEvent);
        }
    }
}