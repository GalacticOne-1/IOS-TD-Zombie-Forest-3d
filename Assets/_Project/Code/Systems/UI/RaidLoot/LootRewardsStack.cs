using System;
using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.UI.Common;
using Galactic1.RaidLoot.Runtime;
using UnityEngine;

namespace Galactic1.Code.UI.RaidLoot
{
    /// <summary>
    /// One stack of reward cards.
    ///
    /// Lives above a single container.
    /// Returned into pool after completion.
    /// </summary>
    public sealed class LootRewardsStack : MonoBehaviour
    {
        [SerializeField]
        private WorldUIFollow _follow;
        
        [Header("Cards")] 
        [SerializeField] private Transform cardRoot;

        [Header("Animation")] 
        [SerializeField] private float _spawnDelay = 0.1f;
        [SerializeField] private float _lifeTime = 2.5f;

        
        private LootRewardCard[] _cards;
        private Action<LootRewardsStack> _returnToPool;

        private Coroutine _routine;
        
        

        // ------------------------------------------------

        public void Setup(Action<LootRewardsStack> returnToPool)
        {
            _returnToPool = returnToPool;

            var l = cardRoot.childCount;
            _cards = new LootRewardCard[l];
            for (int i = 0; i < l; i++)
                _cards[i] = cardRoot.GetChild(i).GetComponent<LootRewardCard>();
        }

        // ------------------------------------------------

        public void Show(IReadOnlyList<LootGenerationRecord> loot)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(ShowRoutine(loot));
        }
        
        public void Attach(Vector3 target, Camera cam)
        {
            _follow.Attach(target, cam);
        }

        // ------------------------------------------------
        
        private IEnumerator ShowRoutine(IReadOnlyList<LootGenerationRecord> loot)
        {
            HideAllCards();

            int count = Mathf.Min(loot.Count, _cards.Length);

            for (int i = 0; i < count; i++)
            {
                var card = _cards[i];

                card.gameObject.SetActive(true);

                card.Show(loot[i].Item, loot[i].Amount);

                yield return new WaitForSeconds(_spawnDelay);
            }

            yield return new WaitForSeconds(_lifeTime);

            yield return HideCardsRoutine(count);

            _returnToPool?.Invoke(this);
        }

        private IEnumerator HideCardsRoutine(int count)
        {
            int completed = 0;

            for (int i = 0; i < count; i++)
            {
                _cards[i].Hide(() =>
                {
                    completed++;
                });
            }

            yield return new WaitUntil(() =>
                completed >= count);
        }
        
        // ------------------------------------------------

        private void HideAllCards()
        {
            foreach (var card in _cards)
            {
                if (card == null)
                    continue;

                card.gameObject.SetActive(false);
            }
        }
        
    }
}