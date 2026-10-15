using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MestreDoPC
{
    public class EvaluationController : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text detailsText;

        private void Start()
        {
            var r = MissionSession.LastResult;
            if (r == null)
            {
                titleText.text = "Nenhum resultado disponível";
                return;
            }

            if (r.detailLines.Count > 0)   
            {
                titleText.text = r.headline;
                scoreText.text = $"{r.Total} / {ScoreCalculator.MaxTotal}" + (r.isNewRecord ? "   Novo recorde!" : "");
                detailsText.text = string.Join("\n", r.detailLines);
                return;
            }

            titleText.text = r.bootable ? "PC montado com sucesso!"
                           : r.timedOut ? "O tempo acabou" : "O PC não ligou";
            scoreText.text = $"{r.Total} / {ScoreCalculator.MaxTotal}" + (r.isNewRecord ? "   Novo recorde!" : "");

            var sb = new StringBuilder();
            sb.AppendLine($"Missão: {r.missionTitle}");
            sb.AppendLine($"Tempo: {(int)r.elapsedSeconds / 60:00}:{(int)r.elapsedSeconds % 60:00}");
            sb.AppendLine($"Orçamento: R$ {r.spent:N0} gasto de R$ {r.budget:N0} (PC montado: R$ {r.buildPrice:N0})");
            sb.AppendLine();
            sb.AppendLine($"Compatibilidade: {r.compatibilityScore} / {ScoreCalculator.MaxCompatibility}");
            sb.AppendLine($"Requisitos: {r.requirementsScore} / {ScoreCalculator.MaxRequirements}");
            sb.AppendLine($"Orçamento: {r.budgetScore} / {ScoreCalculator.MaxBudget}");
            sb.AppendLine($"Tempo: {r.timeScore} / {ScoreCalculator.MaxTime}");
            if (r.penalty > 0) sb.AppendLine($"Penalidade por tentativas erradas: -{r.penalty}");

            if (r.unmetRequirements.Count > 0)
            {
                sb.AppendLine().AppendLine("Requisitos não atendidos:");
                foreach (var u in r.unmetRequirements) sb.AppendLine("• " + u);
            }
            if (r.issues.Count > 0)
            {
                sb.AppendLine().AppendLine("Problemas encontrados:");
                foreach (var i in r.issues) sb.AppendLine("• " + i);
            }
            detailsText.text = sb.ToString();
        }

        public void OnReplay() => Go(SceneNames.ForMode(
            GameManager.Instance != null ? GameManager.Instance.CurrentMode : GameMode.Assembly)
        );
        public void OnMenu() => Go(SceneNames.MainMenu);

        private static void Go(string scene)
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(scene);
            else SceneManager.LoadScene(scene);
        }
    }
}
