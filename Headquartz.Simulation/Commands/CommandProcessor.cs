using Headquartz.Simulation.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Commands;

public class CommandProcessor
{
    private readonly Queue<ICompanyCommand>
        _queue = [];

    public void Enqueue(
        ICompanyCommand command)
    {
        _queue.Enqueue(command);
    }

    public void Process(
        SimulationEngine engine)
    {
        while (_queue.Count > 0)
        {
            var command =
                _queue.Dequeue();

            if (!command.Validate(engine))
            {
                continue;
            }

            command.Execute(engine);
        }
    }
}
