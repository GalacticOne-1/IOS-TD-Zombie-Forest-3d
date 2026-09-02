
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using Galactic1.Runtime.UI.WorldStatus;
using UnityEngine;

namespace Galactic1.UI.WorldStatus
{
    /// <summary>
    /// Создаёт и регистрирует WorkbenchWorldStatusView в UI Canvas.
    /// Регистрируется в DI как singleton.
    ///
    /// FacilityFactory знает только об этом сервисе —
    /// не знает ни про Canvas, ни про префаб View.
    /// </summary>
    public sealed class WorldStatusFactory
    {
        private readonly Camera _camera;
        private readonly RectTransform _container; // родительский контейнер в Canvas

        public WorldStatusFactory(
            Camera camera,
            RectTransform container)
        {
            _camera = camera;
            _container = container;
        }

        /// <summary>
        /// Создаёт View в UI и привязывает к зданию.
        /// Возвращает View для возможности ручного удаления.
        /// </summary>
        public WorkbenchWorldStatusView Create(
            BaseProductionStationRuntime runtime,
            Transform worldTarget)
        {
            // View уже на префабе — ищем на объекте
            var view = $"{AppConstants.PATH_UI_GAMEPLAY}Camp/StationStatus"
                .CreateGO(_container).GetComponent<WorkbenchWorldStatusView>();


            var presenter = new WorkbenchWorldStatusPresenter(
                runtime,
                view);

            view.Bind(presenter, worldTarget, _camera);
            return view;
        }

    }
}