using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Mission")]
    public class MissionDataSO : ScriptableObject
    {
        [Header("Identificação")]
        public string id;
        public string title;
        [TextArea(3, 8)] public string clientBrief;   // fala do cliente
        public GameMode mode;
        public Difficulty difficulty;

        [Header("Restrições")]
        public int budget;
        public float timeLimitSeconds;                 // 0 = sem limite

        [Header("Requisitos (0 = não exigido)")]
        public int minRamGB;
        public int minStorageGB;
        public int minCores;
        public int minGpuVramGB;
    }
}
