using UnityEngine;

namespace MestreDoPC{
    [CreateAssetMenu(menuName = "MestreDoPC/Components/Cooler")]
    public class CoolerDataSO : ComponentDataSO
    {
        public CoolerType type;
        public SocketType[] supportedSockets;
        public int tdpRating; // TDP máximo que dissipa
        public override ComponentCategory Category => ComponentCategory.Cooler;
    }
}
