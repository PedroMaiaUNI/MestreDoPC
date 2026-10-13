using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class TrayManager : Singleton<TrayManager>
    {
        [SerializeField] private Transform origin;
        [SerializeField] private float rowWidth = 3f;
        [SerializeField] private float rowSpacing = 0.9f;
        [SerializeField] private float gap = 0.1f;

        private readonly List<PartView> parts = new List<PartView>();
        private float cursorX;
        private int rowIndex;

        public PartView Spawn(ComponentDataSO data)
        {
            var view = PartFactory.Create(data, origin.position, origin.rotation);
            float w = WidthOf(view.gameObject);

            if (cursorX > 0f && cursorX + w > rowWidth)
            {
                cursorX = 0f;
                rowIndex++;
            }

            var pos = origin.position + new Vector3(-rowWidth * 0.5f + cursorX + w * 0.5f, 0f, -rowIndex * rowSpacing);
            view.SetHome(pos);

            cursorX += w + gap;
            parts.Add(view);
            return view;
        }

        private static float WidthOf(GameObject go)
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return 0.3f;
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b.size.x;
        }
    }
}
