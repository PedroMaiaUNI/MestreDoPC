using UnityEngine;
using UnityEngine.EventSystems;

namespace MestreDoPC
{
    public class DragController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float liftHeight = 0.6f;     // altura da peça enquanto é arrastada
        [SerializeField] private float snapDistance = 0.45f;  // raio de encaixe (plano XZ)

        private PartView dragged;
        private Plane dragPlane;
        private Vector3 grabOffset;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
            dragPlane = new Plane(Vector3.up, new Vector3(0f, liftHeight, 0f));
        }

        private void Update()
        {
            if (dragged == null)
            {
                if (Input.GetMouseButtonDown(0)) TryPick();
            }
            else if (Input.GetMouseButtonUp(0)) Drop();
            else if (Input.GetMouseButton(0)) Drag();
        }

        private void TryPick()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // clique foi na UI

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

            var part = hit.collider.GetComponentInParent<PartView>();
            if (part == null) return;

            if (part.CurrentSlot != null) BuildManager.Instance.Remove(part);

            dragged = part;
            grabOffset = Vector3.zero;
            if (dragPlane.Raycast(ray, out float e))
            {
                grabOffset = part.transform.position - ray.GetPoint(e); // evita a peça "pular" para o cursor
                grabOffset.y = 0f;
            }

            foreach (var s in PartSlot.All)
                s.SetHighlight(s.category == part.Data.Category);
        }

        private void Drag()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!dragPlane.Raycast(ray, out float enter)) return;

            Vector3 p = ray.GetPoint(enter) + grabOffset;
            p.y = liftHeight;
            dragged.transform.position = p;
        }

        private void Drop()
        {
            foreach (var s in PartSlot.All) s.SetHighlight(false);

            PartSlot target = FindBestSlot(dragged);
            if (target != null && BuildManager.Instance.TryInstall(target, dragged, out string msg))
            {
                Say(msg);
            }
            else
            {
                if (target != null)
                {
                    BuildManager.Instance.CanInstall(target, dragged, out string reason);
                    Say(reason);
                }
                else Say("Peça devolvida à bandeja.");

                dragged.SendHome();
            }
            dragged = null;
        }

        private PartSlot FindBestSlot(PartView part)
        {
            PartSlot bestMatch = null, bestAny = null;
            float dMatch = float.MaxValue, dAny = float.MaxValue;
            Vector3 p = part.transform.position;

            foreach (var s in PartSlot.All)
            {
                var sp = s.snapPoint.position;
                float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(sp.x, sp.z));
                if (d > snapDistance) continue;

                if (s.category == part.Data.Category && s.Installed == null && d < dMatch) { bestMatch = s; dMatch = d; }
                if (d < dAny) { bestAny = s; dAny = d; }
            }
            return bestMatch != null ? bestMatch : bestAny;
        }

        private static void Say(string message)
        {
            if (WorkshopHUD.Instance != null) WorkshopHUD.Instance.ShowMessage(message);
        }
    }
}
