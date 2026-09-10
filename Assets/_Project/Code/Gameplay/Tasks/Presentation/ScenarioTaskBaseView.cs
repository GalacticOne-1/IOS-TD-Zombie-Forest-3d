using TMPro;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    public abstract class ScenarioTaskBaseView : MonoBehaviour
    {
        [SerializeField] protected GameObject root;
        [SerializeField] protected TMP_Text desText;


        public abstract void Show(string description);
    }
}