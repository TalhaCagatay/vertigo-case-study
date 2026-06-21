using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Vertigo.CardWheel.UIs.ZoneScroll
{
    class ZoneScrollView : FancyScrollView<ZoneItemData, ZoneContext>
    {
        [SerializeField] private Scroller   scroller;
        [SerializeField] private GameObject cellPrefab;

        protected override GameObject CellPrefab => cellPrefab;

        protected override void Initialize()
        {
            base.Initialize();

            scroller.Draggable    = false;
            scroller.MovementType = MovementType.Unrestricted;
            scroller.SnapEnabled  = false;
            scroller.Inertia      = false;
            scroller.OnValueChanged(UpdatePosition);
        }

        public void UpdateData(IList<ZoneItemData> items)
        {
            UpdateContents(items);
            scroller.SetTotalCount(items.Count);
        }
        
        public void SetSelectedIndex(int index)
        {
            Context.SelectedIndex = index;
            Refresh();
        }
        
        public void CenterOnIndex(int index, float duration = 0f)
        {
            if (scroller == null) return;

            if (duration <= 0f)
            {
                scroller.JumpTo(index);
            }
            else
            {
                scroller.ScrollTo(index, duration);
            }
        }
    }
}