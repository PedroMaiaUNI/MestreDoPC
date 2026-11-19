using System.Collections.Generic;
using System.Linq;

namespace MestreDoPC
{
    public enum IssueSeverity { Warning, Error }

    public class CompatibilityIssue
    {
        public IssueSeverity Severity { get; }
        public string Message { get; }
        public CompatibilityIssue(IssueSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }
    }

    public class CompatibilityReport
    {
        public List<CompatibilityIssue> Issues { get; } = new List<CompatibilityIssue>();
        public bool HasErrors => Issues.Any(i => i.Severity == IssueSeverity.Error);

        public void Add(IssueSeverity s, string msg) => Issues.Add(new CompatibilityIssue(s, msg));
    }

    // Representa o PC "em construção". Slots nulos = ainda não escolhidos.
    public class PcBuild
    {
        public MotherboardDataSO motherboard;
        public CpuDataSO cpu;
        public RamDataSO ram;
        public int ramQuantity = 1;
        public List<StorageDataSO> storage = new List<StorageDataSO>();
        public GpuDataSO gpu;
        public PsuDataSO psu;
        public CoolerDataSO cooler;
        public bool UsesIntegratedGraphics => gpu == null && cpu != null && cpu.hasIntegratedGraphics;
        public bool HasVideoOutput => gpu != null || UsesIntegratedGraphics;
        public int VideoMemoryGB => gpu != null ? gpu.vramGB : (UsesIntegratedGraphics ? cpu.integratedVramGB : 0);

        public int TotalPrice =>
            (motherboard ? motherboard.price : 0) + (cpu ? cpu.price : 0) +
            (ram ? ram.price * ramQuantity : 0) + storage.Sum(s => s.price) +
            (gpu ? gpu.price : 0) + (psu ? psu.price : 0) + (cooler ? cooler.price : 0);


        public string VideoDescription
        {
            get
            {
                if (gpu != null) return gpu.DisplayName;
                if (UsesIntegratedGraphics)
                    return string.IsNullOrEmpty(cpu.integratedGpuName) ? "Gráficos integrados" : $"{cpu.integratedGpuName} (integrado)";
                return null;
            }
        }
    }
}
