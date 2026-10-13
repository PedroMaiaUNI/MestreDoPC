using UnityEngine;

namespace MestreDoPC{
    [CreateAssetMenu(menuName = "MestreDoPC/Components/PSU")]
    public class PsuDataSO : ComponentDataSO
    {
        public int wattage;
        public string certification;

        public override ComponentCategory Category => ComponentCategory.Psu;
        public override string SpecsSummary => $"{wattage} W • {certification}";
    }
}
