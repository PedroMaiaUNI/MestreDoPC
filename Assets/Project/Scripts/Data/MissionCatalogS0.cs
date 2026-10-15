using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Mission Catalog")]
    public class MissionCatalogSO : ScriptableObject
    {
        [SerializeField] private List<MissionDataSO> missions = new List<MissionDataSO>();

        public MissionDataSO Find(GameMode mode, Difficulty difficulty)
            => missions.FirstOrDefault(m => m.mode == mode && m.difficulty == difficulty);
    }
}
