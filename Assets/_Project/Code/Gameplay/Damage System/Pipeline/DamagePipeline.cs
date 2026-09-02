using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Damage
{
    public sealed class DamagePipeline
    {
        private readonly List<IDamageStep> _steps = new();

        public DamagePipeline Add(IDamageStep step)
        {
            _steps.Add(step);
            return this;
        }

        public DamageResult Execute(DamageContext context)
        {
            foreach (var step in _steps)
            {
                if (context.IsCancelled)
                    break;

                if (!step.Process(context))
                    break;
            }

            return new DamageResult(
                context.BaseDamage,
                context.Damage,
                context.IsCancelled);
        }
    }
}