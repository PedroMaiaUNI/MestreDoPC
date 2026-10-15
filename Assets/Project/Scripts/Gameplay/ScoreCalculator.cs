using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    public static class ScoreCalculator
    {
        public const int MaxCompatibility = 400;
        public const int MaxRequirements = 300;
        public const int MaxBudget = 200;
        public const int MaxTime = 100;
        public const int MaxTotal = MaxCompatibility + MaxRequirements + MaxBudget + MaxTime;
        public const int PenaltyPerWrongAttempt = 25;

        public static EvaluationResult Calculate(MissionDataSO m, PcBuild b, CompatibilityReport report,
                                                 int spent, float elapsed, int wrongAttempts)
        {
            var r = new EvaluationResult
            {
                missionId = m.id, missionTitle = m.title, mode = m.mode, difficulty = m.difficulty,
                budget = m.budget, spent = spent, buildPrice = b.TotalPrice, elapsedSeconds = elapsed
            };

            var missing = CompatibilityChecker.MissingParts(b);
            r.bootable = missing.Length == 0 && !report.HasErrors;

            foreach (var i in report.Issues) r.issues.Add(i.Message);
            foreach (var part in missing) r.issues.Add("Falta: " + part);

            if (!r.bootable) return r; // PC que não liga = 0 pontos

            // Compatibilidade: cada alerta tira 50.
            int warnings = report.Issues.Count(i => i.Severity == IssueSeverity.Warning);
            r.compatibilityScore = Mathf.Max(0, MaxCompatibility - warnings * 50);

            // Requisitos do cliente: proporção atendida.
            var reqs = CheckRequirements(m, b);
            int ok = reqs.Count(x => x.ok);
            r.requirementsScore = reqs.Count == 0 ? MaxRequirements
                                                  : Mathf.RoundToInt(MaxRequirements * (float)ok / reqs.Count);
            foreach (var x in reqs) if (!x.ok) r.unmetRequirements.Add(x.label);

            // Orçamento: premia usar bem a verba e não desperdiçar peças compradas e não instaladas.
            float usage = m.budget > 0 ? Mathf.Clamp01((float)b.TotalPrice / m.budget) : 1f;
            float efficiency = spent > 0 ? Mathf.Clamp01((float)b.TotalPrice / spent) : 1f;
            r.budgetScore = Mathf.RoundToInt(MaxBudget * usage * efficiency);

            // Tempo: sem limite = pontuação cheia.
            r.timeScore = m.timeLimitSeconds > 0
                ? Mathf.RoundToInt(MaxTime * Mathf.Clamp01(1f - elapsed / m.timeLimitSeconds))
                : MaxTime;

            r.penalty = wrongAttempts * PenaltyPerWrongAttempt;
            return r;
        }

        private static List<(string label, bool ok)> CheckRequirements(MissionDataSO m, PcBuild b)
        {
            var list = new List<(string label, bool ok)>();
            int ramTotal = b.ram != null ? b.ram.capacityGB * b.ramQuantity : 0;
            int storageTotal = b.storage.Sum(s => s.capacityGB);

            if (m.minRamGB > 0)
                list.Add(($"Memória mínima de {m.minRamGB} GB", ramTotal >= m.minRamGB));
            if (m.minStorageGB > 0)
                list.Add(($"Armazenamento mínimo de {m.minStorageGB} GB", storageTotal >= m.minStorageGB));
            if (m.minCores > 0)
                list.Add(($"CPU com pelo menos {m.minCores} núcleos", b.cpu != null && b.cpu.cores >= m.minCores));
            if (m.minGpuVramGB > 0)
                list.Add(($"GPU com pelo menos {m.minGpuVramGB} GB de VRAM", b.gpu != null && b.gpu.vramGB >= m.minGpuVramGB));
            return list;
        }
    }
}

