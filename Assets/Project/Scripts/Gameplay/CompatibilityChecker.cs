using System.Linq;

namespace MestreDoPC
{
    public static class CompatibilityChecker
    {
        private const int BaseSystemDrawWatts = 80;   // placa, RAM, discos, fans
        private const float PsuSafetyMargin = 1.25f;

        // Só valida o que já foi escolhido, então dá para chamar a cada compra.
        public static CompatibilityReport Validate(PcBuild b)
        {
            var report = new CompatibilityReport();

            if (b.motherboard != null && b.cpu != null && b.motherboard.socket != b.cpu.socket)
                report.Add(IssueSeverity.Error,
                    $"O socket da CPU ({b.cpu.socket}) não é compatível com a placa-mãe ({b.motherboard.socket}).");

            if (b.motherboard != null && b.ram != null)
            {
                if (b.ram.type != b.motherboard.ramType)
                    report.Add(IssueSeverity.Error,
                        $"A placa-mãe usa {b.motherboard.ramType}, mas a memória é {b.ram.type}.");
                if (b.ramQuantity > b.motherboard.ramSlots)
                    report.Add(IssueSeverity.Error,
                        $"A placa-mãe só tem {b.motherboard.ramSlots} slots de RAM.");
                if (b.ram.capacityGB * b.ramQuantity > b.motherboard.maxRamGB)
                    report.Add(IssueSeverity.Error,
                        $"A capacidade total excede o máximo suportado ({b.motherboard.maxRamGB} GB).");
            }

            if (b.cooler != null && b.motherboard != null &&
                (b.cooler.supportedSockets == null || !b.cooler.supportedSockets.Contains(b.motherboard.socket)))
                report.Add(IssueSeverity.Error, "O cooler não suporta o socket desta placa-mãe.");

            if (b.cooler != null && b.cpu != null && b.cooler.tdpRating < b.cpu.tdp)
                report.Add(IssueSeverity.Warning, "O cooler pode não dissipar bem o calor desta CPU.");

            if (b.psu != null)
            {
                int required = BaseSystemDrawWatts
                             + (b.cpu != null ? b.cpu.tdp : 0)
                             + (b.gpu != null ? b.gpu.tdp : 0);

                if (b.psu.wattage < required)
                    report.Add(IssueSeverity.Error,
                        $"Fonte insuficiente: o sistema exige cerca de {required} W e a fonte entrega {b.psu.wattage} W.");
                else if (b.psu.wattage < required * PsuSafetyMargin)
                    report.Add(IssueSeverity.Warning, "A fonte atende, mas com pouca folga de potência.");
            }

            return report;
        }

        // Peças que ainda faltam para o PC poder ligar (usado antes da montagem/boot).
        public static string[] MissingParts(PcBuild b)
        {
            var missing = new System.Collections.Generic.List<string>();
            if (b.motherboard == null) missing.Add("Placa-mãe");
            if (b.cpu == null) missing.Add("CPU");
            if (b.ram == null) missing.Add("Memória RAM");
            if (b.storage.Count == 0) missing.Add("Armazenamento");
            if (b.psu == null) missing.Add("Fonte");
            if (b.cooler == null) missing.Add("Cooler");
            // GPU só é obrigatória se a CPU não tiver vídeo integrado.
            if (b.gpu == null && (b.cpu == null || !b.cpu.hasIntegratedGraphics)) missing.Add("GPU");
            return missing.ToArray();
        }
    }
}
