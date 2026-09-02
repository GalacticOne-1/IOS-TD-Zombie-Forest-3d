
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Definitions;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Gameplay.Enemies.Factories
{
    public sealed class EnemyPresentationDefinitionBuilder
    {
        public EnemyPresentationDefinitionData Build(EnemyPresentationConfig config)
        {
            return new EnemyPresentationDefinitionData(
                config.BasePrefabId,
                config.BaseVisualId,
                config.Animator,
                config.LocomotionSet,
                config.AnimationVariants,
                config.Avatar,
                config.audioConfig,
                BuildThemes(config.Themes));
        }

        private static IReadOnlyList<EnemyThemePresentationDefinition> BuildThemes(
            EnemyThemePresentation[] themes)
        {
            if (themes == null || themes.Length == 0)
                return System.Array.Empty<EnemyThemePresentationDefinition>();

            var result = new EnemyThemePresentationDefinition[themes.Length];
            for (int i = 0; i < themes.Length; i++)
            {
                var t = themes[i];
                result[i] = new EnemyThemePresentationDefinition(
                    t.Theme,
                    t.PrefabPrefix,
                    t.VariantsCount);
            }

            return result;
        }
    }
}