using UnityEngine;

namespace Galactic1.Code.UI.Interaction
{
    /// <summary>
    /// MonoBehaviour-адаптер для прокидывания input в UIInputRouter.
    /// </summary>
    public sealed class UIInputRouterBehaviour : MonoBehaviour, IUpdate, IGameService
    {
        private UIInputRouter _router;

        
        
        public void Initialize(UIInputRouter router, MonoBehaviourMaster master)
        {
            _router = router;
            master.update.Add(this);
        }

        
        public void IUpdateClear() {}

        public void UpdateM()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _router.ProcessPointerDown(Input.mousePosition);
            }
        }
    }
}