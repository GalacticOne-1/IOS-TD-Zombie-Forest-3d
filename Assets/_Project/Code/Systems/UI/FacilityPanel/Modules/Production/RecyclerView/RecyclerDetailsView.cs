using System.Collections.Generic;
using Galactic1.Game.UI.Production.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// View для Recycler рецептов.
    /// Отображает несколько output ресурсов.
    /// </summary>
    public sealed class RecyclerDetailsView : MonoBehaviour
    {
        [Header("Shared")]
        [SerializeField] private RecipeDetailsView sharedBase;
        
        [Header("Outputs")]
        [SerializeField] private Transform outputsRoot;
        [SerializeField] private RecyclerOutputSlotView outputPrefab;

        private readonly List<RecyclerOutputSlotView> _outputs = new();
        public RecipeDetailsView SharedBase => sharedBase;

        

        public void ShowDetails(RecipeDetailsDto dto)
        {
            sharedBase.ShowDetails(dto);
            BuildOutputs(dto.OutputResources);
        }

        private void BuildOutputs(IReadOnlyList<RecyclerOutputDTO> outputs)
        {
            // foreach (var o in _outputs)
            //     Destroy(o.gameObject);
            //
            // _outputs.Clear();
            //
            // if (outputs == null)
            //     return;
            //
            // foreach (var resource in outputs)
            // {
            //     var view = Instantiate(outputPrefab, outputsRoot);
            //     view.Bind(resource.Icon, resource.Amount);
            //     _outputs.Add(view);
            // }
        }
    }
}