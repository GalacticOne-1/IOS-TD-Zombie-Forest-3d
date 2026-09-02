using System;
using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// Сервис occupancy сетки.
    ///
    /// Роль:
    /// - хранит занятые клетки
    /// - регистрирует здания
    /// - проверяет пересечения
    /// </summary>
    public class GridService
    {
        private readonly Dictionary<Vector2Int, GridCellOccupancy> _occupied = new();

        public bool IsCellFree(Vector2Int cell)
        {
            return !_occupied.ContainsKey(cell);
        }

        public void Register(
            BuildableObject obj, 
            BuildingFootprintRuntime footprint,
            GridOccupancyType type)
        {
            foreach (var cell in footprint.Cells)
            {
                if (_occupied.ContainsKey(cell))
                    throw new Exception($"Cell {cell} already occupied");
                
                _occupied[cell] = new GridCellOccupancy(obj, type);
            }
        }

        public void Unregister(BuildableObject obj, BuildingFootprintRuntime footprint)
        {
            foreach (var cell in footprint.Cells)
            {
                if (_occupied.TryGetValue(cell, out var occ))
                {
                    if (occ.Object == obj)
                        _occupied.Remove(cell);
                }
            }
        }
        
        
        
        public bool IsAreaFree(IEnumerable<Vector2Int> cells)
        {
            foreach (var c in cells)
                if (_occupied.ContainsKey(c))
                    return false;

            return true;
        }
        public BuildableObject GetObjectAt(Vector2Int cell)
        {
            _occupied.TryGetValue(cell, out var occ);
            return occ.Object;
        }
        
        public GridOccupancyType GetCellType(Vector2Int cell)
        {
            if (_occupied.TryGetValue(cell, out var occ))
                return occ.Type;

            return GridOccupancyType.None;
        }
        public bool IsObstacle(Vector2Int cell)
        {
            if (_occupied.TryGetValue(cell, out var occ))
                return occ.Type == GridOccupancyType.Obstacle;

            return false;
        }
        
        public bool IsOccupied(Vector2Int cell)
        {
            return _occupied.ContainsKey(cell);
        }
        
        public void Clear()
        {
            _occupied.Clear();
        }

        public List<Vector2Int> GetOccupiedCells()
        {
            return new List<Vector2Int>(_occupied.Keys);
        }
    }
}