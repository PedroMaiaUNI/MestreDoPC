using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Components/Storage")]
    public class StorageDataSO : ComponentDataSO
    {
        public StorageType type;
        public int capacityGB;

        public override ComponentCategory Category => ComponentCategory.Storage;
        public override string SpecsSummary => $"{type} • {capacityGB} GB";
    }
}
