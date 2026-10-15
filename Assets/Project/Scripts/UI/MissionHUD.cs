using TMPro;
using UnityEngine;

namespace MestreDoPC
{
    public class MissionHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text briefText;
        [SerializeField] private TMP_Text timerText;

        private bool briefShown;

        private void Update()
        {
            var s = MissionSession.Instance;
            if (s == null || s.Mission == null) return;

            if (!briefShown)
            {
                briefText.text = $"<b>{s.Mission.title}</b>\n{s.Mission.clientBrief}";
                briefShown = true;
            }

            bool hasLimit = s.Mission.timeLimitSeconds > 0;
            float t = hasLimit ? s.TimeRemaining : s.Elapsed;
            timerText.text = $"{(hasLimit ? "Restante" : "Tempo")}: {(int)t / 60:00}:{(int)t % 60:00}";
            timerText.color = hasLimit && t < 30f ? Color.red : Color.white;
        }
    }
}
