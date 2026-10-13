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
        [SerializeField] private float messageDuration = 3f;

        private Coroutine messageRoutine;

        private void Start()
        {
            messageText.text = "";
            issuesText.text = "";
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
    }
}
