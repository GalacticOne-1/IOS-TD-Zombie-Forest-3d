using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1
{
    public abstract class EntityConfig<T> : ScriptableObject where T : EntityLevelConfigs
    {
        [field: SerializeField] public bool Use { get; private set; }
        
        [field: Space(10)]
        [field: SerializeField] public EntityType EntityType { get; private set; }
        [field: SerializeField] public string ConfigId { get; private set; }
        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }
        [field: SerializeField] public string PrefabPath { get; private set; }
        
        
        [field: Space(10)]
        #region HEADER

        [field: SerializeField] public CHeader Header { get; private set; }
        
        [System.Serializable] 
        public struct CHeader
        {
            public string TitleLid;
            [TextArea]
            public string DescriptionLid;

            public int Order;
            
            [Space]
            public Sprite Icon;
            public float SizeUI;
            public Vector2 IconOffset;
        }
        
        #endregion
        
        
        [field: SerializeField] public List<T> Levels { get; private set; }

        


        [Serializable]
        public abstract class CBaisicWrapper
        {
            public string id;
            public int order;
            public CHeader header;
            public string prefab_path;
            
            [Serializable] 
            public class CHeader
            {
                public string title_lid;
                public string description_lid;
                public string icon; // путь к иконке, загрузим из Resources
                public float size_ui;
                public Vector2 icon_offset;
            }
        }
        
        
        
        /// <summary>
        /// Обновляет ScriptableObject из JSON
        /// </summary>
        public virtual void UpdateFromJson<TData>(string json) where TData : class{}

        public void BasicFromJson(CBaisicWrapper data)
        {
            if (data.header != null)
            {
                Header = new CHeader
                {
                    TitleLid = data.header.title_lid,
                    DescriptionLid = data.header.description_lid,
                    Order = data.order,
                    SizeUI = data.header.size_ui,
                    IconOffset = data.header.icon_offset,
                    Icon = Header.Icon
                    // Icon = !string.IsNullOrEmpty(data.header.icon)
                    //     ? Resources.Load<Sprite>(data.header.icon)
                    //     : null
                };
            }

            
            if (!string.IsNullOrEmpty(data.prefab_path))
                PrefabPath = data.prefab_path;
        }
    }



    [CreateAssetMenu(fileName = "EntityConfigs", menuName = "Game Configs/Entities/New Entity Configs")]
    public class EntityConfig : EntityConfig<EntityLevelConfigs>
    {

    }
}