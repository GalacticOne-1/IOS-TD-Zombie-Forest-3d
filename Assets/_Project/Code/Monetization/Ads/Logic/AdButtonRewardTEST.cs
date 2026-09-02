using System.Collections;
using UnityEngine;

namespace Galactic1.Code.UI.Ads
{
    public class AdButtonRewardTEST : AdButtonPresenter
    {
        protected override void Start()
        {
            StartCoroutine(test());
        }


        IEnumerator test()
        {
            yield return new WaitForSeconds(2);
            Initialize();
        }
    }
}