using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MestreDoPC
{
    public class ShopItemCard : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text iconLabel;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text specsText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;

        public ComponentDataSO Data { get; private set; }

        public void Setup(ComponentDataSO data, Action<ComponentDataSO> onBuy)
        {
            Data = data;
            nameText.text = data.DisplayName;
            specsText.text = data.SpecsSummary;
            priceText.text = $"R$ {data.price:N0}";

            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = Color.white;
                iconLabel.gameObject.SetActive(false);
            }
            else
            {
                // Placeholder: quadrado colorido + sigla da categoria
                iconImage.sprite = null;
                iconImage.color = PartFactory.ColorOf(data.Category);
                iconLabel.text = CategoryLabels.Short(data.Category);
                iconLabel.gameObject.SetActive(true);
            }

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy(data));
        }

        public void Refresh(bool canAfford) => buyButton.interactable = canAfford;
    }
}
