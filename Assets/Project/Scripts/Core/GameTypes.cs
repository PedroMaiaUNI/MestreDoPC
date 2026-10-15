namespace MestreDoPC
{
    public enum GameMode { Assembly, AIChallenge, Maintenance, PartsManual }
    public enum Difficulty { Easy, Medium, Hard }

    // Centraliza os nomes para evitar strings soltas pelo código.
    public static class SceneNames
    {
        public const string Boot = "00_Boot";
        public const string MainMenu = "01_MainMenu";
        public const string Options = "02_Options";
        public const string Records = "03_Records";
        public const string Workshop = "05_Workshop";
        public const string MaintenanceArena = "06_MaintenanceArena";
        public const string AIArena = "07_AIChallengeArena";
        public const string PartsManual = "08_PartsManual";
        public const string Evaluation = "09_Evaluation";

        public static string ForMode(GameMode mode) => mode switch
        {
            GameMode.Assembly    => Workshop,
            GameMode.AIChallenge => AIArena,
            GameMode.Maintenance => MaintenanceArena,
            GameMode.PartsManual => PartsManual,
            _                    => MainMenu
        };
    }
  
}
