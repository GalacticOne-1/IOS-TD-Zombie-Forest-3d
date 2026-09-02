using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.Utility;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Production.DTO;
using Galactic1.Game.UI.Stats;
using Galactic1.Game.UI.Stats.DTO;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Production
{
    public class CraftDetailsView : MonoBehaviour
    {
        [Header("Shared")]
        [SerializeField] private RecipeDetailsView sharedBase;
        
        [Header("Main Info")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text outputCountText;
        [SerializeField] private TMP_Text craftTimeText;
        [SerializeField] private GameObject cBlueprint;

        [Space]
        
        [SerializeField] private Transform cHeader;
        [SerializeField] private CraftDetailsStorageView storage;
        
        [Space]
        [SerializeField] private List<StaticDescriptorSlot> staticLayout;

        [SerializeField] private RectTransform statRoot;
        
        
        
        private StatViewFactory statFactory;
        private readonly List<(StatLayoutType, IPooledStatView<StatDtoBase>)> _spawned = new();
        private Dictionary<string, StaticDescriptorSlot> _staticLayoutMap;
        
        public RecipeDetailsView SharedBase => sharedBase;
        public RectTransform StatRoot => statRoot;
        
        
        
        


        public void ShowDetails(RecipeDetailsDto dto)
        {
            ReleaseAllGroups();

            var glc = ServiceLocator.Current.Get<GameSession>().GameLoopContext;
            statFactory = ServiceLocator.Current.Get<StatViewFactory>();
            sharedBase.ShowDetails(dto);

            if (_staticLayoutMap == null)
                _staticLayoutMap = staticLayout.ToDictionary(x => x.slotId);
            
            // === basic description
            itemIcon.material = ServiceLocator.Current.Get<UIStyleResolver>().ResolveRarityColor(dto.Rarity).Material;
            itemIcon.sprite = dto.Icon;
            itemName.text = dto.Title;
            outputCountText.text = $"{dto.OutputCount}";
            craftTimeText.text = TimeUtils.FormatTime(dto.CraftTime);
            cBlueprint.SetActive(false); // todo 
            
            
            // === storage
            storage.CampText.text = glc.CampRuntime.GetInventory(StorageType.Regular)
                .GetTotalAmount(dto.RecipeId).ToString();
            storage.TransportText.text = glc.PlayerTransport.GetInventory
                .GetTotalAmount(dto.RecipeId).ToString();
            storage.InboxText.text = ServiceLocator.Current.Get<InboxService>()
                .GetTotalAmount(dto.RecipeId).ToString();
                
                
            
            // === special description
            List<string> used = new();
            foreach (var d in dto.DescriptorDto)
            {
                if (d is DescriptorViewDto descriptor)
                {
                    if (descriptor.LayoutType == StatLayoutType.StaticLabel)
                    {
                        var slot = _staticLayoutMap[descriptor.DescriptorEntry.staticLayoutId];
                        slot.Set(descriptor.Label, descriptor.Value);
                        used.Add(descriptor.DescriptorEntry.staticLayoutId);
                    }
                    else // для описаний в dynamic stats
                    {
                        var view = statFactory.Get(descriptor.LayoutType, statRoot);
                        view.Bind(descriptor);
                        //view.RectTransform.SetSiblingIndex(statRoot.childCount-1);
                        _spawned.Add((descriptor.LayoutType, view));
                    }
                }
            }

            // отключем не сипользуемые поля
            foreach (var s in _staticLayoutMap)
            {
                if (!used.Contains(s.Key))
                {
                    s.Value.SetActive(false);
                }
            }
            
            
            // === dynamic stats
            foreach (var s in dto.StatGroups[0].Stats)
            {
                var view = statFactory.Get(s.LayoutType, statRoot);
                view.Bind(s);
                //view.RectTransform.SetSiblingIndex(statRoot.childCount-1);
                _spawned.Add((s.LayoutType, view));
            }
            
            // *** устанавливаем правильный порядок
            StatOrderResolve.ReorderSpawned(_spawned, dto.ItemCategory);
            StatOrderResolve.InsertStructure(
                statFactory, 
                statRoot, 
                dto.ItemCategory, 
                tuple =>  _spawned.Add((tuple.Item1, tuple.Item2)));
            
            // просто ставим на место
            cHeader.SetSiblingIndex(0);
            storage.transform.SetSiblingIndex(1);
            sharedBase.StatScroll.SetSizeContentLayoutGroup(true, statRoot, true, true);
            sharedBase.StatScroll.ScrollRectResetV();
            
            // Форсим layout
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(statRoot);
        }

        public void Release()
        {
            ReleaseAllGroups();
        }
        
        private void ReleaseAllGroups()
        {
            // foreach (var g in _activeGroups)
            //     groupPool.Release(g);
            //
            // _activeGroups.Clear();
            
            foreach (var (type, view) in _spawned)
                statFactory.Release(type, view);

            _spawned.Clear();
        }
    }
}