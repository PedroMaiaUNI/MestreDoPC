using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/AI Profile")]
    public class AIProfileSO : ScriptableObject
    {
        public string displayName = "IA";
        public Difficulty difficulty;

        [Tooltip("Segundos que a IA leva para terminar (sorteado entre X = mínimo e Y = máximo).")]
        public Vector2 timeRange = new Vector2(200f, 300f);

        [Header("Qualidade da escolha (0 = a melhor montagem, 1 = a pior válida)")]
        [Range(0f, 1f)] public float rankMin = 0f;
        [Range(0f, 1f)] public float rankMax = 0.1f;
    }
}
