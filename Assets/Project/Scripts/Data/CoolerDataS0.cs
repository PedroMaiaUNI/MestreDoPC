using UnityEngine;

namespace MestreDoPC{
    [CreateAssetMenu(menuName = "MestreDoPC/Components/Cooler")]
    public class CoolerDataSO : ComponentDataSO
    {
        public CoolerType type;
        public SocketType[] supportedSockets;
        public int tdpRating;

        public override ComponentCategory Category => ComponentCategory.Cooler;
        public override string SpecsSummary => $"{type} • até {tdpRating} W • " + string.Join("/", supportedSockets ?? System.Array.Empty<SocketType>());
    }
}
