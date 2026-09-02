using System;
using UnityEngine;
using Galactic1_DeadHero_Environment = Galactic1.Environment;

namespace Galactic1.Test
{
    public class InteractionPolygon : Singleton<InteractionPolygon>
    {
        [SerializeField] private bool work;

        [Header("Создаст один тип из objects[0]")]
        [SerializeField] private bool oneType;
        [SerializeField] private byte qu;
        
        [Space]
        [SerializeField] private Vector2 startCoord;
        [SerializeField] private GameObject[] objects;
        
        
        

        public void LoadPolygon()
        {
            if (work)
            {
                Vector2 coord = startCoord;
                var l = oneType ? qu : objects.Length;
                for (int i = 0; i < l; i++)
                {
                    var g = objects[oneType ? 0 : i].CreateGO(ServiceLocator.Current.Get<Galactic1_DeadHero_Environment>().playerObj);
                    g.transform.position = coord;
                    coord.x += 2;

                    // if (g.GetComponent<_Tool_>())
                    // {
                    //     g.GetComponent<_Tool_>().SetActivated();
                    // }
                }
            }
        }
    }
}