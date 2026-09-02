using System;
using System.Collections.Generic;

namespace Galactic1.Configs
{
    [Serializable]
    public class ManifestFileEntry
    {
        public string key;   // напр. "enemyconfig"
        public string url;   // полный URL до json
        public string hash;  // опционально: MD5/SHA256 строки файла
    }

    [Serializable]
    public class Manifest
    {
        public int version;
        public ManifestFileEntry[] files;
    }
}