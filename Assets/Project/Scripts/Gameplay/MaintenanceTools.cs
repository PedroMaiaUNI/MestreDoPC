using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MestreDoPC
{
    public enum ToolMode { Move, Inspect, Clean }

    public class MaintenanceTools : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private DragController dragController;
        [SerializeField] private Button moveButton;
        [SerializeField] private Button inspectButton;
        [SerializeField] private Button cleanButton;

        private static readonly Color SelectedColor = new Color(0.35f, 0.75f, 1f);

        public ToolMode Mode { get; private set; } = ToolMode.Move;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Start() => Apply();

        public void SetTool(int mode)
        {
            Mode = (ToolMode)mode;
            Apply();
        }

        private void Apply()
        {
            if (dragController != null) dragController.enabled = Mode == ToolMode.Move;
            Tint(moveButton, Mode == ToolMode.Move);
            Tint(inspectButton, Mode == ToolMode.Inspect);
            Tint(cleanButton, Mode == ToolMode.Clean);
        }

        private static void Tint(Button b, bool selected)
        {
            if (b != null) b.image.color = selected ? SelectedColor : Color.white;
        }

        private void Update()
        {
            if (Mode == ToolMode.Move || MaintenanceSession.Instance == null) return;
            if (Time.timeScale == 0f) return;
            if (!Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

            var part = hit.collider.GetComponentInParent<PartView>();
            if (part == null) return;

            if (Mode == ToolMode.Inspect) MaintenanceSession.Instance.Inspect(part);
            else MaintenanceSession.Instance.Clean(part);
        }
    }
}
