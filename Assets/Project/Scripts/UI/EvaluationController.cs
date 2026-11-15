using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MestreDoPC
{
    public class EvaluationController : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text recordText;
        [SerializeField] private TMP_Text detailsText;
        [SerializeField] private ScrollRect detailsScroll;

        private static readonly Color Positive = new Color(0.47f, 0.90f, 0.55f);
        private static readonly Color Negative = new Color(1f, 0.43f, 0.43f);
        private static readonly Color Neutral = new Color(0.90f, 0.92f, 0.96f);

        private void Start()
        {
            var r = MissionSession.LastResult;
            if (r == null)
            {
                titleText.text = "Nenhum resultado disponível";
                scoreText.text = "";
                recordText.gameObject.SetActive(false);
                detailsText.text = "Jogue uma partida para ver o resultado aqui.";
                return;
            }

            int outcome;
            string details;

            if (r.detailLines.Count > 0)      // Manutenção e Contra a IA: texto vem pronto da sessão
            {
                titleText.text = r.headline;
                outcome = r.outcome;
                details = string.Join("\n", r.detailLines);
            }
            else                              // Montagem
            {
                titleText.text = r.bootable ? "PC montado com sucesso!"
                               : r.timedOut ? "O tempo acabou" : "O PC não ligou";
                outcome = r.bootable ? 1 : -1;
                details = BuildAssemblyDetails(r);
            }

            titleText.color = outcome > 0 ? Positive : outcome < 0 ? Negative : Neutral;
            recordText.gameObject.SetActive(r.isNewRecord);
            if (r.isNewRecord) recordText.text = "Novo recorde!";
            detailsText.text = details;

            StartCoroutine(CountUp(r.Total));
            StartCoroutine(ScrollToTop());
        }

        // A nota "sobe" até o valor final (usa tempo não escalado: funciona mesmo com a pausa ativa).
        private IEnumerator CountUp(int total)
        {
            const float duration = 0.8f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                scoreText.text = $"{Mathf.RoundToInt(total * (t / duration))} / {ScoreCalculator.MaxTotal}";
                yield return null;
            }
            scoreText.text = $"{total} / {ScoreCalculator.MaxTotal}";
        }

        // Espera o layout calcular a altura do texto antes de voltar a rolagem ao topo.
        private IEnumerator ScrollToTop()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (detailsScroll != null) detailsScroll.verticalNormalizedPosition = 1f;
        }

        // Rótulo à esquerda e valor alinhado a 80% da largura (tag <pos> do TextMeshPro).
        private static string Row(string label, string value) => $"{label}<pos=80%>{value}";

        private static string Row(string label, int value, int max) => Row(label, $"{value} / {max}");

        private static string BuildAssemblyDetails(EvaluationResult r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>{r.missionTitle}</b>");
            sb.AppendLine(Row("Tempo", Fmt(r.elapsedSeconds)));
            sb.AppendLine(Row("Orçamento gasto", $"R$ {r.spent:N0} de R$ {r.budget:N0}"));
            sb.AppendLine(Row("PC montado", $"R$ {r.buildPrice:N0}"));
            sb.AppendLine();

            sb.AppendLine("<b>Pontuação</b>");
            sb.AppendLine(Row("Compatibilidade", r.compatibilityScore, ScoreCalculator.MaxCompatibility));
            sb.AppendLine(Row("Requisitos do cliente", r.requirementsScore, ScoreCalculator.MaxRequirements));
            sb.AppendLine(Row("Uso do orçamento", r.budgetScore, ScoreCalculator.MaxBudget));
            sb.AppendLine(Row("Tempo", r.timeScore, ScoreCalculator.MaxTime));
            if (r.penalty > 0) sb.AppendLine(Row("Tentativas erradas", $"-{r.penalty}"));

            if (r.unmetRequirements.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Requisitos não atendidos</b>");
                foreach (var u in r.unmetRequirements) sb.AppendLine("• " + u);
            }
            if (r.issues.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Problemas encontrados</b>");
                foreach (var i in r.issues) sb.AppendLine("• " + i);
            }
            return sb.ToString();
        }

        private static string Fmt(float seconds) => $"{(int)seconds / 60:00}:{(int)seconds % 60:00}";

        // Ligar nos botões (OnClick, sem parâmetro).
        public void OnReplay() => Go(SceneNames.ForMode(
            GameManager.Instance != null ? GameManager.Instance.CurrentMode : GameMode.Assembly));

        public void OnMenu() => Go(SceneNames.MainMenu);

        private static void Go(string scene)
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(scene);
            else SceneManager.LoadScene(scene);
        }
    }
}
