using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Тонкая обёртка над Animator.
    /// Знает только про Animator — ничего про игровую логику.
    /// </summary>
    public sealed class AnimatorBridge : MonoBehaviour
    {
        private Animator _animator;

        [SerializeField] private float dampTime = 0.1f;

        public void Bind() => _animator = GetComponentInChildren<Animator>();

        public void SetFloat(int hash, float value)
            => _animator.SetFloat(hash, value, dampTime, Time.deltaTime);

        public void SetFloat(int hash, float value, float customDamp)
            => _animator.SetFloat(hash, value, customDamp, Time.deltaTime);
        
        public void SetInt(int hash, int value)
            => _animator.SetInteger(hash, value);

        public void SetBool(int hash, bool value)
            => _animator.SetBool(hash, value);

        public void SetTrigger(int hash)
            => _animator.SetTrigger(hash);

        public void CrossFade(string stateName, float duration = 0.15f, int layer = 0)
            => _animator.CrossFadeInFixedTime(stateName, duration, layer);

        public void CrossFade(string stateName, float duration, int layer, float normalizedTime)
            => _animator.CrossFadeInFixedTime(stateName, duration, layer, normalizedTime);
        
        public void CrossFade(int stateHash, float duration = 0.15f, int layer = 0, float normalizedTime = 0)
            => _animator.CrossFadeInFixedTime(stateHash, duration, layer, normalizedTime);
        public void CrossFadeFixed(int stateHash, float durationSeconds, int layer = 0, float offset = 0f)
            => _animator.CrossFadeInFixedTime(stateHash, durationSeconds, layer, offset);

        public void PlayState(string stateName, int layer)
            => _animator.Play(stateName, layer, 0f);

        /// <summary>
        /// Текущее значение Float-параметра (например, для blend tree чтения).
        /// </summary>
        public float GetFloat(int hash) => _animator.GetFloat(hash);

        public bool GetBool(int hash) => _animator.GetBool(hash);

        /// <summary>
        /// Проверяет — проигрывается ли анимация в указанном слое.
        /// Используется для блокировки новых экшнов пока идёт предыдущий.
        /// </summary>
        public bool IsPlaying(string stateName, int layer = 0)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(layer);
            return info.IsName(stateName);
        }

        public void SetOverrideController(AnimatorOverrideController controller)
        {
            if (controller == null) return;

            // Сохраняем текущий нормализованный прогресс
            // чтобы не было резкого сброса анимации
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime % 1f;

            _animator.runtimeAnimatorController = controller;

            // Возобновляем примерно в той же точке
            _animator.Play(stateInfo.fullPathHash, 0, normalizedTime);
        }

        public void ResetAnimator()
        {
            _animator.Rebind();
            _animator.Update(0f);
            _animator.transform.localPosition = Vector3.zero;
            _animator.transform.localRotation = Quaternion.identity;
        }
    }
}