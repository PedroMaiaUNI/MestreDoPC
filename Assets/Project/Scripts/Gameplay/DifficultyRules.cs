namespace MestreDoPC
{
    public static class DifficultyRules
    {
        private static Difficulty Current =>
            GameManager.Instance != null ? GameManager.Instance.CurrentDifficulty : Difficulty.Easy;

        public static bool ShowHolograms => Current != Difficulty.Hard;      // slot compatível acende ao arrastar
        public static bool ShowLiveIssues => Current == Difficulty.Easy;     // lista de problemas em tempo real
        public static bool ShowVideoStatus => Current != Difficulty.Hard &&
                                              (GameManager.Instance == null || GameManager.Instance.CurrentMode != GameMode.Maintenance);
    }
}
