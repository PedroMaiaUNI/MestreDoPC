using TMPro;
using UnityEngine;

namespace MestreDoPC
{
    public class BudgetHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text budgetText;

        private void Start()
        {
            BudgetManager.Instance.OnChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (BudgetManager.Instance != null) BudgetManager.Instance.OnChanged -= Refresh;
        }

        private void Refresh()
        {
            var b = BudgetManager.Instance;
            budgetText.text = $"Saldo: R$ {b.Remaining:N0}   (gasto R$ {b.Spent:N0} de R$ {b.Total:N0})";
        }
    }
}
