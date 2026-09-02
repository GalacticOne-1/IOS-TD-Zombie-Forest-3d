using TMPro;
using UnityEngine;

namespace Galactic1
{
    public class LocationLoadingScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentLocationText;

        
        public struct LocationLoadDTO
        {
            public string locationName;
        }

        public void Entry(LocationLoadDTO dto)
        {
            currentLocationText.text = dto.locationName;
            gameObject.SetActive(true);
        }
    }
}