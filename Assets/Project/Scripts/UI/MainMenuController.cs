using UnityEngine;

namespace MestreDoPC
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject modePanel;
        [SerializeField] private GameObject difficultyPanel;
        [SerializeField] private GameObject quitButton;

        private void Start()
        {
            ShowMain();
#if UNITY_WEBGL && !UNITY_EDITOR
            // Application.Quit() não faz nada no navegador: esconda o botão.
            quitButton.SetActive(false);
#endif
        }

        private void Show(GameObject panel)
        {
            mainPanel.SetActive(panel == mainPanel);
            modePanel.SetActive(panel == modePanel);
            difficultyPanel.SetActive(panel == difficultyPanel);
        }

        // --- Ligar no OnClick dos botões ---
        public void ShowMain() => Show(mainPanel);
        public void ShowModes() => Show(modePanel);

        public void OnModeSelected(int mode) // 0=Assembly, 1=AIChallenge, 2=Maintenance
        {
            GameManager.Instance.CurrentMode = (GameMode)mode;
            Show(difficultyPanel);
        }

        public void OnDifficultySelected(int difficulty) // 0=Easy, 1=Medium, 2=Hard
        {
            GameManager.Instance.CurrentDifficulty = (Difficulty)difficulty;
            GameManager.Instance.SetState(GameState.Playing);

            // Por enquanto todos os modos levam à Workshop; troque conforme as cenas forem criadas.
            SceneLoader.Instance.Load(SceneNames.Workshop);
        }

        public void OnPartsManual() => SceneLoader.Instance.Load(SceneNames.PartsManual);
        public void OnRecords() => SceneLoader.Instance.Load(SceneNames.Records);
        public void OnOptions() => SceneLoader.Instance.Load(SceneNames.Options);
        public void OnQuit() => Application.Quit();
    }
}
