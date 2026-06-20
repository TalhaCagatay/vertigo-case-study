using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Vertigo.CardWheel.UIs.Rewards
{
    public class RewardScrollView : FancyScrollRect<RewardItemData, RewardContext>
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField]         float      cellSize    = 100f;
        [SerializeField] private float      flyDuration = 0.6f;

        private readonly Dictionary<string, RewardEntry>    _entryLookup = new();
        private readonly Dictionary<string, RewardItemData> _rewardItems = new();

        protected override GameObject CellPrefab => cellPrefab;
        protected override float      CellSize   => cellSize;

        private void UpdateData(IList<RewardItemData> items)
        {
            UpdateContents(items);
            RebuildLookup();
        }

        private void RebuildLookup()
        {
            _entryLookup.Clear();
            var rewardEntries = GetComponentsInChildren<RewardEntry>(true);
            foreach (var entry in rewardEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;
                _entryLookup[entry.Id] = entry;
            }
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
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public bool TryGetRewardItem(string id, out RewardItemData item) => _rewardItems.TryGetValue(id, out item);

        public void PlayRewardAnimation(Vector3 sliceWorldPosition, Sprite sliceIcon, string rewardId, int addedAmount, Action onComplete)
        {
            var animationIcon = new GameObject("AnimationRewardIcon", typeof(Image)); // no need for an object pooling for this basic demo
            animationIcon.transform.SetParent(transform, false);

            var flyingImage = animationIcon.GetComponent<Image>();
            flyingImage.sprite = sliceIcon;
            flyingImage.SetNativeSize();
            flyingImage.transform.localScale *= 0.5f;

            var canvas       = GetComponentInParent<Canvas>();
            var canvasCamera = canvas.worldCamera;

            var startScreenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, sliceWorldPosition);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, startScreenPos, canvasCamera, out var localStart))
                animationIcon.transform.localPosition = localStart;

            if (!_rewardItems.TryGetValue(rewardId, out var rewardItem))
            {
                Destroy(animationIcon);
                onComplete?.Invoke();
                return;
            }

            rewardItem.Amount += addedAmount;

            if (!_entryLookup.TryGetValue(rewardId, out var targetEntry))
            {
                Destroy(animationIcon);
                onComplete?.Invoke();
                return;
            }

            var targetIconRect = targetEntry.IconRectTransform;
            var iconCorners    = new Vector3[4];
            targetIconRect.GetWorldCorners(iconCorners);
            var iconCenterWorld = (iconCorners[0] + iconCorners[2]) * 0.5f;

            var seq = DOTween.Sequence();
            seq.Append(animationIcon.transform.DOMove(iconCenterWorld, flyDuration).SetEase(Ease.InBack));
            seq.Join(animationIcon.transform.DOScale(Vector3.one * 0.5f, flyDuration).SetEase(Ease.InBack));
            seq.OnComplete
                (
                 () =>
                 {
                     Destroy(animationIcon);
                     targetEntry.PlayAddAnimation(addedAmount, onComplete);
                 }
                );
        }
    }
}