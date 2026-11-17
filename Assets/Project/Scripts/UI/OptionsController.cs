using MestreDoPC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class OptionsController : MonoBehaviour
    {

        [SerializeField] private GameObject OptionsPanel;


        //public bool active = true;

        void Start()
        {
            ShowOptions();
#if UNITY_WEBGL && !UNITY_EDITOR
            
#endif
        }

        private void Show(GameObject panel)
        {
            OptionsPanel.SetActive(panel == OptionsPanel);
        }

        public void ShowOptions() => Show(OptionsPanel);

        public void OnMainMenu() => SceneLoader.Instance.Load(SceneNames.MainMenu);
    }
}