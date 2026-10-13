using System;
using UnityEngine;

namespace MestreDoPC
{
    public class BudgetManager : Singleton<BudgetManager>
    {
        [SerializeField] private int debugBudget = 4000; // só vale até a Etapa 7 ligar a missão

        public int Total { get; private set; }
        public int Spent { get; private set; }
        public int Remaining => Total - Spent;

        public event Action OnChanged;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this) Begin(debugBudget);
        }

        public void Begin(int total)
        {
            Total = total;
            Spent = 0;
            OnChanged?.Invoke();
        }

        public bool CanAfford(int price) => price <= Remaining;

        public bool TryPurchase(int price)
        {
            if (!CanAfford(price)) return false;
            Spent += price;
            OnChanged?.Invoke();
            return true;
        }
    }
}
