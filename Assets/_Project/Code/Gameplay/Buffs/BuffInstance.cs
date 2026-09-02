using System;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    public class BuffInstance
    {
        public Buff source;
        public float timeRemaining;

        public BuffInstance(Buff buff)
        {
            source = buff;
            timeRemaining = buff.duration;
        }
    }
}