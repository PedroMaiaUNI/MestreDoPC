using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    public static class MissionPicker
    {
        private static readonly Dictionary<string, string> lastPicked = new Dictionary<string, string>();

        public static T Pick<T>(List<T> pool, System.Func<T, string> idOf, string key) where T : class
        {
            if (pool.Count == 0) return null;
            if (pool.Count == 1) return pool[0];

            var done = SaveManager.Instance != null ? SaveManager.Instance.Data.completedMissionIds : null;
            var candidates = done == null ? pool : pool.Where(m => !done.Contains(idOf(m))).ToList();
            if (candidates.Count == 0) candidates = pool;

            if (candidates.Count > 1 && lastPicked.TryGetValue(key, out var last))
            {
                var others = candidates.Where(m => idOf(m) != last).ToList();
                if (others.Count > 0) candidates = others;
            }

            var chosen = candidates[Random.Range(0, candidates.Count)];
            lastPicked[key] = idOf(chosen);
            return chosen;
        }
    }
}
