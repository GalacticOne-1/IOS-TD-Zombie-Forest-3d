using System.Collections.Generic;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime footprint of a building placed in grid.
    /// </summary>
    public class BuildingFootprintRuntime
    {
        public BuildingFootprint Footprint { get; }

        public Vector2Int Origin { get; private set; }

        public int Rotation { get; private set; }

        public List<Vector2Int> Cells { get; private set; }
        
        

        public BuildingFootprintRuntime(
            FacilityFootprintConfig config,
            Vector2Int origin,
            int rotation)
        {
            Footprint = config.ToFootprint();
            Origin = origin;
            Rotation = rotation;
            
            RecalculateCells();
        }

        public void Move(Vector2Int newOrigin)
        {
            Origin = newOrigin;
            RecalculateCells();
        }

        
        public void Rotate(int rotation)
        {
            Rotation = rotation;
            RecalculateCells();
        }
        
        void RecalculateCells()
        {
            Cells = new List<Vector2Int>();

            var rotated = Footprint.Rotate(Rotation);

            int width = rotated.Width;
            int height = rotated.Height;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Cells.Add(new Vector2Int(
                    Origin.x + x,
                    Origin.y + y));
            }
        }
        
        
    }
}