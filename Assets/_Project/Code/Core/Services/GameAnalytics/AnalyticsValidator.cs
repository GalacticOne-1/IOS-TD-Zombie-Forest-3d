namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>Лимиты Firebase Analytics. Значения нужно сверить с документацией
    /// установленной версии SDK; менять только здесь.</summary>
    public static class FirebaseLimits
    {
        public const int MaxNameLength = 40;
        public const int MaxParamsPerEvent = 25;
        public const int MaxStringValueLength = 100;
        public const int MaxUserPropertyNameLength = 24;
        public const int MaxUserPropertyValueLength = 36;
    }

    /// <summary>Проверка контракта. Вызывается только в Editor/Development build.</summary>
    public static class AnalyticsValidator
    {
        private static readonly string[] ReservedPrefixes = { "firebase_", "google_", "ga_" };

        public static bool IsValid(AnalyticsEvent e, out string error)
        {
            if (!IsValidName(e.Name, FirebaseLimits.MaxNameLength, out error))
            {
                error = $"event '{e.Name}': {error}";
                return false;
            }

            if (e.Params.Count > FirebaseLimits.MaxParamsPerEvent)
            {
                error = $"event '{e.Name}': {e.Params.Count} params (max {FirebaseLimits.MaxParamsPerEvent})";
                return false;
            }

            for (int i = 0; i < e.Params.Count; i++)
            {
                var p = e.Params[i];
                if (!IsValidName(p.Key, FirebaseLimits.MaxNameLength, out var perr))
                {
                    error = $"event '{e.Name}', param '{p.Key}': {perr}";
                    return false;
                }

                if (p.Type == AnalyticsParamType.String && p.StringValue.Length > FirebaseLimits.MaxStringValueLength)
                {
                    error = $"event '{e.Name}', param '{p.Key}': value longer than {FirebaseLimits.MaxStringValueLength}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        public static bool IsValidUserProperty(string name, string value, out string error)
        {
            if (!IsValidName(name, FirebaseLimits.MaxUserPropertyNameLength, out error))
            {
                error = $"user property '{name}': {error}";
                return false;
            }

            if (value != null && value.Length > FirebaseLimits.MaxUserPropertyValueLength)
            {
                error = $"user property '{name}': value longer than {FirebaseLimits.MaxUserPropertyValueLength}";
                return false;
            }

            error = null;
            return true;
        }

        private static bool IsValidName(string name, int maxLength, out string error)
        {
            if (string.IsNullOrEmpty(name))
            {
                error = "empty name";
                return false;
            }

            if (name.Length > maxLength)
            {
                error = $"name longer than {maxLength}";
                return false;
            }

            if (!IsAsciiLetter(name[0]))
            {
                error = "name must start with a letter";
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (!IsAsciiLetter(c) && !(c >= '0' && c <= '9') && c != '_')
                {
                    error = $"invalid character '{c}'";
                    return false;
                }
            }

            for (int i = 0; i < ReservedPrefixes.Length; i++)
            {
                if (name.StartsWith(ReservedPrefixes[i]))
                {
                    error = $"reserved prefix '{ReservedPrefixes[i]}'";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static bool IsAsciiLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}