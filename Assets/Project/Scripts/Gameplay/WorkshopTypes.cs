using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public enum WorkshopPhase { Briefing, Shopping, Assembly, Result }

    public class BootResult
    {
        public bool Success;
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
    }

    // Ajudas por nível (GDD 2.1.1): Easy = Nível 1, Medium = Nível 2, Hard = Nível 3.
    public readonly struct AssistanceSettings
    {
        public readonly bool ShowSlotHolograms;
        public readonly bool ShowCompatibilityFilter;
        public readonly bool ShowPartExplanations;
        public readonly bool BlockIncompleteBuild;

        private AssistanceSettings(bool holograms, bool filter, bool explanations, bool blockIncomplete)
        {
            ShowSlotHolograms = holograms;
            ShowCompatibilityFilter = filter;
            ShowPartExplanations = explanations;
            BlockIncompleteBuild = blockIncomplete;
        }

        public static AssistanceSettings For(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:   return new AssistanceSettings(true, true, true, true);
                case Difficulty.Medium: return new AssistanceSettings(true, false, true, false);
                default:                return new AssistanceSettings(false, false, false, false);
            }
        }
    }

    // Monta um PcBuild (Etapa 3) a partir de uma lista de peças.
    public static class PcBuildFactory
    {
        public static PcBuild FromParts(IEnumerable<ComponentDataSO> parts)
        {
            var build = new PcBuild();
            int ramCount = 0;

            foreach (ComponentDataSO part in parts)
            {
                switch (part)
                {
                    case MotherboardDataSO m: build.motherboard = m; break;
                    case CpuDataSO c:         build.cpu = c; break;
                    case RamDataSO r:         if (build.ram == null) build.ram = r; ramCount++; break;
                    case StorageDataSO s:     build.storage.Add(s); break;
                    case GpuDataSO g:         build.gpu = g; break;
                    case PsuDataSO u:         build.psu = u; break;
                    case CoolerDataSO k:      build.cooler = k; break;
                }
            }

            build.ramQuantity = Mathf.Max(1, ramCount);
            return build;
        }
    }
}
