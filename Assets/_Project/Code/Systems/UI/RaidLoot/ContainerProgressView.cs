using Galactic1.Code.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.RaidLoot
{
    /// <summary>
    /// Pooled progress bar UI.
    /// Только отображение.
    /// </summary>
    public sealed class ContainerProgressView : MonoBehaviour
    {
        [SerializeField] private WorldUIFollow _follow;
        [SerializeField] private Image _fill;


        public void Attach(Vector3 target, Camera camera)
        {
            _follow.Attach(target, camera);
        }

        public void SetProgress(float progress)
        {
            if (_fill != null)
                _fill.fillAmount = progress;
        }

        public void ResetView()
        {
            SetProgress(0f);
        }
    }
}