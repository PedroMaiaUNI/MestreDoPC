using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class PartSlot : MonoBehaviour
    {
        private static readonly List<PartSlot> all = new List<PartSlot>();
        public static IReadOnlyList<PartSlot> All => all;

        public ComponentCategory category;
        public bool requiresMotherboard;          // CPU, cooler, RAM e GPU só encaixam com a placa-mãe instalada
        public Transform snapPoint;               // vazio = usa o próprio transform
        public Material hologramMaterial;

        public PartView Installed { get; private set; }

        private GameObject hologram;

        private void OnEnable() => all.Add(this);
        private void OnDisable() => all.Remove(this);

        private void Awake()
        {
            if (snapPoint == null) snapPoint = transform;

            hologram = PartFactory.CreateVisual(category);
            hologram.name = "Hologram";
            foreach (var c in hologram.GetComponents<Collider>()) Destroy(c);
            if (hologramMaterial != null)
                hologram.GetComponent<Renderer>().sharedMaterial = hologramMaterial;

            hologram.transform.SetParent(snapPoint, false);
            hologram.transform.localPosition = Vector3.zero;
            hologram.transform.localRotation = Quaternion.identity;
            hologram.SetActive(false);
        }

        public void SetHighlight(bool on)
            => hologram.SetActive(on && Installed == null && DifficultyRules.ShowHolograms);

        public void Place(PartView part)
        {
            Installed = part;
            part.CurrentSlot = this;
            part.InstallCount++;
            part.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
            hologram.SetActive(false);
        }

        public void Clear()
        {
            if (Installed == null) return;
            Installed.CurrentSlot = null;
            Installed = null;
        }
    }
}
