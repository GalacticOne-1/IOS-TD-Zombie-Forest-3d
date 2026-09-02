using Galactic1;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour, ISceneActivator,  IUpdate
{
    public delegate void ParallaxCameraDelegate(float deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;

    private float oldPosition;

    
    
    
    public void Activator()
    {
        oldPosition = transform.position.x;
        ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
    }

    public void IUpdateClear(){}

    public void UpdateM()
    {
        if (transform.position.x != oldPosition)
        {
            if (onCameraTranslate != null)
            {
                float delta = oldPosition - transform.position.x;
                onCameraTranslate(delta);
            }

            oldPosition = transform.position.x;
        }
    }

    
}
