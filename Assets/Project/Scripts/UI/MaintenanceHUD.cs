using TMPro;
using UnityEngine;

namespace MestreDoPC
{
    public class MaintenanceHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text briefText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text plugLabel;

        private bool briefShown;

        private void Update()
        {
            var s = MaintenanceSession.Instance;
            if (s == null || s.Case == null) return;

            if (!briefShown)
            {
                briefText.text = $"<b>{s.Case.title}</b>\n{s.Case.clientComplaint}";
                briefShown = true;
            }

            float t = s.TimeRemaining;
            timerText.text = $"Restante: {(int)t / 60:00}:{(int)t % 60:00}";
            timerText.color = t < 30f ? Color.red : Color.white;

            plugLabel.text = s.Plugged ? "Tomada: ligada" : "Tomada: desconectada";
        }
    }
}
