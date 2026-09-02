using Galactic1.Mobile;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class BtnMobile3_delegate : BtnMobile3
    {


        public DFunc func;

        public override void OnClick()
        {
            func?.Invoke();
        }
    }
}