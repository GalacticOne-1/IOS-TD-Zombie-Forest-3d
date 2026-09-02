
using System;
using UnityEngine;

namespace Galactic1.UI.CharacterPreview
{
    /// <summary>
    /// Владеет RenderTexture портрета.
    /// Dispose освобождает текстуру.
    /// Вешается на карточку — при закрытии карточки вызывается Dispose.
    /// </summary>
    public sealed class CharacterPortraitHandle : IDisposable
    {
        public RenderTexture Texture { get; private set; }
        private bool disposed;

        public CharacterPortraitHandle(RenderTexture texture)
        {
            Texture = texture;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            if (Texture != null)
            {
                Texture.Release();
                Texture = null;
            }
        }
    }
}