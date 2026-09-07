
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression.Editor
{
    /// <summary>
    /// Editor-only utilities for polymorphic UnlockRequirement types.
    ///
    /// Runtime code must never depend on this class.
    /// </summary>
    internal static class UnlockRequirementEditorUtility
    {
        private static Type[] _requirementTypes;

        public static IReadOnlyList<Type> RequirementTypes
        {
            get
            {
                if (_requirementTypes == null)
                    _requirementTypes = FindRequirementTypes();

                return _requirementTypes;
            }
        }

        private static Type[] FindRequirementTypes()
        {
            var types = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    assemblyTypes = exception.Types
                        .Where(type => type != null)
                        .ToArray();
                }

                foreach (var type in assemblyTypes)
                {
                    if (type == null)
                        continue;

                    if (type.IsAbstract)
                        continue;

                    if (!type.IsClass)
                        continue;

                    if (!typeof(UnlockRequirement).IsAssignableFrom(type))
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    types.Add(type);
                }
            }

            return types
                .Distinct()
                .OrderBy(type => type.Name)
                .ToArray();
        }

        public static string GetDisplayName(Type type)
        {
            if (type == typeof(ProgressionLevelRequirement))
                return "Progression Level Requirement";

            string name = type.Name;

            if (name.EndsWith("Requirement", StringComparison.Ordinal))
            {
                name = name.Substring(
                    0,
                    name.Length - "Requirement".Length);
            }

            return ObjectNames.NicifyVariableName(name);
        }

        public static void ClearCache()
        {
            _requirementTypes = null;
        }
    }
}
