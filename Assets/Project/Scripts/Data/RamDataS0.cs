using UnityEngine;

namespace MestreDoPC{
[CreateAssetMenu(menuName = "MestreDoPC/Components/RAM")]
    public class RamDataSO : ComponentDataSO
    {
        public RamType type;
        public int capacityGB;   // por módulo
        public int speedMhz;
        public override ComponentCategory Category => ComponentCategory.Ram;
    }
}
