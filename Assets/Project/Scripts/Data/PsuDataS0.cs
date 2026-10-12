using UnityEngine;

namespace MestreDoPC{
    [CreateAssetMenu(menuName = "MestreDoPC/Components/PSU")]
    public class PsuDataSO : ComponentDataSO
    {
        public int wattage;
        public string certification; // ex.: "80 Plus Bronze"
        public override ComponentCategory Category => ComponentCategory.Psu;
    }
}
