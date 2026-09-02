using System.Collections.Generic;
using Galactic1;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    /*
     *      Контроллер, запускает блоки с бг
     *      блоки должны быть вложены (transform.GetChild)
     */
    
    
    private ParallaxCamera parallaxCamera;
    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    
    public void Init(ParallaxCamera scr)
    {
        parallaxCamera = scr;
        // if (parallaxCamera == null)
        //     parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();

        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;

        SetLayers();
    }

    void SetLayers()
    {
        parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).name = "block-" + i;
            var ll = transform.GetChild(i).childCount;
            for (int j = 0; j < ll; j++)
            {
                ParallaxLayer layer = gameObject.GetChild(i, j).GetComponent<ParallaxLayer>();

                if (layer != null)
                {
                    layer.name = "Layer-" + j;
                    parallaxLayers.Add(layer);
                }
            }
        }
    }

    void Move(float delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}