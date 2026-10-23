using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MestreDoPC
{
    public class AIOpponentManager : Singleton<AIOpponentManager>
    {
        [SerializeField] private ComponentDatabaseSO database;
        [SerializeField] private List<AIProfileSO> profiles = new List<AIProfileSO>();

        [Header("Painel da IA")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text stepsText;
        [SerializeField] private Slider progressBar;

        private MissionDataSO mission;
        private AIProfileSO profile;
        private PcBuild aiBuild;
        private float aiTime;
        private List<string> steps = new List<string>();
        private bool ready, failed;

        private void OnEnable() => MissionSession.OnResultReady += HandlePlayerResult;
        private void OnDisable() => MissionSession.OnResultReady -= HandlePlayerResult;

        private void Update()
        {
            var session = MissionSession.Instance;
            if (session == null || session.Mission == null || failed) return;

            if (!ready)
            {
                Initialize(session.Mission);
                if (!ready) return;
            }
            UpdatePanel(session.Elapsed);
        }

        private void Initialize(MissionDataSO m)
        {
            mission = m;

            var gm = GameManager.Instance;
            Difficulty diff = gm != null ? gm.CurrentDifficulty : m.difficulty;
            profile = profiles.FirstOrDefault(p => p.difficulty == diff) ?? profiles.FirstOrDefault();
            if (profile == null) { Fail("Nenhum AIProfile cadastrado no AIOpponentManager."); return; }

            aiTime = Random.Range(profile.timeRange.x, profile.timeRange.y);
            if (m.timeLimitSeconds > 0f) aiTime = Mathf.Min(aiTime, m.timeLimitSeconds * 0.95f);

            aiBuild = AIBuilder.Build(database, m, profile, aiTime);
            if (aiBuild == null)
            {
                Fail("A IA não achou montagem válida. Confira orçamento e requisitos da missão e o banco de peças.");
                return;
            }

            steps = AIBuilder.Describe(aiBuild);
            titleText.text = profile.displayName;
            ready = true;
            Debug.Log($"[IA] {profile.displayName}: termina em {aiTime:0}s com R$ {aiBuild.TotalPrice:N0}.");
        }

        private void UpdatePanel(float t)
        {
            float progress = Mathf.Clamp01(t / aiTime);
            int revealed = Mathf.Min(steps.Count, Mathf.FloorToInt(steps.Count * progress));

            var sb = new StringBuilder();
            for (int i = 0; i < revealed; i++) sb.AppendLine("[ok] " + steps[i]);
            if (revealed < steps.Count) sb.AppendLine("... escolhendo");
            stepsText.text = sb.ToString();

            progressBar.value = progress;
            statusText.text = progress >= 1f ? $"Terminou em {Fmt(aiTime)}!" : "Montando...";
        }

        private void Fail(string message)
        {
            failed = true;
            Debug.LogError("[IA] " + message);
            if (statusText != null) statusText.text = "IA indisponível (veja o Console).";
        }

        private void HandlePlayerResult(EvaluationResult player)
        {
            if (!ready) return;

            var report = CompatibilityChecker.Validate(aiBuild);
            var ai = ScoreCalculator.Calculate(mission, aiBuild, report, aiBuild.TotalPrice, aiTime, 0);

            if (player.Total > ai.Total) player.headline = $"Você venceu a {profile.displayName}!";
            else if (player.Total < ai.Total) player.headline = $"A {profile.displayName} venceu desta vez";
            else player.headline = "Empate!";

            var lines = player.detailLines;
            lines.Clear();
            lines.Add($"Você: {player.Total} pts   |   {profile.displayName}: {ai.Total} pts");
            lines.Add($"Tempo: você {Fmt(player.elapsedSeconds)}   |   IA {Fmt(aiTime)}");
            lines.Add($"Montagem: você R$ {player.buildPrice:N0}   |   IA R$ {aiBuild.TotalPrice:N0}   (orçamento R$ {mission.budget:N0})");
            lines.Add("");
            lines.Add("Seu desempenho:");
            lines.Add($"   Compatibilidade: {player.compatibilityScore} / {ScoreCalculator.MaxCompatibility}");
            lines.Add($"   Requisitos: {player.requirementsScore} / {ScoreCalculator.MaxRequirements}");
            lines.Add($"   Orçamento: {player.budgetScore} / {ScoreCalculator.MaxBudget}");
            lines.Add($"   Tempo: {player.timeScore} / {ScoreCalculator.MaxTime}");
            if (player.penalty > 0) lines.Add($"   Penalidade por tentativas erradas: -{player.penalty}");
            foreach (var u in player.unmetRequirements) lines.Add("   Requisito não atendido: " + u);
            foreach (var i in player.issues) lines.Add("   Problema: " + i);
            lines.Add("");
            lines.Add($"Montagem da {profile.displayName}:");
            foreach (var d in AIBuilder.Describe(aiBuild)) lines.Add("   " + d);
        }

        private static string Fmt(float seconds) => $"{(int)seconds / 60:00}:{(int)seconds % 60:00}";
    }
}
