using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Единственная точка Request → ITutorialTarget. Линейный перебор зарегистрированных
    /// резолверов (тот же принцип "первый подошедший побеждает", что у AllOf/AnyOf и
    /// transitions) — при 6 резолверах не проблема производительности, а извлечение в
    /// Dictionary<Type,...> добавило бы резолверам обязанность заявлять свой Type отдельно
    /// (string/enum ResolverId, чего ТЗ явно просит избежать). Новый request-тип не требует
    /// изменений в этом классе — только регистрация нового резолвера в DI.
    /// </summary>
    public sealed class TutorialTargetResolverRegistry
    {
        private readonly IReadOnlyList<ITutorialTargetResolver> _resolvers;

        public TutorialTargetResolverRegistry(IReadOnlyList<ITutorialTargetResolver> resolvers)
            => _resolvers = resolvers ?? Array.Empty<ITutorialTargetResolver>();

        public bool TryResolve(TutorialTargetRequest request, out ITutorialTarget target)
        {
            target = null;
            if (request == null) return false;

            foreach (var resolver in _resolvers)
                if (resolver.CanResolve(request))
                    return resolver.TryResolve(request, out target);

            return false;
        }

        public ITutorialTargetResolver FindResolver(TutorialTargetRequest request)
        {
            if (request == null) return null;
            foreach (var resolver in _resolvers)
                if (resolver.CanResolve(request))
                    return resolver;
            return null;
        }
    }
}