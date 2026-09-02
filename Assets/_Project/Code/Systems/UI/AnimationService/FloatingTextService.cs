using System;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class FloatingTextService : MonoBehaviour, IGameService
    {

        [SerializeField] private RectTransform container;
        [SerializeField] private FloatingTextView prefab;


        public void ShowText(Vector2 screenPosition, string text, Color color)
        {
            var view = Instantiate(prefab, container);
            view.Play(text, color, screenPosition);
        }
        
    }

}