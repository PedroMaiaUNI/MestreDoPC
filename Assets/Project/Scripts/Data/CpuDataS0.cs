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
        public CoolerDataSO bundledCooler;

        [Header("Vídeo integrado (só vale se hasIntegratedGraphics estiver marcado)")]
        public string integratedGpuName;
        public int integratedVramGB = 1;

        public override string SpecsSummary => $"{socket} • {cores}C/{threads}T • {tdp} W"
            + (hasIntegratedGraphics
                ? $" • vídeo integrado: {(string.IsNullOrEmpty(integratedGpuName) ? "sim" : integratedGpuName)}"
                : " • sem vídeo integrado")
            + (bundledCooler != null ? $" • inclui cooler {bundledCooler.model}" : " • sem cooler");
    }
}
