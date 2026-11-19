using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace MestreDoPC
{
    public class WorkshopHUD : Singleton<WorkshopHUD>
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text issuesText;
        [SerializeField] private TMP_Text videoText;
        [SerializeField] private float messageDuration = 3f;

        private Coroutine messageRoutine;

        private void Start()
        {
            messageText.text = "";
            issuesText.text = "";
            if (videoText != null) videoText.text = "";
            BuildManager.Instance.OnBuildChanged += HandleBuildChanged;
        }

        private void OnDestroy()
        {
            if (BuildManager.Instance != null) BuildManager.Instance.OnBuildChanged -= HandleBuildChanged;
        }

        public void ShowMessage(string message)
        {
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(MessageRoutine(message));
        }

        private IEnumerator MessageRoutine(string message)
        {
            messageText.text = message;
            yield return new WaitForSeconds(messageDuration);
            messageText.text = "";
        }

        private void HandleBuildChanged(PcBuild build, CompatibilityReport report)
        {
            UpdateVideoText(build);
            if (!DifficultyRules.ShowLiveIssues) { issuesText.text = ""; return; }

            var sb = new StringBuilder();
            foreach (var i in report.Issues)
            {
                string color = i.Severity == IssueSeverity.Error ? "#FF5555" : "#FFD24D";
                sb.AppendLine($"<color={color}>• {i.Message}</color>");
            }

            var missing = CompatibilityChecker.MissingParts(build);
            if (missing.Length > 0)
                sb.AppendLine($"<color=#AAAAAA>Falta: {string.Join(", ", missing)}</color>");

            issuesText.text = sb.ToString();
        }

        private void UpdateVideoText(PcBuild build)
        {
            if (videoText == null) return;
            if (!DifficultyRules.ShowVideoStatus || build.cpu == null) { videoText.text = ""; return; }

            if (build.gpu != null)
            {
                videoText.text = build.cpu.hasIntegratedGraphics
                    ? $"Vídeo: <b>{build.gpu.DisplayName}</b>\n<size=75%><color=#AAAAAA>gráficos integrados desativados</color></size>"
                    : $"Vídeo: <b>{build.gpu.DisplayName}</b>";
            }
            else if (build.cpu.hasIntegratedGraphics)
            {
                videoText.text = $"Vídeo: <b>{build.VideoDescription}</b>\n<size=75%><color=#AAAAAA>sem placa de vídeo dedicada</color></size>";
            }
            else
            {
                videoText.text = "<color=#FF8888>Sem saída de vídeo</color>\n<size=75%><color=#AAAAAA>esta CPU não tem vídeo integrado</color></size>";
            }
        }
    }
}
