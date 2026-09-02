using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    /// <summary>
    /// Управление главными заставками
    /// </summary>
    public class UIRootView : MonoBehaviour, IGameService
    {
        
        [Serializable]
        public struct CLoadingScreen
        {
            public GameObject screen;
            public Image progressBar;
            public TMP_Text tProgressDes, tProgress, tVersion;
        }

        
        [SerializeField] private CLoadingScreen _loadingScreen;
        [SerializeField] private LocationLoadingScreen locationLoadingScreen;
        [SerializeField] private Transform _containerUiScene;
        [SerializeField] private GameObject _blockScreen, purchaseScreen;
        [field: SerializeField] public Canvas canvas { get; private set; }


        public LocationLoadingScreen LocationLoadingScreen => locationLoadingScreen;


        private void Awake()
        {
            ShowLoadingScreen();
            HideLocationLoadScreen();


            _loadingScreen.progressBar.fillAmount = 0;
            LoadingManager.I.progressBar = _loadingScreen.progressBar;
            LoadingManager.I.tProgress = _loadingScreen.tProgress;
            LoadingManager.I.tProgressDes = _loadingScreen.tProgressDes;
            LoadingManager.I.tVersion = _loadingScreen.tVersion;
        }


        public void SetVersion(string newVersion) => _loadingScreen.tVersion.text = newVersion;
        public string GetVersion() => _loadingScreen.tVersion.text;







        public void ShowLoadingScreen() => _loadingScreen.screen.SetActive(true);

        public void HideLoadingScreen() => _loadingScreen.screen.SetActive(false);


        // загрузочное окно при смене локации
        public void ShowLocationLoadScreen(LocationLoadingScreen.LocationLoadDTO dto)
            => locationLoadingScreen.Entry(dto);
        public void HideLocationLoadScreen() => locationLoadingScreen.gameObject.SetActive(false);
        //


        public void EnableBlockScreen() => _blockScreen.SetActive(true);
        public void DisableBlockScreen() => _blockScreen.SetActive(false);

        public void ShowPurchaseScreen() => purchaseScreen.SetActive(true);
        public void HidePurchaseScreen() => purchaseScreen.SetActive(false);





        public void AttachSceneUI(GameObject sceneUI)
        {
            ClearSceneUI();

            sceneUI.transform.parent = _containerUiScene;
            sceneUI.transform.localPosition = Vector2.zero;
        }

        void ClearSceneUI()
        {
            var l = _containerUiScene.childCount;
            for (int i = 0; i < l; i++)
            {
                Destroy(_containerUiScene.GetChild(i).gameObject);
            }
        }
    }
}