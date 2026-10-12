using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Component Database")]
    public class ComponentDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<ComponentDataSO> components = new List<ComponentDataSO>();

        public IReadOnlyList<ComponentDataSO> All => components;

        public IEnumerable<T> GetAll<T>() where T : ComponentDataSO => components.OfType<T>();

        public IEnumerable<ComponentDataSO> GetByCategory(ComponentCategory category)
            => components.Where(c => c.Category == category);

        public ComponentDataSO GetById(string id) => components.FirstOrDefault(c => c.id == id);
    }
}
