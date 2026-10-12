using UnityEngine;

namespace MestreDoPC
{
    public abstract class ComponentDataSO : ScriptableObject
    {
        [Header("Identificação")]
        public string id;
        public string brand;
        public string model;
        public int price; // em reais (inteiro simplifica a soma de orçamento)

        [Header("Apresentação")]
        [TextArea] public string description; // usada nas explicações do Nível 1 e no Manual
        public Sprite icon;
        public GameObject prefab;

        public abstract ComponentCategory Category { get; }
        public string DisplayName => $"{brand} {model}";

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id)) id = name;
        }
    }
}
