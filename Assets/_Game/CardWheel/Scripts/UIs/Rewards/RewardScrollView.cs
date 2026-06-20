using System;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Random = UnityEngine.Random;

namespace Vertigo.CardWheel.UIs.Rewards
{
    public class RewardScrollView : FancyScrollRect<RewardItemData, RewardContext>
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private float      cellSize    = 100f;
        [SerializeField] private float      flyDuration = 0.6f;
        [SerializeField] private GameObject flyingIconPrefab;

        private readonly Dictionary<string, RewardEntry>    _entryLookup = new();
        private readonly Dictionary<string, RewardItemData> _rewardItems = new();

        protected override GameObject CellPrefab => cellPrefab;
        protected override float      CellSize   => cellSize;

        // private void Awake()
        // {
        //     if (flyingIconPrefab != null)
        //         LeanPool.Pre(flyingIconPrefab, 6);
        // }

        private void UpdateData(IList<RewardItemData> items)
        {
            UpdateContents(items);
            RebuildLookup();
        }

        private void RebuildLookup()
        {
            _entryLookup.Clear();
            RewardEntry lastEntry     = null;
            var         rewardEntries = GetComponentsInChildren<RewardEntry>(true);
            foreach (var entry in rewardEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;
                _entryLookup[entry.Id] = entry;
                lastEntry              = entry;
            }

            if (lastEntry != null) LayoutRebuilder.ForceRebuildLayoutImmediate(lastEntry.transform as RectTransform);
        }

        public void ClearRewards()
        {
            _rewardItems.Clear();
            UpdateData(new List<RewardItemData>());
        }

        public void AddOrUpdateReward(Sprite icon, int amount, string id, string label)
        {
            if (_rewardItems.TryGetValue(id, out var existing))
            {
                existing.Amount += amount;
            }
            else
            {
                _rewardItems.Add(id, new RewardItemData(id, icon, label, amount));
            }

            UpdateData(new List<RewardItemData>(_rewardItems.Values));
        }

        public bool TryGetRewardItem(string id, out RewardItemData item) => _rewardItems.TryGetValue(id, out item);

        public void PlayRewardAnimation(Vector3 sliceWorldPosition, Sprite sliceIcon, string rewardId, int addedAmount, Action onComplete)
        {
            var canvas       = GetComponentInParent<Canvas>();
            var canvasCamera = canvas.worldCamera;

            var startScreenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, sliceWorldPosition);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, startScreenPos, canvasCamera, out var localStart))
            {
                onComplete?.Invoke();
                return;
            }

            if (!_rewardItems.TryGetValue(rewardId, out var rewardItem))
            {
                onComplete?.Invoke();
                return;
            }

            rewardItem.Amount += addedAmount;

            if (!_entryLookup.TryGetValue(rewardId, out var targetEntry))
            {
                onComplete?.Invoke();
                return;
            }

            var targetIconRect = targetEntry.IconRectTransform;
            var iconCorners    = new Vector3[4];
            targetIconRect.GetWorldCorners(iconCorners);
            var iconCenterWorld = (iconCorners[0] + iconCorners[2]) * 0.5f;
            var targetScale     = targetIconRect.localScale;

            const int   iconCount      = 6;
            const float spreadRadius   = 40f;
            var         completedCount = 0;

            for (var i = 0; i < iconCount; i++)
            {
                var animationIcon = LeanPool.Spawn(flyingIconPrefab, transform);
                animationIcon.name = $"AnimationRewardIcon_{i}";

                var flyingImage = animationIcon.GetComponent<Image>();
                flyingImage.sprite         = sliceIcon;
                flyingImage.preserveAspect = true;

                var angleJitter  = Random.Range(-0.4f, 0.4f);
                var angle        = i / (float)iconCount * Mathf.PI * 2f + angleJitter;
                var radiusJitter = Random.Range(0.5f, 1.5f);
                var spreadOffset = new Vector2(Mathf.Cos(angle) * spreadRadius * radiusJitter, Mathf.Sin(angle) * spreadRadius * radiusJitter);
                animationIcon.transform.localPosition = localStart + spreadOffset;
                animationIcon.transform.localScale    = Vector3.zero;

                var randomScale = targetScale * Random.Range(0.6f, 1.3f);

                var seq = DOTween.Sequence();
                seq.Append(animationIcon.transform.DOScale(randomScale, flyDuration    * 0.3f).SetEase(Ease.OutBack));
                seq.Append(animationIcon.transform.DOMove(iconCenterWorld, flyDuration * 0.4f).SetEase(Ease.InBack));
                seq.Join(animationIcon.transform.DOScale(targetScale, flyDuration      * 0.4f).SetEase(Ease.InBack));
                seq.OnComplete
                    (
                     () =>
                     {
                         flyingImage.sprite                 = null;
                         animationIcon.transform.localScale = Vector3.zero;
                         LeanPool.Despawn(animationIcon);

                         completedCount++;
                         if (completedCount >= iconCount)
                             targetEntry.PlayAddAnimation(addedAmount, onComplete);
                     }
                    );
            }
        }
    }
}