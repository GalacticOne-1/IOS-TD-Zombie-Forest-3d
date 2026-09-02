using System.Collections;
using UnityEngine;

namespace Gameplay
{





    public class UNIT_GFX_Flash
    {
        private MaterialPropertyBlock flashMat;
        private SpriteRenderer[] sprites;

        public UNIT_GFX_Flash(SpriteRenderer[] sprites)
        {
            this.sprites = sprites;
        }

        public IEnumerator Flash()
        {
            flashMat = new MaterialPropertyBlock();
            var l = sprites.Length;
            for (int i = 0; i < l; i++)
            {
                sprites[i].GetPropertyBlock(flashMat);
                flashMat.SetFloat("_FlashAmount", 1);
                sprites[i].SetPropertyBlock(flashMat);
            }
           

            for (float f = 1; f >= 0; f-=.13f)
            {
                for (int i = 0; i < l; i++)
                {
                    sprites[i].GetPropertyBlock(flashMat);
                    flashMat.SetFloat("_FlashAmount", f);
                    sprites[i].SetPropertyBlock(flashMat);
                }

                yield return null;
            }

            for (int i = 0; i < l; i++)
            {
                sprites[i].GetPropertyBlock(flashMat);
                flashMat.SetFloat("_FlashAmount", 0);
                sprites[i].SetPropertyBlock(flashMat);
            }
        }

        protected void FlashClear()
        {
            flashMat = new MaterialPropertyBlock();
            var l = sprites.Length;
            for (int i = 0; i < l; i++)
            {
                sprites[i].GetPropertyBlock(flashMat);
                flashMat.SetFloat("_FlashAmount", 0);
                sprites[i].SetPropertyBlock(flashMat);
            }
        }
    }
}