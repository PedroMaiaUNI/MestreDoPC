using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Components/Motherboard")]
    public class MotherboardDataSO : ComponentDataSO
    {
        public SocketType socket;
        public string chipset;
        public FormFactor formFactor;
        public RamType ramType;
        public int ramSlots = 2;
        public int maxRamGB = 64;

        public override ComponentCategory Category => ComponentCategory.Motherboard;
        public override string SpecsSummary => $"{socket} • {ramType} • {formFactor} • {ramSlots} slots (máx. {maxRamGB} GB)";
    }
}
