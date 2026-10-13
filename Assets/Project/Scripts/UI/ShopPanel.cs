using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MestreDoPC
{
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private ComponentDatabaseSO database;
        [SerializeField] private GameObject window;           // janela que abre/fecha
        [SerializeField] private Transform tabContainer;
        [SerializeField] private Transform itemContainer;     // Content do ScrollView
        [SerializeField] private Button tabButtonPrefab;
        [SerializeField] private ShopItemCard cardPrefab;

        private readonly List<ShopItemCard> cards = new List<ShopItemCard>();

        private void Start()
        {
            foreach (ComponentCategory c in Enum.GetValues(typeof(ComponentCategory)))
            {
                var category = c; // captura correta para o lambda
                var tab = Instantiate(tabButtonPrefab, tabContainer);
                tab.GetComponentInChildren<TMP_Text>().text = CategoryLabels.Of(category);
                tab.onClick.AddListener(() => ShowCategory(category));
            }

            BudgetManager.Instance.OnChanged += RefreshAffordability;
            ShowCategory(ComponentCategory.Motherboard);
            window.SetActive(false);
        }

        private void OnDestroy()
        {
            if (BudgetManager.Instance != null) BudgetManager.Instance.OnChanged -= RefreshAffordability;
        }

        public void Toggle() => window.SetActive(!window.activeSelf);

        private void ShowCategory(ComponentCategory category)
        {
            foreach (var c in cards) if (c != null) Destroy(c.gameObject);
            cards.Clear();

            foreach (var data in database.GetByCategory(category).OrderBy(d => d.price))
            {
                var card = Instantiate(cardPrefab, itemContainer);
                card.Setup(data, OnBuy);
                cards.Add(card);
            }
            RefreshAffordability();
        }

        private void RefreshAffordability()
        {
            foreach (var c in cards)
                if (c != null) c.Refresh(BudgetManager.Instance.CanAfford(c.Data.price));
        }

        private void OnBuy(ComponentDataSO data)
        {
            if (!BudgetManager.Instance.TryPurchase(data.price))
            {
                Say("Orçamento insuficiente.");
                return;
            }
            TrayManager.Instance.Spawn(data);
            Say($"{data.DisplayName} comprado. Pegue na bandeja.");
        }

        private static void Say(string message)
        {
            if (WorkshopHUD.Instance != null) WorkshopHUD.Instance.ShowMessage(message);
        }
    }
}
