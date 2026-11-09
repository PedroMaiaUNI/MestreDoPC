using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Maintenance Catalog")]
    public class MaintenanceCatalogSO : ScriptableObject
    {
        [SerializeField] private List<MaintenanceCaseDataSO> cases = new List<MaintenanceCaseDataSO>();

        public MaintenanceCaseDataSO Pick(Difficulty difficulty)
        {
            var pool = cases.Where(c => c != null && c.difficulty == difficulty).ToList();
            return MissionPicker.Pick(pool, c => c.id, $"maintenance/{difficulty}");
        }

        public MaintenanceCaseDataSO Find(Difficulty difficulty)
            => cases.FirstOrDefault(c => c.difficulty == difficulty);
    }
}
