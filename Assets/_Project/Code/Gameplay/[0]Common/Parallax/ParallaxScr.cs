using Galactic1;
using UnityEngine;

namespace Galactic1
{

    [System.Serializable]
    public struct CBgSetup
    {
        public GameObject prefabBg;
        public byte amount;
        public Vector2 start;
    }
    
    
    
    
    public class ParallaxInit
    {
        public ParallaxInit(CBgSetup setup)
        {
            CameraFollow.I.gameObject.GetComponent<ParallaxCamera>().Activator();
            new ParallaxNewBG(setup);
        }
    }

    public class ParallaxClear
    {
        public ParallaxClear()
        {
            CameraFollow.I.gameObject.GetComponent<ParallaxCamera>().onCameraTranslate = null;
            ServiceLocator.Current.Get<Environment>().levelBG.transform.MakeEmpty();
        }
    }
    
    class ParallaxNewBG
    {
        public ParallaxNewBG(CBgSetup setup)
        {
            // создаем блоки на нужную длинну сцены
            GameObject g;
            for (int i = 0; i < setup.amount; i++)
            {
                g = setup.prefabBg.CreateGO(ServiceLocator.Current.Get<Environment>().levelBG.transform);
                g.transform.localPosition = setup.start;
                setup.start.x += 30;
            }
            
            // активация
            ServiceLocator.Current.Get<Environment>().levelBG.GetComponent<ParallaxBackground>()
                .Init(CameraFollow.I.gameObject.GetComponent<ParallaxCamera>());
        }
    }
}