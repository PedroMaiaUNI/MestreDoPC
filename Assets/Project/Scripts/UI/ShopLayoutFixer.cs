using System.Text;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MestreDoPC
{
    public class ShopLayoutFixer : MonoBehaviour
    {
        [SerializeField] private RectTransform shopRoot;
        [SerializeField] private RectTransform shopWindow;
        [SerializeField] private RectTransform tabContainer;
        [SerializeField] private RectTransform btnShop;
        [SerializeField] private RectTransform budgetText;

        private static readonly Vector3[] corners = new Vector3[4];

        [ContextMenu("Diagnosticar posições")]
        private void Diagnose()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) { Debug.LogError("[Layout] Este objeto não está dentro de um Canvas."); return; }
            canvas = canvas.rootCanvas;
            var canvasRect = (RectTransform)canvas.transform;
            var scaler = canvas.GetComponent<CanvasScaler>();

            var sb = new StringBuilder("[Layout] Diagnóstico");
            sb.AppendLine();
            sb.AppendLine($"Canvas: {canvas.renderMode} | área {canvasRect.rect.width:0}x{canvasRect.rect.height:0} | Game view {Screen.width}x{Screen.height}");
            sb.AppendLine(scaler == null
                ? "CanvasScaler: AUSENTE"
                : $"CanvasScaler: {scaler.uiScaleMode} | ref {scaler.referenceResolution.x:0}x{scaler.referenceResolution.y:0} | match {scaler.matchWidthOrHeight}");

            // A janela precisa estar ativa para o layout ser calculado.
            bool reactivate = shopWindow != null && !shopWindow.gameObject.activeSelf;
            if (reactivate) shopWindow.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();

            Report(sb, canvasRect, "ShopRoot", shopRoot);
            Report(sb, canvasRect, "ShopWindow", shopWindow);
            Report(sb, canvasRect, "BtnShop", btnShop);
            Report(sb, canvasRect, "BudgetText", budgetText);

            if (tabContainer != null)
            {
                Report(sb, canvasRect, "TabContainer", tabContainer);
                sb.AppendLine($"  abas criadas: {tabContainer.childCount}" +
                              (tabContainer.rect.height < 1f ? "  <-- ALTURA 0: falta Layout Element (104) ou o Vertical Layout Group do ShopWindow" : ""));
            }

            if (reactivate) shopWindow.gameObject.SetActive(false);
            Debug.Log(sb.ToString(), this);
        }

        [ContextMenu("Aplicar layout padrão")]
        private void Apply()
        {
            //    objeto      âncora mín.        âncora máx.        pivô                 posição              tamanho
            Place(shopRoot,    new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(0, 0),   new Vector2(0, 0));    // tela toda
            Place(shopWindow,  new Vector2(1, 0), new Vector2(1, 1), new Vector2(1f, 0.5f),    new Vector2(0, 0),   new Vector2(720, 0));  // coluna direita, altura toda
            Place(btnShop,     new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),        new Vector2(30, 30), new Vector2(220, 64)); // canto inferior esquerdo
            Place(budgetText,  new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(900, 50)); // topo central
            Debug.Log("[Layout] Layout padrão aplicado. Rode 'Diagnosticar posições' para conferir.", this);
        }

        private static void Place(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            if (rt == null) return;
#if UNITY_EDITOR
            Undo.RecordObject(rt, "Aplicar layout da Loja");
#endif
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition3D = new Vector3(pos.x, pos.y, 0f);
#if UNITY_EDITOR
            EditorUtility.SetDirty(rt);
#endif
        }

        private static void Report(StringBuilder sb, RectTransform canvasRect, string label, RectTransform rt)
        {
            if (rt == null) { sb.AppendLine($"{label}: campo vazio no ShopLayoutFixer"); return; }

            rt.GetWorldCorners(corners);
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var c in corners)
            {
                Vector3 p = canvasRect.InverseTransformPoint(c);
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
            }

            // Coordenadas em unidades do Canvas, a partir do canto inferior esquerdo.
            var r = canvasRect.rect;
            float x0 = minX - r.xMin, x1 = maxX - r.xMin, y0 = minY - r.yMin, y1 = maxY - r.yMin;
            bool inside = x0 >= -1f && y0 >= -1f && x1 <= r.width + 1f && y1 <= r.height + 1f;

            sb.AppendLine($"{label}: x {x0:0}..{x1:0}  y {y0:0}..{y1:0}  ({x1 - x0:0}x{y1 - y0:0})  " +
                          (inside ? "OK, dentro da tela" : "<-- FORA DA TELA"));
        }
    }
}
