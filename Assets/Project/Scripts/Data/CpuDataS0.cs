using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Components/CPU")]
    public class CpuDataSO : ComponentDataSO
    {
        public SocketType socket;
        public int tdp;
        public int cores;
        public int threads;
        public bool hasIntegratedGraphics;
        public override ComponentCategory Category => ComponentCategory.Cpu;
    }
}
