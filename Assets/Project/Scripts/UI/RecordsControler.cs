using MestreDoPC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class RecordsController : MonoBehaviour
    {

        [SerializeField] private GameObject RecordsPanel;


        //public bool active = true;

        void Start()
        {
            ShowRecords();
#if UNITY_WEBGL && !UNITY_EDITOR
            
#endif
        }

        private void Show(GameObject panel)
        {
            RecordsPanel.SetActive(panel == RecordsPanel);
        }

        public void ShowRecords() => Show(RecordsPanel);

        public void OnMainMenu() => SceneLoader.Instance.Load(SceneNames.MainMenu);
    }
}