using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MestreDoPC
{
    public class SellZone : MonoBehaviour
    {
        public static SellZone Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float refundPercent = 1f;   // 1 = devolve 100%
        [SerializeField] private Vector2 panelSize = new Vector2(560f, 120f);
        [SerializeField] private float bottomOffset = 150f;                  // distância da borda de baixo

        private static readonly Color Idle = new Color(0.55f, 0.15f, 0.15f, 0.90f);
        private static readonly Color Hover = new Color(0.90f, 0.22f, 0.22f, 0.97f);

        private GameObject canvasObject;
        private RectTransform panelRect;
        private Image background;
        private TMP_Text label;
        private PartView dragging;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);   // só o componente duplicado; o objeto continua existindo
                return;
            }
            Instance = this;
            BuildUI();
            Debug.Log("[Devolver] Zona criada. Pegue uma peça comprada para vê-la.");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (canvasObject != null) Destroy(canvasObject);
        }

        private void BuildUI()
        {
            // Canvas próprio, por cima de toda a HUD. Sem GraphicRaycaster: nunca bloqueia cliques.
            canvasObject = new GameObject("SellZoneCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Painel: base centralizada na borda de baixo, "bottomOffset" acima dela.
            var panelObject = new GameObject("SellPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
            panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = new Vector2(0f, bottomOffset);

            background = panelObject.GetComponent<Image>();
            background.color = Idle;
            background.raycastTarget = false;

            // Texto: ocupa o painel inteiro, com 12 de margem.
            var labelObject = new GameObject("SellLabel", typeof(RectTransform));
            labelObject.transform.SetParent(panelObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12f, 12f);
            labelRect.offsetMax = new Vector2(-12f, -12f);

            var text = labelObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = 28f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            label = text;

            panelObject.SetActive(false);
        }

        // Chamado pelo DragController quando o jogador pega uma peça.
        public void BeginDrag(PartView part)
        {
            dragging = part.Sellable ? part : null;
            Debug.Log($"[Devolver] Pegou {part.Data.DisplayName}: PurchasePrice={part.PurchasePrice}, IsBundled={part.IsBundled}, Sellable={part.Sellable}");

            panelRect.gameObject.SetActive(dragging != null);
            if (dragging != null)
                label.text = $"Solte aqui para devolver\n{part.Data.DisplayName}: reembolso de R$ {RefundFor(part):N0}";
        }

        public void EndDrag()
        {
            dragging = null;
            if (panelRect != null) panelRect.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (dragging != null) background.color = Contains(Input.mousePosition) ? Hover : Idle;
        }

        // Canvas em Screen Space - Overlay: a câmera do teste é null.
        public bool Contains(Vector2 screenPoint)
            => panelRect != null && panelRect.gameObject.activeSelf &&
               RectTransformUtility.RectangleContainsScreenPoint(panelRect, screenPoint, null);

        public int RefundFor(PartView part) => Mathf.RoundToInt(part.PurchasePrice * refundPercent);

        // Devolve a peça (e o brinde que veio com ela). Retorna false se a peça não puder ser devolvida.
        public bool TrySell(PartView part)
        {
            if (!part.Sellable)
            {
                Say("Essa peça não pode ser devolvida.");
                return false;
            }

            string name = part.Data.DisplayName;
            int refund = RefundFor(part);
            BudgetManager.Instance.Refund(refund);

            if (part.BundledChild != null) Discard(part.BundledChild);
            Discard(part);

            Say($"{name} devolvida: +R$ {refund:N0}.");
            return true;
        }

        private static void Discard(PartView part)
        {
            if (part.CurrentSlot != null) BuildManager.Instance.Remove(part);   // tira do PC, se estiver instalada
            if (TrayManager.Instance != null) TrayManager.Instance.Forget(part);
            Destroy(part.gameObject);
        }

        private static void Say(string message)
        {
            if (WorkshopHUD.Instance != null) WorkshopHUD.Instance.ShowMessage(message);
        }

        // Teste de posição: em Play, ⋮ do componente > este item mostra o painel sem precisar pegar peça.
        [ContextMenu("Debug: mostrar a zona agora")]
        private void DebugShow()
        {
            if (panelRect == null) { Debug.LogWarning("[Devolver] Dê Play antes."); return; }
            label.text = "Zona de teste (posição OK)";
            panelRect.gameObject.SetActive(true);
        }
    }
}
