using UnityEngine;

namespace MestreDoPC
{
    public enum PartFeedback { None, Warning, Error }

    public class PartView : MonoBehaviour
    {
        public ComponentDataSO Data { get; private set; }
        public PartSlot CurrentSlot { get; set; }
        public Vector3 HomePosition { get; private set; }
        public Quaternion HomeRotation { get; private set; }
        public int PurchasePrice { get; set; } = -1;     //(-1 = peça que não foi comprada)
        public bool IsBundled { get; set; }
        public PartView BundledChild { get; set; }
        public bool Sellable => PurchasePrice >= 0 && !IsBundled;


        private Renderer[] renderers;
        private Color[] baseColors;

        public void Init(ComponentDataSO data, Vector3 homePosition, Quaternion homeRotation)
        {
            Data = data;
            HomePosition = homePosition;
            HomeRotation = homeRotation;

            renderers = GetComponentsInChildren<Renderer>();
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                baseColors[i] = renderers[i].material.color;
        }

        // Usado pela bandeja ao posicionar a peça definitivamente.
        public void SetHome(Vector3 position)
        {
            HomePosition = position;
            transform.position = position;
        }

        public void SendHome() => transform.SetPositionAndRotation(HomePosition, HomeRotation);

        public void SetFeedback(PartFeedback feedback)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = feedback switch
                {
                    PartFeedback.Error   => Color.red,
                    PartFeedback.Warning => Color.yellow,
                    _                    => baseColors[i]
                };
            }
        }

        public int InstallCount { get; set; }

        public void SetHomeKeepPosition(Vector3 position) => HomePosition = position;
    }
}
