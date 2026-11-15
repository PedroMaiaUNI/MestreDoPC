using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class PartSlot : MonoBehaviour
    {
        private static readonly List<PartSlot> all = new List<PartSlot>();
        public static IReadOnlyList<PartSlot> All => all;

        public ComponentCategory category;
        public bool requiresMotherboard;          // CPU, cooler, RAM, GPU e M.2 só encaixam com a placa-mãe instalada
        public Transform snapPoint;               // vazio = usa o próprio transform
        public Material hologramMaterial;

        [Header("Só para o slot de armazenamento")]
        [Tooltip("Onde encaixam HD e SSD SATA (baia). O snapPoint normal passa a ser o M.2 (NVMe), sobre a placa-mãe.")]
        public Transform sataSnapPoint;

        public PartView Installed { get; private set; }

        private GameObject hologram;       // slot principal (no armazenamento: o M.2)
        private GameObject sataHologram;   // só existe se sataSnapPoint estiver preenchido

        private void OnEnable() => all.Add(this);
        private void OnDisable() => all.Remove(this);

        private void Awake()
        {
            if (snapPoint == null) snapPoint = transform;

            hologram = MakeHologram(snapPoint);
            if (category == ComponentCategory.Storage && sataSnapPoint != null)
            {
                hologram.transform.localScale = PartFactory.NvmeScale;   // o M.2 é menor que a baia
                sataHologram = MakeHologram(sataSnapPoint);
            }
        }

        private GameObject MakeHologram(Transform parent)
        {
            var h = PartFactory.CreateVisual(category);
            h.name = "Hologram";
            foreach (var c in h.GetComponents<Collider>()) Destroy(c);
            if (hologramMaterial != null)
                h.GetComponent<Renderer>().sharedMaterial = hologramMaterial;

            h.transform.SetParent(parent, false);
            h.transform.localPosition = Vector3.zero;
            h.transform.localRotation = Quaternion.identity;
            h.SetActive(false);
            return h;
        }

        private static bool IsSata(PartView part)
            => part != null && part.Data is StorageDataSO s && s.type != StorageType.NVMe;

        // Onde a peça encaixa: NVMe no M.2 (placa-mãe); SATA na baia.
        public Transform SnapFor(PartView part)
            => sataSnapPoint != null && IsSata(part) ? sataSnapPoint : snapPoint;

        // NVMe fica na placa-mãe e depende dela; SATA fica na baia e não.
        public bool RequiresMotherboardFor(PartView part)
            => requiresMotherboard && !(sataSnapPoint != null && IsSata(part));

        public void SetHighlight(bool on, PartView part = null)
        {
            bool show = on && Installed == null && DifficultyRules.ShowHolograms;
            bool sata = sataSnapPoint != null && IsSata(part);
            hologram.SetActive(show && !sata);
            if (sataHologram != null) sataHologram.SetActive(show && sata);
        }

        public void Place(PartView part)
        {
            Installed = part;
            part.CurrentSlot = this;
            part.InstallCount++;

            var t = SnapFor(part);
            part.transform.SetPositionAndRotation(t.position, t.rotation);

            hologram.SetActive(false);
            if (sataHologram != null) sataHologram.SetActive(false);
        }

        public void Clear()
        {
            if (Installed == null) return;
            Installed.CurrentSlot = null;
            Installed = null;
        }
    }
}
