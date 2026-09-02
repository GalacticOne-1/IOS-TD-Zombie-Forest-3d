using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using Galactic1.Game.UI.Garage.DTO;
using Galactic1.Game.UI.Stats;
using Galactic1.Game.UI.Stats.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Garage
{
    public class GarageDetailsView : MonoBehaviour
    {
        
        
        [Header("Main Info")]
        [SerializeField] private TMP_Text itemName;

        [Space]
        
        [SerializeField] private Transform cHeader;
        
        [Space]
        [SerializeField] private List<StaticDescriptorSlot> staticLayout;

        [SerializeField] private RectTransform statRoot;


        private GarageModulesPanelView modulesPanel;
        private StatViewFactory statFactory;
        private readonly List<(StatLayoutType, IPooledStatView<StatDtoBase>)> _spawned = new();
        private Dictionary<string, StaticDescriptorSlot> _staticLayoutMap;
        
        
        
        
        


        public void ShowDetails(GarageModuleDetailsDTO dto, GarageModulesPanelView modulesPanel)
        {
            ReleaseAllGroups();

            this.modulesPanel = modulesPanel;
            statFactory = ServiceLocator.Current.Get<StatViewFactory>();

            if (_staticLayoutMap == null)
                _staticLayoutMap = staticLayout.ToDictionary(x => x.slotId);
            
            // === basic description
            //itemIcon.material = ServiceLocator.Current.Get<UIStyleResolver>().ResolveRarityColor(dto.Rarity).Material;
            //itemIcon.sprite = dto.Icon;
            itemName.text = dto.Title;
            //outputCountText.text = $"{dto.OutputCount}";
            //craftTimeText.text = TimeUtils.FormatTime(dto.CraftTime);
            //cBlueprint.SetActive(false); // todo 
            
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
            StatOrderResolve.ReorderSpawned(_spawned, ItemCategory.Vehicle); // todo ItemCategory ??
            StatOrderResolve.InsertStructure(
                statFactory, 
                statRoot, 
                ItemCategory.Vehicle, // todo ItemCategory ??
                tuple =>  _spawned.Add((tuple.Item1, tuple.Item2)));
            
            // просто ставим на место
            cHeader.SetSiblingIndex(0);
            modulesPanel.StatScroll.SetSizeContentLayoutGroup(true, statRoot, true, true);
            modulesPanel.StatScroll.ScrollRectResetV();
            
            // Форсим layout
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(() =>
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(statRoot);
            });
        }

        public void Release()
        {
            ReleaseAllGroups();
        }
        
        private void ReleaseAllGroups()
        {
            foreach (var (type, view) in _spawned)
                statFactory.Release(type, view);

            _spawned.Clear();
        }
    }
}