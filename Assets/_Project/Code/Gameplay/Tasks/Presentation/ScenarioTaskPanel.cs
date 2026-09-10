using System.Collections.Generic;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>
    /// Persistent HUD-панель списка активных задач — источник-агностична, не знает про
    /// Tutorial/Daily Task/Scenario. Живёт по тому же паттерну, что HUDCamp/HUDMap/
    /// HUDLocation: создаётся один раз в UIScreenManager.PreloadScreens под
    /// _layerRoot.hudRoot и сама активируется в Initialize() — никогда не проходит через
    /// UIScreenManager.OpenScreen() (тот эксклюзивен и закрыл бы её вместе с остальными
    /// экранами при любом OpenScreen где-либо в игре).
    ///
    /// contentRoot назначается как Content существующего ScrollRect в префабе —
    /// панель сама ничего не знает про сам ScrollRect, только про точку для дочерних
    /// ScenarioTaskView.
    ///
    /// ⚠️ Требует UIScreenId.ScenarioTaskHUD в enum + добавления в UIScreenManager.GetRoot
    /// (→ hudRoot, по аналогии с TutorialHUD) и в массив, передаваемый в PreloadScreens.
    /// </summary>
    public sealed class ScenarioTaskPanel : UIScreenPanel
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private ScenarioTaskView viewPrefab;

        private IScenarioTaskService _taskService;
        private readonly List<ScenarioTaskView> _pool = new();

        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            gameObject.SetActive(true);

            _taskService = ServiceLocator.Current.Get<IScenarioTaskService>();
            _taskService.OnTasksChanged += Refresh;
            Refresh();
        }

        public override void Remove()
        {
            base.Remove();
            if (_taskService != null)
                _taskService.OnTasksChanged -= Refresh;
        }

        private void Refresh()
        {
            var tasks = _taskService.GetTasks();

            EnsurePoolSize(tasks.Count);
            for (int i = 0; i < _pool.Count; i++)
                _pool[i].gameObject.SetActive(i < tasks.Count);

            for (int i = 0; i < tasks.Count; i++)
                _pool[i].Bind(tasks[i]);



            scrollRect.SetSizeContentLayoutGroup(true, null, true, true);
        }

        private void EnsurePoolSize(int count)
        {
            while (_pool.Count < count)
                _pool.Add(Instantiate(viewPrefab, scrollRect.content));
        }
    }
}