
using UnityEngine;
using UnityEngine.EventSystems;

namespace MestreDoPC
{
    public class ModelRotation : MonoBehaviour,
        IPointerDownHandler, IDragHandler
    {
        [Header("Configuração da Rotação")]
        [SerializeField] private Transform model;
        [SerializeField] private float rotationSpeed = 0.5f;

        public void SetModel(Transform newModel)
        {
            model = newModel;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("Clique detectado na área do modelo.");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (model == null)
            {
                return;
            }

            float rotationX = eventData.delta.y * rotationSpeed;
            float rotationY = -eventData.delta.x * rotationSpeed;

            model.Rotate(Vector3.up, rotationY, Space.World);
            model.Rotate(Vector3.right, rotationX, Space.World);
        }
    }
}
