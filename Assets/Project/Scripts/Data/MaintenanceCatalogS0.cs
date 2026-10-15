using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Maintenance Catalog")]
    public class MaintenanceCatalogSO : ScriptableObject
    {
        [SerializeField] private List<MaintenanceCaseDataSO> cases = new List<MaintenanceCaseDataSO>();

        public MaintenanceCaseDataSO Find(Difficulty difficulty)
            => cases.FirstOrDefault(c => c.difficulty == difficulty);
    }
}
