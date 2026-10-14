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
        [SerializeField] private GameObject window;
        [SerializeField] private Transform tabContainer;      
        [SerializeField] private Transform itemContainer;    
        [SerializeField] private Button tabButtonPrefab;
        [SerializeField] private ShopItemCard cardPrefab;
        [SerializeField] private TMP_Text emptyText;        

        private static readonly Color TabNormal = new Color(0.25f, 0.27f, 0.33f);
        private static readonly Color TabSelected = new Color(0.20f, 0.55f, 0.95f);

        private readonly List<ShopItemCard> cards = new List<ShopItemCard>();
        private readonly Dictionary<ComponentCategory, Button> tabs = new Dictionary<ComponentCategory, Button>();

        private void Start()
        {
            if (!ValidateReferences()) return; 

            foreach (ComponentCategory c in Enum.GetValues(typeof(ComponentCategory)))
            {
                var category = c; 
                var tab = Instantiate(tabButtonPrefab, tabContainer);
                tab.name = "Tab_" + category;
                tab.GetComponentInChildren<TMP_Text>().text = CategoryLabels.Of(category);
                tab.onClick.AddListener(() => ShowCategory(category));
                tabs[category] = tab;
            }

            BudgetManager.Instance.OnChanged += RefreshAffordability;
            ShowCategory(ComponentCategory.Motherboard);
            window.SetActive(false);
        }

        private void OnDestroy()
        {
            if (BudgetManager.Instance != null) BudgetManager.Instance.OnChanged -= RefreshAffordability;
        }

        public void Toggle()
        {
            window.SetActive(!window.activeSelf);
            Debug.Log($"[Loja] Janela {(window.activeSelf ? "ABERTA" : "FECHADA")}");
        }

        private bool ValidateReferences()
        {
            bool ok = true;

            void Check(UnityEngine.Object field, string fieldName)
            {
                if (field != null) return;
                Debug.LogError($"ShopPanel: o campo '{fieldName}' não está atribuído no Inspector.", this);
                ok = false;
            }

            Check(database, "Database");
            Check(window, "Window");
            Check(tabContainer, "Tab Container");
            Check(itemContainer, "Item Container");
            Check(tabButtonPrefab, "Tab Button Prefab");
            Check(cardPrefab, "Card Prefab");

            if (BudgetManager.Instance == null)
            {
                Debug.LogError("ShopPanel: não existe BudgetManager na cena. Adicione o componente ao objeto [Workshop].", this);
                ok = false;
            }
            if (TrayManager.Instance == null)
            {
                Debug.LogError("ShopPanel: não existe TrayManager na cena. Adicione o componente ao objeto [Workshop].", this);
                ok = false;
            }
            return ok;
        }

        private void ShowCategory(ComponentCategory category)
        {
            foreach (var c in cards) if (c != null) Destroy(c.gameObject);
            cards.Clear();

            foreach (var kv in tabs)
                kv.Value.image.color = kv.Key == category ? TabSelected : TabNormal;

            var items = database.GetByCategory(category).OrderBy(d => d.price).ToList();
            foreach (var data in items)
            {
                var card = Instantiate(cardPrefab, itemContainer);
                card.Setup(data, OnBuy);
                cards.Add(card);
            }

            if (emptyText != null) emptyText.gameObject.SetActive(items.Count == 0);
            RefreshAffordability();
        }

        private void RefreshAffordability()
        {
            foreach (var c in cards)
                if (c != null) c.Refresh(BudgetManager.Instance.CanAfford(c.Data.price));
        }

        private void OnBuy(ComponentDataSO data)
        {
            if (!BudgetManager.Instance.CanAfford(data.price))
            {
                Say("Orçamento insuficiente.");
                return;
            }

            var part = TrayManager.Instance.Spawn(data);   // se falhar, não debita
            if (part == null)
            {
                Say("Não foi possível criar a peça (veja o Console).");
                return;
            }

            BudgetManager.Instance.TryPurchase(data.price);
            Say($"{data.DisplayName} comprado. Pegue na bandeja.");
        }

        private static void Say(string message)
        {
            if (WorkshopHUD.Instance != null) WorkshopHUD.Instance.ShowMessage(message);
        }

        [ContextMenu("Debug: comprar o 1º item da aba atual")]
        private void DebugBuyFirst()
        {
            if (cards.Count == 0) { Debug.LogWarning("Nenhum card visível (dê Play antes)."); return; }
            OnBuy(cards[0].Data);
        }
    }
}
