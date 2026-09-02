using System;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public abstract class _EntityConfig_ : EntityConfig<BuildableLevelConfigs>
    {
        




        #region STAT

        public struct CStatGUI
        {
            public string title, value;
            public Sprite icon;
        }
        
        /// <summary>
        /// Получение списка аттрибутов объекта
        /// <br/>(for UI)
        /// </summary>
        /// <returns></returns>
        public virtual CStatGUI[] GetStatGUI() => null;
        

        #endregion
        
        
        
    }


    public interface IAssetHeader
    {
        public _EntityConfig_.CHeader Header { get; }
        
        public ERarities Rare { get; }
        
        public bool Stackable { get; }
    }

    public interface IAssetSorting
    {
        public ERarities Rare { get; }
        public ESorting Sorting { get; }
        
        public int SortingTypeOrder { get; }
        public int SortingOrder { get; }
    }

    public interface IAssetHint
    {
        public _EntityConfig_.CHeader Header { get; }

        /// <summary>
        /// Основные хар-ки предмета
        /// </summary>
        /// <returns></returns>
        public string GetMainFeatures();
    }
    
    
}