using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class DebugPartSpawner : MonoBehaviour
    {
        [SerializeField] private List<ComponentDataSO> parts = new List<ComponentDataSO>();

        private void Start()
        {
            foreach (var p in parts) TrayManager.Instance.Spawn(p);
        }
    }
}
