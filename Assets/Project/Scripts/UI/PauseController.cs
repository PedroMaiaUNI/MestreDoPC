using UnityEngine;
using UnityEngine.SceneManagement;

namespace MestreDoPC
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private KeyCode key = KeyCode.P;

        public bool Paused { get; private set; }

        private void Update()
        {
            if (Input.GetKeyDown(key) || Input.GetKeyDown(KeyCode.Escape)) SetPaused(!Paused);
        }

        // Aba do navegador em segundo plano: pausa sozinho (no WebGL costuma funcionar).
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && !Paused) SetPaused(true);
        }

        private void OnDestroy() => Time.timeScale = 1f;   // nunca sai de uma cena com o jogo congelado

        public void SetPaused(bool paused)
        {
            Paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            pausePanel.SetActive(paused);
        }

        // Ligar nos botões (OnClick, sem parâmetro).
        public void OnResume() => SetPaused(false);

        public void OnMainMenu()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(SceneNames.MainMenu);
            else SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
