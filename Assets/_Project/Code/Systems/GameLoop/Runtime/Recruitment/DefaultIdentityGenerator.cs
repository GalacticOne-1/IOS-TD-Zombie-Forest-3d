using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Runtime
{
    public sealed class DefaultIdentityGenerator : IIdentityGenerator
    {
        private readonly UnitIdentityPoolConfig _unitConfig;

        private readonly RandomNumberGenerator _rng;

        public DefaultIdentityGenerator(UnitIdentityPoolConfig unitConfig)
        {
            _unitConfig = unitConfig;
            _rng = RandomNumberGenerator.Create();
        }

        public UnitIdentity Generate(IReadOnlyCollection<string> usedArchetypeIds = null)
        {
            string id = Guid.NewGuid().ToString("N");
            string archetypeId = GenerateArchetype(usedArchetypeIds);

            // подибраем имя male/female
            var survivorEntry = _unitConfig.GetSurvivorEntry(archetypeId);
            string name = GenerateName(survivorEntry.variant.Female);

            return new UnitIdentity(id, name, archetypeId, DateTime.UtcNow.Ticks);
        }

        private string GenerateName(bool female)
        {
            var nameSet = _unitConfig.GetNameSet(female);
            int first = RandomInt(nameSet.Name.Count);
            int last = RandomInt(nameSet.LastName.Count);

            return $"{nameSet.Name[first]} {nameSet.LastName[last]}";
        }
        
        private string GenerateArchetype(IReadOnlyCollection<string> usedIds)
        {
            var archetype = _unitConfig.GetAvailableArchetype(usedIds ?? Array.Empty<string>());
            return archetype != null ? archetype : string.Empty;
        }

        private int RandomInt(int maxExclusive)
        {
            Span<byte> buffer = stackalloc byte[4];
            _rng.GetBytes(buffer);

            int value = BitConverter.ToInt32(buffer);
            value = Math.Abs(value);

            return value % maxExclusive;
        }

    }
}