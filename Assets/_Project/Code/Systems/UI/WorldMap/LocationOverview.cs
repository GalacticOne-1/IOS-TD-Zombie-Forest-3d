
using Galactic1.Code.Notification;
using Galactic1.Code.Utility;
using Galactic1.Code.WorldMap.Intel;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.RaidLoot.Authoring;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Отвечает за отображение доступности локаций, маршрутов, дней и предупреждений орды.
    /// </summary>
    public class LocationOverview : UIScreenPanel
    {
        [Header("Header")]
        [SerializeField] private TMP_Text locationTitleText;
        [SerializeField] private TMP_Text homeDescriptionText;

        [Header("Feature Block Root")]
        [SerializeField] private GameObject intelBlockRoot;
        [SerializeField] private GameObject equipmentAlert;

        [Header("Resources Volume")]
        [SerializeField] private ScrollRect scrollResources;
        [SerializeField] private GameObject resourcesCategoryItemPrefab;
        

        [Header("Time Block Root")] 
        [SerializeField] private GameObject timeToLocation;
        [SerializeField] private GameObject timeBlockRoot;
        [SerializeField] private GameObject threatAlert;

        [Header("Buttons")]
        [SerializeField] private GameObject startButton;
        [SerializeField] private GameObject closeButton, closeButtin2;


        private GameObject[] resCategoryCash;



       
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            // =======
            ServiceLocator.Current.Get<WorldMapController>().locationOverview = this;
            closeButton.RegisterButtonClick(Hide);
            closeButtin2.RegisterButtonClick(Hide);
            
            
            // === load category items
            var styleConfig = ServiceLocator.Current.Get<UIStyleResolver>().locationIntelConfig;
            resCategoryCash = new GameObject[6];
            for (int i = 0; i < 6; i++)
            {
                resCategoryCash[i] = resourcesCategoryItemPrefab.CreateGO(scrollResources.content);

                var style= styleConfig.GetColorCategoryVolume((ResourceVolume)i + 1);
                resCategoryCash[i].GetChild(0).CMP_Image().sprite = style.sprite;
                resCategoryCash[i].GetChild(0).CMP_Image().color = style.color;
            }
            HideResourcesCategory();
        }

        public override void Remove()
        {
            base.Remove();
        }
        

        /// <summary>
        /// Отображить информацию о выбранной локации
        /// </summary>
        public void ShowNodeInfo(
            MapNode node,
            bool canVisit,
            float raidCost,
            float backToBaseCost,
            float daysUntilThreat,
            System.Action action
        )
        {
            //gameObject.SetActive(true);
            startButton.RegisterButtonClick(() =>
            {
                Hide();
                action.Invoke();
            });
            
            
            
            var config = node.Config;

            bool isHome = node.Id == GameIdProvider.Home;

            // 1. Название + уровень
            locationTitleText.text = isHome
                ? $"{config.Header.TitleLid}"
                : $"{config.Header.TitleLid}  "; // (Lvl. {config.RequiresLevel})

            
            // всегда отображаем время до локации
            timeToLocation.CMP_Text().text = "Path to location:";
            timeToLocation.GetChild(0).CMP_Text().text = TimeUtils.FormatTime(raidCost);
                // $"{DayTimeFormatter.Format(raidCost)} days";

            if (isHome)
            {
                homeDescriptionText.gameObject.SetActive(true);
                timeBlockRoot.SetActive(false);
                threatAlert.SetActive(false);
                intelBlockRoot.SetActive(false);
                equipmentAlert.SetActive(false);
                return;
            }
            
            homeDescriptionText.gameObject.SetActive(false);
            
            // 2. Блок времени
            timeBlockRoot.SetActive(true);
            timeBlockRoot.GetChild(1).CMP_Text().text = "Back to camp:";
            timeBlockRoot.GetChild(1,0).CMP_Text().text = TimeUtils.FormatTime(backToBaseCost);
                //$"{DayTimeFormatter.Format(backToBaseCost)} days";

            
            if (daysUntilThreat == -1)  // угрозы нет
            {
                timeBlockRoot.GetChild(3).SetActive(false);
                timeBlockRoot.GetChild(4).SetActive(false);
                threatAlert.SetActive(false);
            }
            else
            {
                float timeLeft = daysUntilThreat - (raidCost + backToBaseCost);
                
                timeBlockRoot.GetChild(3).CMP_Text().text = "Time left after raid:";
                timeBlockRoot.GetChild(3, 0).CMP_Text().text = TimeUtils.FormatTime(Mathf.Max(0f, timeLeft));
                    //$"{DayTimeFormatter.Format(Mathf.Max(0f, timeLeft))} days";
                
                timeBlockRoot.GetChild(4).CMP_Text().text = "Until horde attack:";
                timeBlockRoot.GetChild(4, 0).CMP_Text().text = TimeUtils.FormatTime(daysUntilThreat);
                    //$"{DayTimeFormatter.Format(daysUntilThreat)} days";
                
                timeBlockRoot.GetChild(3).SetActive(true);
                timeBlockRoot.GetChild(4).SetActive(true);
                threatAlert.SetActive(timeLeft < 0);
            }
            
            
            // 3. Блок разведки (что есть в локации)
            UpdateIntelBlock(config);
            // алерт спец. требования локации
            equipmentAlert.SetActive(false);
            
            
            // TODO
            // 3. Requires level
            // ...
        }


        /// <summary>
        /// Закрыть панель
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            HideResourcesCategory();
        }
        
        
        private void UpdateIntelBlock(LocationConfig config)
        {
            var intel = config.LocationIntel;

            var styleConfig = ServiceLocator.Current.Get<UIStyleResolver>().locationIntelConfig;

            // === danger
            UpdateIntel(
                0,
                "Enemies:",
                LootVolumeExtensions.GetActiveIcons((int)intel.threatLevel),
                styleConfig.threatLevel,
                styleConfig
            );
            
            // === possible loot
            int n = 0;
            LootEconomyCategory[] economyCategory;
            var l = resCategoryCash.Length;
            for (int i = 0; i < l; i++)
            {
                economyCategory = intel.resourcesVolume[i].lootEconomyCategory;
                if (economyCategory == null || economyCategory.Length == 0)
                {
                    resCategoryCash[i].SetActive(false);
                }
                else
                {
                    resCategoryCash[i].SetActive(true);

                    var s = economyCategory.Length;
                    for (int j = 0; j < s; j++)
                    {
                        styleConfig.GetIconSet(economyCategory[j], out var iconSet);
                        if (iconSet != null)
                        {
                            var el = resCategoryCash[i].GetChild(1, j);
                            el.SetActive(true);
                            el.CMP_Image().sprite = iconSet.activeIcon;

                            var _n = n;
                            n++;
                            var mes = economyCategory[j];
                            el.RegisterButtonClick(() => ServiceLocator.Current.Get<INotificationService>()
                                .Push(_n, LocationLootNotification.GetMessage(mes)));
                        }
                    }
                    
                    // размер каждого блока категории 
                    var size = resCategoryCash[i].CMP_RectTr().sizeDelta;
                    size.y = 100 * Mathf.Max(1, s / 5);
                    resCategoryCash[i].SetUISize(size);
                }
            }
            
            scrollResources.SetSizeContentLayoutGroup(true, null, true, true);
            scrollResources.ScrollRectResetV();
        }

        private int UpdateIntel(
            int index,
            string label,
            int active,
            IntelIconSet iconSet,
            IntelStyleConfig intelStyleConfig)
        {
            if (index >= intelBlockRoot.transform.childCount)
                return index;
            
            intelBlockRoot.SetActive(true);

            var el = intelBlockRoot.GetChild(index);
            el.gameObject.SetActive(true);

            // Name Text
            el.GetComponent<TMP_Text>().text = $"{label}";

            
            // Icons
            Sprite sprite = active == -1
                ? intelStyleConfig.noneIcon             // показываем прочерк
                : active == 0
                    ? intelStyleConfig.unknownIcon      // показываем вопрос
                    : iconSet.activeIcon;               // иконка ресурса
            
            var enable = active == -1 || active == 0;
            
            var l = el.transform.childCount;
            for (int i = 0; i < l; i++)
            {
                el.GetChild(i).SetActive(enable || i < active);
                el.GetChild(i).CMP_Image().sprite = sprite;
            }

            return index + 1;
        }


        void HideResourcesCategory()
        {
            var l = resCategoryCash.Length;
            for (int i = 0; i < l; i++)
            {
                var s = resCategoryCash[i].GetChild(1).transform.childCount;
                for (int j = 0; j < s; j++)
                    resCategoryCash[i].GetChild(1, j).SetActive(false);
            }
        }

    }
}