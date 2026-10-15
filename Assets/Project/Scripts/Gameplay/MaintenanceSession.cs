using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MestreDoPC
{
    public class MaintenanceSession : Singleton<MaintenanceSession>
    {
        private const int MaxEfficacy = 400, MaxCare = 300, MaxCost = 200, MaxTime = 100;

        [SerializeField] private MaintenanceCatalogSO catalog;
        [SerializeField] private MaintenanceCaseDataSO fallbackCase;
        [SerializeField] private Transform parkingOrigin;          
        [SerializeField] private float inspectTimeCost = 5f;
        [SerializeField] private float cleanTimeCost = 8f;
        [SerializeField] private float testTimeCost = 10f;

        private class ActiveDefect
        {
            public DefectData data;
            public PartView part;
            public bool cleaned;
        }

        public MaintenanceCaseDataSO Case { get; private set; }
        public float Elapsed { get; private set; }
        public bool Finished { get; private set; }
        public bool Plugged { get; private set; } = true; 

        public float TimeRemaining => Case != null ? Mathf.Max(0f, Case.timeLimitSeconds - Elapsed) : 0f;

        private readonly List<ActiveDefect> defects = new List<ActiveDefect>();
        private readonly List<PartView> initialParts = new List<PartView>();
        private int unpluggedActions;
        private float parkCursor;

        private void Start()
        {
            var gm = GameManager.Instance;
            Difficulty diff = gm != null ? gm.CurrentDifficulty : Difficulty.Easy;

            Case = catalog != null ? catalog.Find(diff) : null;
            if (Case == null) Case = fallbackCase;
            if (Case == null)
            {
                Debug.LogError("[Manutenção] Sem caso: preencha o catálogo ou o caso padrão no MaintenanceSession.");
                enabled = false;
                return;
            }

            BudgetManager.Instance.Begin(Case.budget);
            BuildInitialPc();
            BuildManager.Instance.OnPartRemoved += HandlePartRemoved;
        }

        private void OnDestroy()
        {
            if (BuildManager.Instance != null) BuildManager.Instance.OnPartRemoved -= HandlePartRemoved;
        }

        private void Update()
        {
            if (Finished) return;
            Elapsed += Time.deltaTime;
            if (Elapsed >= Case.timeLimitSeconds) Finish(true);
        }

        // ---------- Montagem do PC recebido ----------

        private void BuildInitialPc()
        {
            var list = new List<ComponentDataSO>();
            void Add(ComponentDataSO d, int qty = 1)
            {
                if (d == null) return;
                for (int i = 0; i < qty; i++) list.Add(d);
            }

            Add(Case.motherboard);
            Add(Case.cpu);
            Add(Case.ram, Mathf.Max(1, Case.ramQuantity));
            foreach (var s in Case.storage) Add(s);
            Add(Case.gpu);
            Add(Case.psu);
            Add(Case.cooler);

            foreach (var data in list)
            {
                var slot = PartSlot.All.FirstOrDefault(s => s.category == data.Category && s.Installed == null);
                if (slot == null)
                {
                    Debug.LogWarning($"[Manutenção] Sem slot livre para {data.DisplayName}.");
                    continue;
                }

                var view = PartFactory.Create(data, slot.snapPoint.position, slot.snapPoint.rotation);
                Park(view);                                    
                BuildManager.Instance.ForceInstall(slot, view); 
                initialParts.Add(view);
            }

            foreach (var d in Case.defects)
            {
                var part = initialParts.FirstOrDefault(p => p.Data.Category == d.category);
                if (part == null)
                {
                    Debug.LogWarning($"[Manutenção] Defeito em {d.category}, mas o PC não tem essa peça.");
                    continue;
                }
                defects.Add(new ActiveDefect { data = d, part = part });
            }
        }

        private void Park(PartView view)
        {
            if (parkingOrigin == null) return;

            var rends = view.GetComponentsInChildren<Renderer>();
            float depth = 0.3f;
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                depth = b.size.z;
            }

            view.SetHome(parkingOrigin.position + new Vector3(0f, 0f, -(parkCursor + depth * 0.5f)));
            parkCursor += depth + 0.1f;
        }

        // ---------- Ações do jogador ----------

        public void TogglePlug()
        {
            if (Finished) return;
            Plugged = !Plugged;
            Say(Plugged ? "PC conectado à tomada." : "PC desconectado da tomada.");
        }

        public void Inspect(PartView part)
        {
            if (Finished) return;
            Elapsed += inspectTimeCost;

            var d = defects.FirstOrDefault(x => x.part == part && ShowsSign(x));
            Say(d != null
                ? $"{part.Data.DisplayName}: {d.data.inspectionHint}"
                : $"{part.Data.DisplayName}: sem sinais visíveis de problema.");
        }

        public void Clean(PartView part)
        {
            if (Finished) return;
            Elapsed += cleanTimeCost;

            if (Plugged) unpluggedActions++;

            var d = defects.FirstOrDefault(x => x.part == part && x.data.type == DefectType.Dust);
            if (d != null && !d.cleaned)
            {
                d.cleaned = true;
                Say(Plugged ? $"{part.Data.DisplayName} limpo, mas com o PC ligado na tomada (risco!)."
                            : $"{part.Data.DisplayName} limpo.");
            }
            else Say($"{part.Data.DisplayName}: não havia sujeira relevante (tempo perdido).");
        }

        public void RunTest()
        {
            if (Finished) return;
            if (!Plugged) { Say("Conecte o PC à tomada para testar."); return; }

            Elapsed += testTimeCost;

            var build = BuildManager.Instance.Build;
            var missing = CompatibilityChecker.MissingParts(build);
            if (missing.Length > 0)
            {
                Say("O PC não liga: faltam peças (" + string.Join(", ", missing) + ").");
                return;
            }

            var report = BuildManager.Instance.Report;
            if (report.HasErrors)
            {
                Say("O PC não liga: " + report.Issues.First(i => i.Severity == IssueSeverity.Error).Message);
                return;
            }

            var active = defects.FirstOrDefault(d => !IsResolved(d));
            if (active != null)
            {
                Say("Resultado do teste: " + active.data.symptom);
                return;
            }

            Say("O PC ligou e funciona perfeitamente! Se terminou, clique em Finalizar serviço.");
        }

        public void OnFinishPressed()
        {
            if (!Finished) Finish(false);
        }

        private void HandlePartRemoved(PartView part)
        {
            if (Finished || !Plugged) return;
            unpluggedActions++;
            Say("Cuidado! Você mexeu no PC com ele ligado na tomada. Desconecte antes de manipular peças.");
        }

        // ---------- Estado dos defeitos ----------

        private bool ShowsSign(ActiveDefect d)
        {
            switch (d.data.type)
            {
                case DefectType.FailedPart: return true;
                case DefectType.Unseated:   return !IsResolved(d);
                case DefectType.Dust:       return !d.cleaned;
            }
            return false;
        }

        private bool IsResolved(ActiveDefect d)
        {
            switch (d.data.type)
            {
                case DefectType.FailedPart: return d.part.CurrentSlot == null;                    
                case DefectType.Unseated:   return d.part.CurrentSlot != null && d.part.InstallCount >= 2; 
                case DefectType.Dust:       return d.cleaned;
            }
            return true;
        }

        // ---------- Fim e pontuação ----------

        private void Finish(bool timedOut)
        {
            Finished = true;

            var build = BuildManager.Instance.Build;
            var report = BuildManager.Instance.Report;
            bool bootable = CompatibilityChecker.MissingParts(build).Length == 0 && !report.HasErrors;

            int total = defects.Count;
            int resolved = defects.Count(IsResolved);
            bool success = !timedOut && bootable && resolved == total;

            int spent = BudgetManager.Instance.Spent;
            int healthyReplaced = initialParts.Count(p => p.CurrentSlot == null &&
                !defects.Any(d => d.part == p && d.data.type == DefectType.FailedPart));

            var r = new EvaluationResult
            {
                missionId = Case.id, missionTitle = Case.title,
                mode = GameMode.Maintenance, difficulty = Case.difficulty,
                bootable = bootable, timedOut = timedOut,
                budget = Case.budget, spent = spent, buildPrice = build.TotalPrice, elapsedSeconds = Elapsed
            };

            if (!timedOut && bootable)
                r.compatibilityScore = total == 0 ? MaxEfficacy : Mathf.RoundToInt(MaxEfficacy * (float)resolved / total);

            if (success)
            {
                r.requirementsScore = Mathf.Max(0, MaxCare - unpluggedActions * 100 - healthyReplaced * 50);
                r.budgetScore = Mathf.RoundToInt(MaxCost * Mathf.Clamp01(1f - (float)spent / Mathf.Max(1, Case.budget)));
                r.timeScore = Mathf.RoundToInt(MaxTime * Mathf.Clamp01(1f - Elapsed / Case.timeLimitSeconds));
            }

            r.headline = timedOut ? "Tempo esgotado: serviço não concluído"
                       : success ? "Serviço concluído!" : "Serviço incompleto";

            var lines = r.detailLines;
            lines.Add($"Caso: {Case.title}");
            lines.Add($"Tempo usado: {Fmt(Elapsed)} de {Fmt(Case.timeLimitSeconds)}");
            lines.Add($"Gasto com peças: R$ {spent:N0} de R$ {Case.budget:N0}");
            lines.Add("");
            lines.Add($"Defeitos resolvidos: {resolved} de {total}");
            lines.Add($"Eficácia: {r.compatibilityScore} / {MaxEfficacy}");
            lines.Add($"Cuidados com o PC: {r.requirementsScore} / {MaxCare}");
            if (unpluggedActions > 0) lines.Add($"   - mexeu com o PC ligado na tomada: {unpluggedActions}x (-100 cada)");
            if (healthyReplaced > 0) lines.Add($"   - trocou peças sem defeito: {healthyReplaced} (-50 cada)");
            lines.Add($"Custo: {r.budgetScore} / {MaxCost}");
            lines.Add($"Tempo: {r.timeScore} / {MaxTime}");
            if (!success && !timedOut) lines.Add("(Cuidados, custo e tempo só pontuam com o serviço concluído.)");

            var open = defects.Where(d => !IsResolved(d)).ToList();
            if (open.Count > 0)
            {
                lines.Add("");
                lines.Add("Defeitos que ficaram sem solução:");
                foreach (var d in open)
                    lines.Add($"   - {CategoryLabels.Of(d.data.category)}: {TypeLabel(d.data.type)}");
            }

            MissionSession.LastResult = r;

            if (SaveManager.Instance != null && r.Total > 0)
            {
                r.isNewRecord = SaveManager.Instance.AddRecord(new RecordEntry
                {
                    mode = GameMode.Maintenance.ToString(),
                    difficulty = Case.difficulty.ToString(),
                    missionId = Case.id,
                    score = r.Total,
                    timeSeconds = Elapsed,
                    budgetSpent = spent,
                    date = DateTime.Now.ToString("yyyy-MM-dd")
                });
                SaveManager.Instance.MarkMissionCompleted(Case.id);
            }

            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(SceneNames.Evaluation);
            else SceneManager.LoadScene(SceneNames.Evaluation);
        }

        private static string TypeLabel(DefectType t) => t switch
        {
            DefectType.FailedPart => "peça com defeito (era preciso trocar)",
            DefectType.Unseated   => "peça mal encaixada (era preciso reencaixar)",
            DefectType.Dust       => "sujeira (era preciso limpar)",
            _                     => t.ToString()
        };

        private static string Fmt(float seconds) => $"{(int)seconds / 60:00}:{(int)seconds % 60:00}";

        private static void Say(string message)
        {
            if (WorkshopHUD.Instance != null) WorkshopHUD.Instance.ShowMessage(message);
        }
    }
}
