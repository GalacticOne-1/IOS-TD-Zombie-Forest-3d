using System;
using System.Collections;
using System.Collections.Generic;
using Galactic1.UI.Shop;
using UnityEngine;
using UnityEngine.Purchasing;
using Object = UnityEngine.Object;

namespace Galactic1.Systems.Purchase
{
    /// <summary>
    /// Единый сервис покупок.
    /// Управляет покупками через Unity IAP.
    /// Таймауты и restore выполняются полностью внутри сервиса.
    /// </summary>
    public sealed class PurchaseService :  IStoreListener
    {
        public readonly DIContainer _rootContainer;
        public bool IsInitialized => storeController != null;

        private IStoreController storeController;
        private IExtensionProvider extensions;
        
        private CoroutineRunner runner;

        private readonly Dictionary<string, IAPConfig> _iapConfigsMap = new();
        private readonly HashSet<string> processedTransactions = new();

        public event Action<string> _OnPurchaseSuccess;
        public event Action<string, PurchaseResult> _OnPurchaseFailed;

        
        
        private readonly Action onInitialized;
        public event Action OnRestoreStarted;
        public event Action OnRestoreCompleted;
        public event Action<PurchaseResult> OnRestoreFailed;

        

        #region Init

        public PurchaseService(
            DIContainer rootContainer, 
            Action onInitialized,
            Dictionary<string, IAPConfig> iapConfigsMap)
        {
            if (IsInitialized)
            {
                Debug.Log("PurchaseService already initialized");
                return;
            }
            
            _rootContainer = rootContainer;
            _iapConfigsMap = iapConfigsMap;
            this.onInitialized = onInitialized;
            
            // Создаем CoroutineRunner если его еще нет
            if (runner == null)
            {
                runner = Object.FindObjectOfType<CoroutineRunner>();
                if (runner == null)
                {
                    var go = new GameObject("PurchaseCoroutineRunner");
                    GameObject.DontDestroyOnLoad(go);
                    runner = go.AddComponent<CoroutineRunner>();
                }
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var iapConfig in _iapConfigsMap)
            {
                // * здесь могут быть карточки не для IAP, которые не должны попадать в builder
                
                // ! only for IAP configs !
                if(iapConfig.Value is IAPConfig config)
                {
                    builder.AddProduct(config.ProductId, config.ProductType);
                }
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            onInitialized();
            this.storeController = controller;
            this.extensions = extensions;
            
            DLog.Alert("**********✅✅✅ IAP Service initialized");
            
            // foreach (var iapConfig in _iapConfigsMap)
            // {
            //     // ! only for IAP configs !
            //     if (iapConfig.Value is IAPConfig config)
            //     {
            //         config.SetPrice(
            //             controller.products.WithID(config.ProductId).metadata.localizedPriceString,
            //             controller.products.WithID(config.ProductId).metadata.localizedPrice);
            //     }
            // }
            foreach (var (_, config) in _iapConfigsMap)
            {
                if (!config.UseIAP) 
                    continue;
                
                var product = controller.products.WithID(config.ProductId);

                if (product == null)
                {
                    Debug.LogError($"Product not found: {config.ProductId}");
                    continue;
                }

                Debug.Log($"✅{config.ProductId} -> {product.metadata.localizedPriceString}");

                config.SetPrice(
                    product.metadata.localizedPriceString,
                    product.metadata.localizedPrice,
                    product.metadata.isoCurrencyCode);
            }

            Debug.Log("PurchaseService initialized");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError("Purchase init failed: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"Purchase init failed: {error} {message}");
        }

        #endregion

        #region Buy

        private Coroutine purchaseTimeoutCoroutine;
        private const float PurchaseTimeoutSeconds = 10f;

        public void Buy(string productId)
        {
            if (!IsInitialized)
            {
                _OnPurchaseFailed?.Invoke(productId, PurchaseResult.NotInitialized);
                return;
            }

            var product = storeController.products.WithID(productId);

            if (product == null || !product.availableToPurchase)
            {
                _OnPurchaseFailed?.Invoke(productId, PurchaseResult.Failed);
                return;
            }

            // Таймаут покупки
            StopPurchaseTimeout();
            purchaseTimeoutCoroutine = runner.StartCoroutine(PurchaseTimeout(productId));

            storeController.InitiatePurchase(product);
        }

        private IEnumerator PurchaseTimeout(string productId)
        {
            yield return new WaitForSeconds(PurchaseTimeoutSeconds);

            purchaseTimeoutCoroutine = null;
            //Debug.LogError($"Purchase timeout for {productId}");
            _OnPurchaseFailed?.Invoke(productId, PurchaseResult.Timeout);
        }

        private void StopPurchaseTimeout()
        {
            if (purchaseTimeoutCoroutine != null)
            {
                runner.StopCoroutine(purchaseTimeoutCoroutine);
                purchaseTimeoutCoroutine = null;
            }
        }

        public void CompletePurchase()
        {
            StopPurchaseTimeout();
        }

        #endregion

        #region IStoreListener

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            CompletePurchase();

            var productId = e.purchasedProduct.definition.id;
            var transactionId = e.purchasedProduct.transactionID;

            if (!string.IsNullOrEmpty(transactionId))
            {
                if (processedTransactions.Contains(transactionId))
                    return PurchaseProcessingResult.Complete;

                processedTransactions.Add(transactionId);
            }

            _OnPurchaseSuccess?.Invoke(productId);
            DLog.Alert($"Purchase success: {productId}, transactionId: {transactionId}");

            storeController.ConfirmPendingPurchase(e.purchasedProduct);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            StopPurchaseTimeout();
            _OnPurchaseFailed?.Invoke(product.definition.id, PurchaseResult.Failed);
            DLog.Alert($"Purchase failed: {product.definition.id}, reason: {reason}");

        }

        #endregion

        #region Restore Purchases

        private Coroutine restoreTimeoutCoroutine;
        private const float RestoreTimeoutSeconds = 10f;

        public void RestorePurchases()
        {
            if (!IsInitialized)
            {
                OnRestoreFailed?.Invoke(PurchaseResult.NotInitialized);
                return;
            }

#if UNITY_IOS || UNITY_STANDALONE_OSX
            var apple = extensions.GetExtension<IAppleExtensions>();

            OnRestoreStarted?.Invoke();

            StopRestoreTimeout();
            restoreTimeoutCoroutine = runner.StartCoroutine(RestoreTimeout());

            apple.RestoreTransactions((result, message) =>
            {
                StopRestoreTimeout();

                if (result)
                    OnRestoreCompleted?.Invoke();
                else
                    OnRestoreFailed?.Invoke(PurchaseResult.Failed);
            });
#else
            // Android / Google Play: restore автоматически через ProcessPurchase
            OnRestoreStarted?.Invoke();
            OnRestoreCompleted?.Invoke();
#endif
        }

        private IEnumerator RestoreTimeout()
        {
            yield return new WaitForSeconds(RestoreTimeoutSeconds);

            restoreTimeoutCoroutine = null;
            Debug.LogWarning("RestorePurchases timeout");
            OnRestoreFailed?.Invoke(PurchaseResult.Failed);
        }

        private void StopRestoreTimeout()
        {
            if (restoreTimeoutCoroutine != null)
            {
                runner.StopCoroutine(restoreTimeoutCoroutine);
                restoreTimeoutCoroutine = null;
            }
        }

        #endregion

        #region Helpers

        public string GetLocalizedPrice(string productId)
        {
            if (!IsInitialized)
                return string.Empty;

            var product = storeController.products.WithID(productId);
            return product?.metadata.localizedPriceString ?? "N/A";
        }

        #endregion

    }
}
