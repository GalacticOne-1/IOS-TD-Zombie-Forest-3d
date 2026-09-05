using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Простая текстовая панель инструкции. textKey сейчас используется как
    /// прямой текст — подключение локализации, если появится LID-система в проекте
    /// (как titleLid/descriptionLid у LocationConfig.CHeader) — отдельная точка интеграции.</summary>
    public sealed class TutorialInstructionView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMPro.TMP_Text text;

        public void Show(string textKey)
        {
            text.text = textKey; // TODO: LocalizationService.Resolve(textKey), если появится
            root.SetActive(true);
        }

        public void Hide() => root.SetActive(false);
    }
}
