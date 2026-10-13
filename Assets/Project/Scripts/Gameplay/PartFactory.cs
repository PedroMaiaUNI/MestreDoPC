using UnityEngine;

namespace MestreDoPC
{
    public static class PartFactory
    {
        // Visual placeholder por categoria (usado pelas peças E pelos hologramas dos slots).
        public static GameObject CreateVisual(ComponentCategory category)
        {
            var type = category == ComponentCategory.Cooler ? PrimitiveType.Cylinder : PrimitiveType.Cube;
            var go = GameObject.CreatePrimitive(type);
            go.transform.localScale = ScaleOf(category);
            go.GetComponent<Renderer>().material.color = ColorOf(category);
            return go;
        }

        public static PartView Create(ComponentDataSO data, Vector3 position, Quaternion rotation)
        {
            GameObject go;
            if (data.prefab != null)
            {
                go = Object.Instantiate(data.prefab, position, rotation); // asset real, quando existir
            }
            else
            {
                go = CreateVisual(data.Category);
                go.transform.SetPositionAndRotation(position, rotation);
            }

            go.name = data.DisplayName;

            if (go.GetComponentInChildren<Collider>() == null)
            {
                Debug.LogWarning($"{go.name}: prefab sem Collider, adicionando BoxCollider padrão.");
                go.AddComponent<BoxCollider>();
            }

            var view = go.AddComponent<PartView>();
            view.Init(data, position, rotation);
            return view;
        }

        private static Vector3 ScaleOf(ComponentCategory c) => c switch
        {
            ComponentCategory.Motherboard => new Vector3(1.0f, 0.05f, 0.8f),
            ComponentCategory.Cpu         => new Vector3(0.2f, 0.04f, 0.2f),
            ComponentCategory.Ram         => new Vector3(0.08f, 0.04f, 0.35f),
            ComponentCategory.Storage     => new Vector3(0.3f, 0.05f, 0.2f),
            ComponentCategory.Gpu         => new Vector3(0.8f, 0.1f, 0.25f),
            ComponentCategory.Psu         => new Vector3(0.5f, 0.3f, 0.4f),
            ComponentCategory.Cooler      => new Vector3(0.3f, 0.05f, 0.3f), // cilindro: altura = 2 × scale.y
            _                             => Vector3.one * 0.2f
        };

        private static Color ColorOf(ComponentCategory c) => c switch
        {
            ComponentCategory.Motherboard => new Color(0.10f, 0.50f, 0.20f),
            ComponentCategory.Cpu         => new Color(0.60f, 0.60f, 0.65f),
            ComponentCategory.Ram         => new Color(0.20f, 0.40f, 0.90f),
            ComponentCategory.Storage     => new Color(0.95f, 0.60f, 0.15f),
            ComponentCategory.Gpu         => new Color(0.75f, 0.15f, 0.55f),
            ComponentCategory.Psu         => new Color(0.15f, 0.15f, 0.15f),
            ComponentCategory.Cooler      => new Color(0.20f, 0.80f, 0.85f),
            _                             => Color.white
        };
    }
}
