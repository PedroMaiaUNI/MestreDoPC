using UnityEngine;

namespace MestreDoPC{

    [CreateAssetMenu(menuName = "MestreDoPC/Components/GPU")]
    public class GpuDataSO : ComponentDataSO
    {
        public int tdp;
        public int vramGB;
        public override ComponentCategory Category => ComponentCategory.Gpu;
    }
}
