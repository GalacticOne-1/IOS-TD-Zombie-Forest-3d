using System.Collections.Generic;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    public class GridShaderRenderer : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [SerializeField] private GameObject gridPrefab;

        [Header("Grid Material")] [SerializeField]
        private Material gridMaterial;

        [SerializeField] private Color validColor;
        [SerializeField] private Color invalidColor;

        private static readonly int CellSizeID = Shader.PropertyToID("_CellSize");
        private static readonly int GridOffsetID = Shader.PropertyToID("_GridOffset");
        private static readonly int HoverCellID = Shader.PropertyToID("_HoverCell");
        private static readonly int FootprintMinID = Shader.PropertyToID("_FootprintMin");
        private static readonly int FootprintMaxID = Shader.PropertyToID("_FootprintMax");
        private static readonly int FootprintColorID = Shader.PropertyToID("_FootprintColor");
        private static readonly int OriginMinID = Shader.PropertyToID("_OriginMin");
        private static readonly int OriginMaxID = Shader.PropertyToID("_OriginMax");
        private static readonly int OriginColorID = Shader.PropertyToID("_OriginColor");
        private static readonly int GridVisibleID = Shader.PropertyToID("_GridVisible");
        private static readonly int BlockedMaskID = Shader.PropertyToID("_BlockedMask");
        private static readonly int BlockedColorID = Shader.PropertyToID("_BlockedColor");

        private static readonly Vector4 InvalidCell = new Vector4(-9999, -9999, 0, 0);

        // === blocked mask state ===
        private Texture2D _blockedMaskTexture;
        private GridSettingsConfig _gridConfig;
        private GridBlockedAreaService _blockedAreaService;

        private void Awake()
        {
            ClearHover();
            ClearFootprint();
        }

        private void OnDestroy()
        {
            if (_blockedMaskTexture != null)
                Destroy(_blockedMaskTexture);
        }

        public void CreateGrid(GridSettingsConfig config)
        {
            var grid = gridPrefab.CreateGO(root);

            var gridSize = config.GridSize;
            var offset = config.GridOffset;
            var cellSize = config.CellSize;

            float width = gridSize.x * cellSize;
            float height = gridSize.y * cellSize;

            grid.transform.localScale = new Vector3(width, height, 1f);

            grid.transform.position = new Vector3(
                offset.x + width * 0.5f,
                .3f,
                offset.y + height * 0.5f
            );

            gridMaterial.SetFloat(CellSizeID, cellSize);
            gridMaterial.SetVector(GridOffsetID, new Vector4(offset.x, offset.y, 0, 0));
        }

        // =========================
        // BLOCKED MASK
        // =========================

        /// <summary>
        /// Инициализирует систему blocked-mask. Вызывается один раз при загрузке сцены
        /// (см. CampRegistrations). Создаёт текстуру под дефолтное состояние (без учёта
        /// конкретного здания — любая заблокированная клетка красная).
        /// </summary>
        public void InitializeBlockedMask(
            GridSettingsConfig gridConfig,
            GridBlockedAreaService blockedAreaService)
        {
            _gridConfig = gridConfig;
            _blockedAreaService = blockedAreaService;

            _blockedMaskTexture = GridBlockedMaskGenerator.Build(gridConfig, blockedAreaService);
            gridMaterial.SetTexture(BlockedMaskID, _blockedMaskTexture);
        }

        /// <summary>
        /// Пересобирает существующую маску под allowedZoneTags конкретного здания.
        /// config == null — сброс к дефолтному состоянию (любой тег = заблокировано).
        /// Переиспользует уже созданную текстуру — новых аллокаций нет.
        /// </summary>
        public void RefreshBlockedMask(FacilityModule config)
        {
            if (_blockedMaskTexture == null || _gridConfig == null || _blockedAreaService == null)
                return; // InitializeBlockedMask ещё не был вызван

            GridBlockedMaskGenerator.Rebuild(
                _blockedMaskTexture,
                _gridConfig,
                _blockedAreaService,
                config);

            // текстура та же самая, но на всякий случай (editor / material instancing) переустановим
            gridMaterial.SetTexture(BlockedMaskID, _blockedMaskTexture);
        }

        public void SetBlockedColor(Color color)
        {
            gridMaterial.SetColor(BlockedColorID, color);
        }

        // =========================
        // (остальной код без изменений: HighlightCells, HighlightOrigin,
        // Reset, ShowGrid/HideGrid, SetHoverCell, ClearHover, ClearFootprint) 
        // =========================

        public void HighlightCells(List<Vector2Int> cells, bool valid)
        {
            if (cells == null || cells.Count == 0)
            {
                ClearFootprint();
                return;
            }

            Vector2Int min = cells[0];
            Vector2Int max = cells[0];

            foreach (var c in cells)
            {
                if (c.x < min.x) min.x = c.x;
                if (c.y < min.y) min.y = c.y;
                if (c.x > max.x) max.x = c.x;
                if (c.y > max.y) max.y = c.y;
            }

            max.x += 1;
            max.y += 1;

            gridMaterial.SetVector(FootprintMinID, new Vector4(min.x, min.y, 0, 0));
            gridMaterial.SetVector(FootprintMaxID, new Vector4(max.x, max.y, 0, 0));

            gridMaterial.SetColor(FootprintColorID, valid ? validColor : invalidColor);
        }

        public void HighlightOrigin(List<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                gridMaterial.SetVector(OriginMinID, InvalidCell);
                gridMaterial.SetVector(OriginMaxID, InvalidCell);
                return;
            }

            Vector2Int min = cells[0];
            Vector2Int max = cells[0];

            foreach (var c in cells)
            {
                if (c.x < min.x) min.x = c.x;
                if (c.y < min.y) min.y = c.y;
                if (c.x > max.x) max.x = c.x;
                if (c.y > max.y) max.y = c.y;
            }

            max.x += 1;
            max.y += 1;

            gridMaterial.SetVector(OriginMinID, new Vector4(min.x, min.y, 0, 0));
            gridMaterial.SetVector(OriginMaxID, new Vector4(max.x, max.y, 0, 0));
            gridMaterial.SetColor(OriginColorID, validColor);
        }

        public void Reset(object _)
        {
            ClearHover();
            ClearFootprint();
            ClearOrigin();
        }

        private void ClearOrigin()
        {
            gridMaterial.SetVector(OriginMinID, InvalidCell);
            gridMaterial.SetVector(OriginMaxID, InvalidCell);
        }

        public void ShowGrid() => gridMaterial.SetFloat(GridVisibleID, 1f);
        public void HideGrid() => gridMaterial.SetFloat(GridVisibleID, 0f);

        public void SetHoverCell(Vector2Int cell)
        {
            gridMaterial.SetVector(HoverCellID, new Vector4(cell.x, cell.y, 0, 0));
        }

        public void ClearHover()
        {
            gridMaterial.SetVector(HoverCellID, InvalidCell);
        }

        private void ClearFootprint()
        {
            gridMaterial.SetVector(FootprintMinID, InvalidCell);
            gridMaterial.SetVector(FootprintMaxID, InvalidCell);
        }
    }
}