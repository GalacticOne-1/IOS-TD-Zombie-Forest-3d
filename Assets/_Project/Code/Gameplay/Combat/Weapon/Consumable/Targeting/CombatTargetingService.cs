using System;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;
using Galactic1.Code.Systems.Raid;
using Galactic1.Core.UI.HUD;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Targeting
{
    /// <summary>
    /// Scene-level сервис таргетинга (визуал + spawn).
    /// НЕ управляет input — только реагирует на pipeline.
    /// </summary>
    public sealed class CombatTargetingService : MonoBehaviour
    {
        [SerializeField] private string radiusPrefabPath;

        private TargetingInputPipeline _input;
        private TargetingValidationService _validator;
        private IUnitRuntime _user;
        private SurvivorInstance _instance;
        
        private IAbilityVisual _visual;
        private LineAbilityRender _line;
        private UnitOutlineEffect _outline;

        private UseModule _config;
        private Action<Vector3> _onConfirm;
        private Action _onCancel;

        private Vector3 _lastValidPosition;
        private bool _lastPositionValid;
        

        // =========================
        // Init
        // =========================
        public void Initialize(TargetingInputPipeline input)
        {
            _input = input;

            var go = radiusPrefabPath.CreateGO(ServiceLocator.Current.Get<Environment>().obj);
            go.SetActive(false);
            
            _visual = new CircleAbilityVisual(go);
            _line = new LineAbilityRender(GetComponent<LineRenderer>());
            _validator = new TargetingValidationService();


            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(() =>
            {
                _input.SetCancelZone(FindAnyObjectByType<AbilityCancelZone>());
            }));
        }

        // =========================
        // API
        // =========================
        public void StartTargeting(
            IUnitRuntime user,
            UseModule config, 
            Action<Vector3> onConfirm,
            Action onCancel)
        {
            _user = user;
            _config = config;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            
            var repo = ServiceLocator.Current.Get<SurvivorRepository>().TryGet(_user.Id);
            _instance = repo.instance;

            _input.OnStart += HandleStart;
            _input.OnUpdate += HandleUpdate;
            _input.OnConfirm += HandleConfirm;
            _input.OnCancel += HandleCancel;

            _input.Activate();
            
            
            // Радиус AOE из behaviour если это граната, иначе fallback на Range
            float explosionRadius = (config.Behaviour as GrenadeBehaviour).ExplosionRadius;
            float outerExplosionRadius = (config.Behaviour as GrenadeBehaviour).OuterExplosionRadius;

            _visual.SetRadius(explosionRadius, outerExplosionRadius);
            
            // outline на юните
            _outline = new UnitOutlineEffect(_instance.gameObject);
            _outline.Show();
            
            // показываем визуал сразу по центру экрана
            ShowAtScreenCenter();
        }

        public void StopTargeting()
        {
            _input.OnStart -= HandleStart;
            _input.OnUpdate -= HandleUpdate;
            _input.OnConfirm -= HandleConfirm;
            _input.OnCancel -= HandleCancel;

            _input.Deactivate();
            _visual.Hide();
            
            _outline?.Hide();
            _outline = null;

            _line.Hide();

            _config = null;
            _onConfirm = null;
            _onCancel = null;
        }

        // =========================
        // Pipeline handlers
        // =========================
        private void HandleStart(Vector3 pos)
        {
            _visual.Show();
        }

        private void HandleUpdate(Vector3 pos)
        {
            bool valid = _config.Behaviour.ValidateTarget(
                _instance.GetEyePoint().position, 
                pos, 
                _config, 
                out var projected);

            if (valid)
                _lastValidPosition = projected;
            
            _lastPositionValid = valid;

            _visual.Update(projected, valid);
            
            // обновляем линию
            _line.Show();
            _line.Update(_instance.Tr.position, projected, valid);
        }

        private void HandleConfirm(Vector3 pos)
        {
            if (!_lastPositionValid)
                return;
            
            _onConfirm?.Invoke(_lastValidPosition);
            StopTargeting();
        }

        private void HandleCancel()
        {
            _onCancel?.Invoke();
            StopTargeting();
        }

        
        
        private void ShowAtScreenCenter()
        {
            var cam = Camera.main;
            if (cam == null) return;

            var screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            var ray = cam.ScreenPointToRay(screenCenter);

            if (Physics.Raycast(ray, out var hit))
            {
                bool valid = _config.Behaviour.ValidateTarget(
                    _instance.GetEyePoint().position, hit.point, _config, out var projected);
                _lastPositionValid = valid;
                _lastValidPosition = projected;
                _visual.Show();
                _visual.Update(projected, true);
            }
            else
            {
                // fallback: показываем перед юнитом, если рейкаст не попал
                var origin = _instance.Tr.position;
                var forward = _instance.Tr.forward;
                _lastPositionValid = false;
                _lastValidPosition = origin + forward * 3f;
                _visual.Show();
                _visual.Update(_lastValidPosition, false);
            }
            
            // линия сразу от юнита к центру
            _line.Show();
            _line.Update(_instance.Tr.position, _lastValidPosition, true);
        }

    }
}