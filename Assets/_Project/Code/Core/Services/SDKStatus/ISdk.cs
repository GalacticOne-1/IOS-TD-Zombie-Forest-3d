using System;

namespace Galactic1
{
    public interface ISdk
    {
        ESdkType SdkType { get; }
        
        void SDKInitialize(Action onComplete);
        
        /// <summary>
        /// Вызывается по готовности сдк
        /// </summary>
        void SDKInitialized();
    }
}