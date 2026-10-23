using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    public static class AIBuilder
    {
        public static PcBuild Build(ComponentDatabaseSO db, MissionDataSO mission, AIProfileSO profile,
                                    float aiTime, int attempts = 600)
        {
            var cpus = db.GetAll<CpuDataSO>().ToList();
            var mobos = db.GetAll<MotherboardDataSO>().ToList();
            var rams = db.GetAll<RamDataSO>().ToList();
            var storages = db.GetAll<StorageDataSO>().ToList();
            var gpus = db.GetAll<GpuDataSO>().ToList();
            var psus = db.GetAll<PsuDataSO>().ToList();
            var coolers = db.GetAll<CoolerDataSO>().ToList();

            var found = new List<(PcBuild build, int score)>();
            var seen = new HashSet<string>();

            for (int i = 0; i < attempts; i++)
            {
                var b = RandomBuild(cpus, mobos, rams, storages, gpus, psus, coolers);
                if (b == null || b.TotalPrice > mission.budget) continue;
                if (!seen.Add(Key(b))) continue;

                var report = CompatibilityChecker.Validate(b);
                if (report.HasErrors || CompatibilityChecker.MissingParts(b).Length > 0) continue;

                int score = ScoreCalculator.Calculate(mission, b, report, b.TotalPrice, aiTime, 0).Total;
                found.Add((b, score));
            }
            if (found.Count == 0) return null;

            found.Sort((x, y) => y.score.CompareTo(x.score)); 
            int last = found.Count - 1;
            int lo = Mathf.FloorToInt(profile.rankMin * last);
            int hi = Mathf.Max(lo, Mathf.FloorToInt(profile.rankMax * last));
            return found[Random.Range(lo, hi + 1)].build;
        }

        public static List<string> Describe(PcBuild b)
        {
            var list = new List<string>
            {
                $"Placa-mãe: {b.motherboard.DisplayName}",
                $"CPU: {b.cpu.DisplayName}",
                $"Memória: {(b.ramQuantity > 1 ? b.ramQuantity + "× " : "")}{b.ram.DisplayName}"
            };
            foreach (var s in b.storage) list.Add($"Armazenamento: {s.DisplayName}");
            if (b.gpu != null) list.Add($"GPU: {b.gpu.DisplayName}");
            list.Add($"Fonte: {b.psu.DisplayName}");
            list.Add($"Cooler: {b.cooler.DisplayName}");
            return list;
        }

        private static PcBuild RandomBuild(List<CpuDataSO> cpus, List<MotherboardDataSO> mobos, List<RamDataSO> rams,
                                           List<StorageDataSO> storages, List<GpuDataSO> gpus,
                                           List<PsuDataSO> psus, List<CoolerDataSO> coolers)
        {
            var cpu = Pick(cpus);
            if (cpu == null) return null;

            var mb = Pick(mobos.Where(m => m.socket == cpu.socket).ToList());
            if (mb == null) return null;

            var ram = Pick(rams.Where(r => r.type == mb.ramType).ToList());
            var cooler = Pick(coolers.Where(c => c.supportedSockets != null && c.supportedSockets.Contains(cpu.socket)).ToList());
            var storage = Pick(storages);
            var psu = Pick(psus);
            if (ram == null || cooler == null || storage == null || psu == null) return null;

            var b = new PcBuild
            {
                motherboard = mb, cpu = cpu, ram = ram,
                ramQuantity = Random.Range(1, mb.ramSlots + 1),
                psu = psu, cooler = cooler
            };
            b.storage.Add(storage);
            if (Random.value < 0.3f) b.storage.Add(storage);

            if (!cpu.hasIntegratedGraphics || Random.value < 0.5f) b.gpu = Pick(gpus);
            return b;
        }

        private static T Pick<T>(IList<T> list) where T : class
            => list.Count == 0 ? null : list[Random.Range(0, list.Count)];

        private static string Key(PcBuild b)
            => $"{b.motherboard.id}|{b.cpu.id}|{b.ram.id}x{b.ramQuantity}|" +
               $"{string.Join(",", b.storage.Select(s => s.id))}|{(b.gpu != null ? b.gpu.id : "-")}|{b.psu.id}|{b.cooler.id}";
    }
}
