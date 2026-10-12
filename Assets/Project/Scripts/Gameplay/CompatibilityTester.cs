using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class CompatibilityTester : MonoBehaviour
    {
        public MotherboardDataSO motherboard;
        public CpuDataSO cpu;
        public RamDataSO ram;
        public List<StorageDataSO> storage;
        public GpuDataSO gpu;
        public PsuDataSO psu;
        public CoolerDataSO cooler;

        [ContextMenu("Run Test")]
        private void Run()
        {
            var build = new PcBuild
            {
                motherboard = motherboard, cpu = cpu, ram = ram,
                storage = storage ?? new List<StorageDataSO>(),
                gpu = gpu, psu = psu, cooler = cooler
            };

            var report = CompatibilityChecker.Validate(build);
            if (report.Issues.Count == 0) Debug.Log("Build compatível!");
            foreach (var i in report.Issues) Debug.Log($"[{i.Severity}] {i.Message}");
            Debug.Log("Faltando: " + string.Join(", ", CompatibilityChecker.MissingParts(build)));
        }
    }
}
