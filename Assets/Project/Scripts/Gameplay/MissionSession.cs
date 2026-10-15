using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MestreDoPC
{
    public class MissionSession : Singleton<MissionSession>
    {
        public static EvaluationResult LastResult { get; set; }
        public static event Action<EvaluationResult> OnResultReady; 

        [SerializeField] private MissionCatalogSO catalog;
        [SerializeField] private MissionDataSO fallbackMission;

        public MissionDataSO Mission { get; private set; }
        public float Elapsed { get; private set; }
        public bool Finished { get; private set; }

        public float TimeRemaining => Mission != null && Mission.timeLimitSeconds > 0
            ? Mathf.Max(0f, Mission.timeLimitSeconds - Elapsed) : -1f;

        private void Start()
        {
            var gm = GameManager.Instance;
            GameMode mode = gm != null ? gm.CurrentMode : GameMode.Assembly;
            Difficulty diff = gm != null ? gm.CurrentDifficulty : Difficulty.Easy;

            Mission = catalog != null ? catalog.Find(mode, diff) : null;
            if (Mission == null)
            {
                Debug.LogWarning($"Nenhuma missão para {mode}/{diff}; usando a missão padrão.");
                Mission = fallbackMission;
            }
            if (Mission == null)
            {
                Debug.LogError("MissionSession sem missão: preencha o catálogo ou a missão padrão.");
                enabled = false;
                return;
            }

            BudgetManager.Instance.Begin(Mission.budget);
        }

        private void Update()
        {
            if (Finished) return;
            Elapsed += Time.deltaTime;
            if (Mission.timeLimitSeconds > 0 && Elapsed >= Mission.timeLimitSeconds) Finish(true);
        }

        public void OnFinishPressed()
        {
            if (Finished) return;

            var missing = CompatibilityChecker.MissingParts(BuildManager.Instance.Build);
            if (missing.Length > 0)
            {
                if (WorkshopHUD.Instance != null)
                    WorkshopHUD.Instance.ShowMessage("Ainda faltam peças: " + string.Join(", ", missing));
                return;
            }
            Finish(false);
        }

        private void Finish(bool timedOut)
        {
            Finished = true;

            LastResult = ScoreCalculator.Calculate(
                Mission, BuildManager.Instance.Build, BuildManager.Instance.Report,
                BudgetManager.Instance.Spent, Elapsed, BuildManager.Instance.WrongAttempts);
            LastResult.timedOut = timedOut;
            OnResultReady?.Invoke(LastResult);

            if (SaveManager.Instance != null && LastResult.Total > 0)
            {
                LastResult.isNewRecord = SaveManager.Instance.AddRecord(new RecordEntry
                {
                    mode = Mission.mode.ToString(),
                    difficulty = Mission.difficulty.ToString(),
                    missionId = Mission.id,
                    score = LastResult.Total,
                    timeSeconds = Elapsed,
                    budgetSpent = BudgetManager.Instance.Spent,
                    date = DateTime.Now.ToString("yyyy-MM-dd")
                });
                SaveManager.Instance.MarkMissionCompleted(Mission.id);
            }

            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(SceneNames.Evaluation);
            else SceneManager.LoadScene(SceneNames.Evaluation);
        }
    }
}
