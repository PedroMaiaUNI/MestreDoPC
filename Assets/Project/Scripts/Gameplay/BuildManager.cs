using System;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    public class BuildManager : Singleton<BuildManager>
    {
        public PcBuild Build { get; private set; } = new PcBuild();
        public CompatibilityReport Report { get; private set; } = new CompatibilityReport();
        public int WrongAttempts { get; private set; }   // usado na pontuação (Etapa 7)

        public event Action<PcBuild, CompatibilityReport> OnBuildChanged;
        public event Action<PartView> OnPartRemoved;

        public bool CanInstall(PartSlot slot, PartView part, out string reason)
        {
            reason = null;

            if (slot.category != part.Data.Category)
            { reason = "Essa peça não encaixa nesse slot."; return false; }

            if (slot.Installed != null)
            { reason = "Esse slot já está ocupado."; return false; }

            if (slot.requiresMotherboard && !HasInstalled<MotherboardDataSO>())
            { reason = "Instale a placa-mãe primeiro."; return false; }

            if (part.Data is RamDataSO ram)
            {
                foreach (var s in PartSlot.All)
                {
                    if (s.Installed != null && s.Installed.Data is RamDataSO other && other != ram)
                    { reason = "Use módulos de RAM iguais (limitação atual)."; return false; }
                }
            }
            return true;
        }

        public bool TryInstall(PartSlot slot, PartView part, out string message)
        {
            if (slot.category != part.Data.Category) WrongAttempts++;

            if (!CanInstall(slot, part, out message)) return false;

            int errBefore = Report.Issues.Count(i => i.Severity == IssueSeverity.Error);
            int warnBefore = Report.Issues.Count(i => i.Severity == IssueSeverity.Warning);

            slot.Place(part);
            Rebuild();

            int errAfter = Report.Issues.Count(i => i.Severity == IssueSeverity.Error);
            int warnAfter = Report.Issues.Count(i => i.Severity == IssueSeverity.Warning);

            if (errAfter > errBefore)
            {
                WrongAttempts++;
                part.SetFeedback(PartFeedback.Error);
                message = "Instalada, mas há uma incompatibilidade!";
            }
            else if (warnAfter > warnBefore)
            {
                part.SetFeedback(PartFeedback.Warning);
                message = "Instalada, mas com um alerta.";
            }
            else message = "Peça instalada.";

            return true;
        }

        public void ForceInstall(PartSlot slot, PartView part)
        {
            slot.Place(part);
            Rebuild();
        }

        public void Remove(PartView part)
        {
            var slot = part.CurrentSlot;
            if (slot == null) return;

            slot.Clear();
            part.SetFeedback(PartFeedback.None);
            OnPartRemoved?.Invoke(part);

            // Sem a placa-mãe, tudo que dependia dela volta para a bandeja.
            if (part.Data is MotherboardDataSO)
            {
                foreach (var s in PartSlot.All)
                {
                    if (!s.requiresMotherboard || s.Installed == null) continue;
                    var dependent = s.Installed;
                    s.Clear();
                    dependent.SetFeedback(PartFeedback.None);
                    dependent.SendHome();
                }
            }
            Rebuild();
        }

        private bool HasInstalled<T>() where T : ComponentDataSO
            => PartSlot.All.Any(s => s.Installed != null && s.Installed.Data is T);

        private void Rebuild()
        {
            var b = new PcBuild();
            int ramCount = 0;

            foreach (var s in PartSlot.All)
            {
                if (s.Installed == null) continue;
                switch (s.Installed.Data)
                {
                    case MotherboardDataSO m: b.motherboard = m; break;
                    case CpuDataSO c:         b.cpu = c; break;
                    case RamDataSO r:         if (b.ram == null) b.ram = r; ramCount++; break;
                    case StorageDataSO st:    b.storage.Add(st); break;
                    case GpuDataSO g:         b.gpu = g; break;
                    case PsuDataSO p:         b.psu = p; break;
                    case CoolerDataSO co:     b.cooler = co; break;
                }
            }
            b.ramQuantity = Mathf.Max(1, ramCount);

            Build = b;
            Report = CompatibilityChecker.Validate(b);

            // Sem erros, nenhuma peça deve continuar vermelha.
            if (!Report.HasErrors)
                foreach (var s in PartSlot.All)
                    if (s.Installed != null) s.Installed.SetFeedback(PartFeedback.None);

            OnBuildChanged?.Invoke(Build, Report);
        }
    }
}
