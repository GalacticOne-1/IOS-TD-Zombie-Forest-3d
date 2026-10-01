using System.Collections.Generic;
using System.Text;

namespace Galactic1.Code.Systems.Analytics
{
    public enum AnalyticsParamType : byte { String, Long, Double }

    public readonly struct AnalyticsParam
    {
        public readonly string Key;
        public readonly AnalyticsParamType Type;
        public readonly string StringValue;
        public readonly long LongValue;
        public readonly double DoubleValue;

        private AnalyticsParam(string key, AnalyticsParamType type, string s, long l, double d)
        {
            Key = key;
            Type = type;
            StringValue = s;
            LongValue = l;
            DoubleValue = d;
        }

        public static AnalyticsParam Of(string key, string value) => new(key, AnalyticsParamType.String, value, 0, 0);
        public static AnalyticsParam Of(string key, long value) => new(key, AnalyticsParamType.Long, null, value, 0);
        public static AnalyticsParam Of(string key, double value) => new(key, AnalyticsParamType.Double, null, 0, value);

        public string ValueToString()
        {
            switch (Type)
            {
                case AnalyticsParamType.String: return StringValue;
                case AnalyticsParamType.Long: return LongValue.ToString();
                default: return DoubleValue.ToString("0.###");
            }
        }
    }

    /// <summary>
    /// Провайдер-независимое описание события. Не знает про Firebase.
    /// Использование: new AnalyticsEvent(AnalyticsEventNames.X).Add(AnalyticsParams.Y, value)
    /// </summary>
    public sealed class AnalyticsEvent
    {
        public readonly string Name;
        public readonly List<AnalyticsParam> Params = new(8);

        public AnalyticsEvent(string name) { Name = name; }

        /// <summary>null/пустая строка пропускаются.</summary>
        public AnalyticsEvent Add(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                Params.Add(AnalyticsParam.Of(key, value));
            return this;
        }

        public AnalyticsEvent Add(string key, long value)
        {
            Params.Add(AnalyticsParam.Of(key, value));
            return this;
        }

        public AnalyticsEvent Add(string key, int value) => Add(key, (long)value);
        public AnalyticsEvent Add(string key, bool value) => Add(key, value ? 1L : 0L);

        public AnalyticsEvent Add(string key, double value)
        {
            Params.Add(AnalyticsParam.Of(key, value));
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Name);
            if (Params.Count == 0) return sb.ToString();

            sb.Append(" {");
            for (int i = 0; i < Params.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Params[i].Key).Append('=').Append(Params[i].ValueToString());
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}