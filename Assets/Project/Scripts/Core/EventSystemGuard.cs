using UnityEngine;
using UnityEngine.EventSystems;

namespace MestreDoPC
{
    public class EventSystemGuard : MonoBehaviour
    {
        private void Awake()
        {
            if (FindObjectOfType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem (auto)");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
