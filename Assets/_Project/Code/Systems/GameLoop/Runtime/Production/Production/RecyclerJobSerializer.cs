using System.Collections.Generic;
using Newtonsoft.Json;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Сериализация multi-output для хранения в ProductionJobProxy.MetadataJson.
    /// Runtime остаётся чистым.
    /// </summary>
    public static class RecyclerJobSerializer
    {
        public static string Serialize(List<RecyclerJobOutput> outputs)
        {
            return JsonConvert.SerializeObject(outputs);
        }

        public static List<RecyclerJobOutput> Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new List<RecyclerJobOutput>();

            return JsonConvert.DeserializeObject<List<RecyclerJobOutput>>(json);
        }
    }
}