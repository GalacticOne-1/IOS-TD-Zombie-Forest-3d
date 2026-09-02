using System.Collections.Generic;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Scene coordinator системы строительства.
    ///
    /// Роль:
    /// - управляет ghost
    /// - делегирует операции ConstructionService
    /// - вызывает создание runtime объектов
    /// </summary>
    public class ConstructionPlacementController : MonoBehaviour
    {
        [SerializeField] private ConstructionGhostController ghostController;

        private ConstructionModeController _controller;
        private GridSettingsConfig _gridSettings;
        private ConstructionService _constructionService;
        private ConstructionRequirementService _requirementService;
        private CameraController _cameraController;
        private PlacementPreviewRuntime _preview;
        
        public ConstructionGhostController GhostController => ghostController;
        public ConstructionService ConstructionService => _constructionService;

        
        
        private FacilityModule _selectedConfig;
        private BuildableObject _movingObject;
        private Vector2Int _originalCell;
        private int _originalRotation;
        Vector2Int? _lastCell;
        
        
        public BuildableObject CurrentGhost => ghostController.Ghost;
        
        private List<Vector2Int> _originCells = new();
        
        
        
        
        // =========================
        // INIT
        // =========================

        public void Initialize(
            ConstructionModeController controller, 
            ConstructionService constructionService,
            ConstructionRequirementService requirementService,
            GridSettingsConfig gridConfig,
            CameraController camera)
        {
            _controller = controller;
            _gridSettings = gridConfig;
            _requirementService = requirementService;
            _cameraController = camera;
            _constructionService = constructionService;

            ghostController.Initialize(
                controller, 
                _constructionService, 
                camera);
        }

        // =========================
        // GHOST
        // =========================

        public void CreateGhost(FacilityModule config)
        {
            _selectedConfig = config;
            _controller.GridShaderRenderer.RefreshBlockedMask(_selectedConfig);

            _lastCell = null;
            _preview = new PlacementPreviewRuntime();
            _preview.Initialize(config);
            _controller.Context.Preview = _preview;

            ghostController.CreateGhost(config);
        }

        public void DestroyGhost()
        {
            if (ghostController.HasGhost)
            {
                // Снять визуальный ghost
                ghostController.DestroyGhost();
                _controller.GridShaderRenderer.RefreshBlockedMask(null);

                _selectedConfig = null;
                
                // Очистка preview
                _preview?.Clear();
                _preview = null;
                _lastCell = null;

                // Сброс контекста
                _controller.Context.CurrentGhost = null;

                _controller.FinishPlacement();
            }
        }
        
        public Vector2Int GetInitialPlacementCell(FacilityModule config, Vector2Int screenCenterCell)
        {
            var footprint = config.FootprintConfig.ToFootprint();
            var rotation = 0;

            // Пробуем стартовую клетку
            var result = _constructionService.ValidatePlacement(screenCenterCell, footprint, rotation, _selectedConfig);
            if (result.IsValid)
                return screenCenterCell;

            // Если занято, ищем ближайшую свободную
            int searchRadius = 1;
            while (searchRadius < Mathf.Max(_gridSettings.GridSize.x, _gridSettings.GridSize.y))
            {
                // Проходим клетки вокруг центра по спирали
                for (int dx = -searchRadius; dx <= searchRadius; dx++)
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    var candidate = screenCenterCell + new Vector2Int(dx, dy);

                    // пропускаем те, что вне сетки
                    if (!_constructionService.Coordinates.IsInsideGrid(candidate))
                        continue;

                    result = _constructionService.ValidatePlacement(candidate, footprint, rotation, _selectedConfig);
                    if (result.IsValid)
                        return candidate;
                }

                searchRadius++;
            }
            
            // UI update
            _controller.UpdatePlacementState(result);

            // Если свободного места нет — возвращаем оригинал
            return screenCenterCell;
        }

        public void MoveGhost(Vector2Int cell)
        {
            if (!ghostController.HasGhost)
                return;

            if (cell == _lastCell)
                return;

            _lastCell = cell;

            bool hasResources = HasResourcesForCurrent();
            var result = UpdatePlacement(
                cell,
                _preview.Config.FootprintConfig.ToFootprint(), // оригинальный
                _preview.Rotation,
                hasResources);
            

            ghostController.MoveTo(cell);
            ghostController.SetValid(result.IsValid && hasResources);
            _cameraController.FocusOnPositionFacility(CurrentGhost.transform.position);
        }
        
        // public void GhostRotation(int rotation)
        // {
        //     if (_preview == null)
        //         return;
        //
        //     _preview.Rotation = rotation;
        //
        //     Vector2Int cell;
        //
        //     if (_lastCell.HasValue)
        //         cell = _lastCell.Value;
        //     else
        //         cell = _preview.Origin;
        //
        //     bool hasResources = HasResourcesForCurrent();
        //     var result = UpdatePlacement(
        //         cell,
        //         _preview.Footprint,
        //         rotation,
        //         hasResources);
        //
        //     ghostController.SetRotation(rotation);
        // }
        public void GhostRotation(int rotation)
        {
            if (_preview == null)
                return;

            _preview.Rotation = rotation;

            // Сначала обновляем footprint, потом всё остальное
            _preview.Footprint = _preview.Config.FootprintConfig
                .ToFootprint()
                .Rotate(rotation);

            Vector2Int cell = _lastCell.HasValue ? _lastCell.Value : _preview.Origin;

            bool hasResources = HasResourcesForCurrent();
            
            UpdatePlacement(
                cell,
                _preview.Footprint, // не повёрнутый
                rotation,
                hasResources);

            ghostController.SetRotation(rotation);
            
            // сбрасываем кэш чтобы MoveGhost не пропустил обновление
            _lastCell = null;
        }
        
        public void MoveGhostNextCell()
        {
            var centerCell = ghostController.CurrentCell();
            
            // 2. Находим подходящую клетку с учётом размера
            Vector2Int nextCell = GetNextPlacementCell(_controller.Context.BuildConfig, centerCell);

            MoveGhost(nextCell);
        }
        
        // *находит место для госта в авто билде
        public Vector2Int GetNextPlacementCell(FacilityModule config, Vector2Int screenCenterCell)
        {
            var footprint = config.FootprintConfig.ToFootprint();
            var rotation = 0;

            // Пробуем стартовую клетку
            var result = _constructionService.ValidatePlacement(screenCenterCell, footprint, rotation, _selectedConfig);
            if (result.IsValid)
                return screenCenterCell;

            int maxRadius = Mathf.Max(_gridSettings.GridSize.x, _gridSettings.GridSize.y);

            for (int r = 1; r < maxRadius; r++)
            {
                // Сначала крест (4 направления)
                Vector2Int[] cross = new Vector2Int[]
                {
                    new Vector2Int(screenCenterCell.x + r, screenCenterCell.y),
                    new Vector2Int(screenCenterCell.x - r, screenCenterCell.y),
                    new Vector2Int(screenCenterCell.x, screenCenterCell.y + r),
                    new Vector2Int(screenCenterCell.x, screenCenterCell.y - r)
                };

                foreach (var candidate in cross)
                {
                    if (!_constructionService.Coordinates.IsInsideGrid(candidate))
                        continue;

                    result = _constructionService.ValidatePlacement(candidate, footprint, rotation, _selectedConfig);
                    if (result.IsValid)
                        return candidate;
                }

                // Потом диагональ (4 направления)
                Vector2Int[] diagonals = new Vector2Int[]
                {
                    new Vector2Int(screenCenterCell.x + r, screenCenterCell.y + r),
                    new Vector2Int(screenCenterCell.x + r, screenCenterCell.y - r),
                    new Vector2Int(screenCenterCell.x - r, screenCenterCell.y + r),
                    new Vector2Int(screenCenterCell.x - r, screenCenterCell.y - r)
                };

                foreach (var candidate in diagonals)
                {
                    if (!_constructionService.Coordinates.IsInsideGrid(candidate))
                        continue;

                    result = _constructionService.ValidatePlacement(candidate, footprint, rotation, _selectedConfig);
                    if (result.IsValid)
                        return candidate;
                }
            }

            // UI update
            _controller.UpdatePlacementState(result);

            // Если свободного места нет — возвращаем оригинал
            return screenCenterCell;
        }
        
        // =========================
        // REQUIREMENTS
        // =========================

        public bool HasResourcesForCurrent()
        {
            if (_selectedConfig == null)
                return false;

            return _requirementService.CanBuild(_selectedConfig);
        }


        // =========================
        // BUILD
        // =========================
        public void Build()
        {
            if (_preview == null || !_preview.IsValid)
                return;
            
            // Списываем ресурсы
            if (!_requirementService.TrySpend(_selectedConfig))
                return;

            var footprint = new BuildingFootprintRuntime(
                _selectedConfig.FootprintConfig,
                _preview.Origin,
                _preview.Rotation);
            
            var runtime = ServiceLocator.Current.Get<GameSession>().GameLoopContext
                .CreateFacilityCompletely(
                    _selectedConfig,
                    footprint);
            
            _controller.ConstructionCompleted();
            
            ServiceLocator.Current.Get<GameSession>().MarkDirty();
        }

        // =========================
        // ROTATION
        // =========================
        public void ApplyRotation(int rotation)
        {
            if (_movingObject == null)
                return;

            // оригинальный, без поворота
            var footprint = _movingObject.FootprintRuntime.Footprint;

            var result = UpdatePlacement(
                _movingObject.FootprintRuntime.Origin,
                footprint,
                rotation);

            // применяем rotation в runtime
            _movingObject.Adapter.SetRotation(rotation);
            
            // сбрасываем кэш чтобы MoveGhost не пропустил обновление
            _lastCell = null;
        }
        

        // =========================
        // MOVE
        // =========================
        
        /// <summary>
        /// Единый pipeline обновления placement.
        /// Используется для ghost и перемещения объектов.
        /// </summary>
        private PlacementValidationResult UpdatePlacement(
            Vector2Int cell,
            BuildingFootprint footprint,
            int rotation,
            bool hasResources = true)
        {
            var result = _constructionService.ValidatePlacement(
                cell,
                footprint,
                rotation,
                _selectedConfig);

            if (!hasResources)
            {
                result.IsValid = false;
                result.Message = "Not enough materials";
            }

            // обновляем preview runtime
            _preview?.SetPlacement(
                cell,
                result.Cells,
                result.IsValid);

            // подсветка сетки
            _controller.GridShaderRenderer.HighlightCells(
                result.Cells,
                result.IsValid);

            // обновление UI
            _controller.UpdatePlacementState(result);

            return result;
        }
        
        
        public void StartMove(BuildableObject buildable)
        {
            _selectedConfig = buildable.FacilityConfig;
            _controller.GridShaderRenderer.RefreshBlockedMask(_selectedConfig);
            
            _movingObject = buildable;
            _originalCell = buildable.FootprintRuntime.Origin;
            _originalRotation = buildable.FootprintRuntime.Rotation;
            
            // Запоминаем стартовые клетки
            _originCells = new List<Vector2Int>(buildable.FootprintRuntime.Cells);
            
            // освобождаем клетки
            _constructionService.Unregister(buildable);

            // обновляем подсветку сетки
            var footprint = buildable.FootprintRuntime.Footprint;

            var result = UpdatePlacement(
                _originalCell,
                footprint,
                _originalRotation);
        }

        
        public void MoveTo(Vector2Int cell)
        {
            if (_movingObject == null)
                return;

            var result = UpdatePlacement(
                cell,
                _movingObject.FootprintRuntime.Footprint,
                _movingObject.FootprintRuntime.Rotation);
            
            // показываем стартовые клетки
            _controller.GridShaderRenderer.HighlightOrigin(_originCells);

            _movingObject.Adapter.SetPosition(cell);
            _cameraController.FocusOnPositionFacility(_movingObject.transform.position);
        }
        
        
        public void ConfirmMove()
        {
            if (_movingObject == null)
                return;

            // регистрируем объект в новых клетках
            _constructionService.Register(_movingObject);
            _controller.GridShaderRenderer.RefreshBlockedMask(null);

            _controller.FinishPlacement();
            _movingObject = null;
            
            ServiceLocator.Current.Get<GameSession>().MarkDirty();
        }
        
        public void CancelMove()
        {
            if (_movingObject == null)
                return;

            // вернуть runtime позицию
            _movingObject.Adapter.SetRotation(_originalRotation);
            _movingObject.Adapter.SetPosition(_originalCell);

            // зарегистрировать обратно
            _constructionService.Register(_movingObject);
            _controller.GridShaderRenderer.RefreshBlockedMask(null);

            _controller.FinishPlacement();

            _movingObject = null;
        }

        // =========================
        // DELETE
        // =========================

        public void DeleteObject(BuildableObject buildable)
        {
            if (buildable == null) 
                return;

            ServiceLocator.Current.Get<GameSession>().GameLoopContext
                .DeleteFacilityCompletely(buildable.Facility.UniqueId);

            ServiceLocator.Current.Get<GameSession>().MarkDirty();

            // Сброс контекста, если был выбран
            if (_controller.Context.SelectedObject == buildable)
                _controller.Context.ClearSelection();
        }

        
        
        
        
        
        
        
        void OnDrawGizmos()
        {
            if (_gridSettings == null)
                return;

            // Настройки сетки
            var width = _gridSettings.GridSize.x;
            var height = _gridSettings.GridSize.y;
            var origin = _gridSettings.GridOffset;
            var cellSize = _gridSettings.CellSize;

            // Цвет пустых клеток
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(
                    origin.x + x * cellSize + cellSize * 0.5f,
                    0,
                    origin.y + y * cellSize + cellSize * 0.5f
                );

                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.01f, cellSize));
            }
            
            // Цвет непроходимых (статически заблокированных) зон
            if (_constructionService != null && _constructionService.BlockedAreas != null)
            {
                

                var blockedCells = _constructionService.BlockedAreas.BlockedCells;

                foreach (var cell in blockedCells)
                {
                    Vector3 worldPos = new Vector3(
                        origin.x + cell.Key.x * cellSize + cellSize * 0.5f,
                        0.005f,
                        origin.y + cell.Key.y * cellSize + cellSize * 0.5f
                    );

                    Gizmos.color = GridZoneColors.Get(cell.Value);
                    Gizmos.DrawCube(worldPos, new Vector3(cellSize, 0.01f, cellSize));
                }
            }

            // Цвет выделенного объекта / ghost
            if (ghostController != null && ghostController.HasGhost)
            {
                Vector2Int ghostCell = ghostController.CurrentCell();
                Vector3 ghostWorld = _constructionService.Coordinates.CellToWorld(ghostCell);
                ghostWorld.y = 0.01f; // чуть выше сетки
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(
                    ghostWorld + new Vector3(cellSize / 2f, 0, cellSize / 2f),
                    new Vector3(cellSize, 0.01f, cellSize)
                );
            }

            // Цвет перемещаемого объекта
            if (_movingObject != null)
            {
                Vector2Int moveCell = _movingObject.FootprintRuntime.Origin;
                Vector3 moveWorld = _constructionService.Coordinates.CellToWorld(moveCell);
                moveWorld.y = 0.02f;
                Gizmos.color = Color.cyan;

                var footprint = _movingObject.FootprintRuntime.Footprint;
                Vector3 size = new Vector3(footprint.Width * cellSize, 0.01f, footprint.Height * cellSize);
                Gizmos.DrawWireCube(moveWorld + new Vector3(size.x / 2f, 0, size.z / 2f), size);
            }
        }
        
        
    }
}