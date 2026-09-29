using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MestreDoPC
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private CanvasGroup loadingScreen;
        [SerializeField] private Slider progressBar;
        [SerializeField] private float fadeDuration = 0.25f;

        private bool isLoading;

        public void Load(string sceneName)
        {
            if (!isLoading) StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            isLoading = true;
            yield return Fade(0f, 1f);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
            {
                // O progress do Unity vai só até 0.9 antes de ativar a cena.
                progressBar.value = Mathf.Clamp01(op.progress / 0.9f);
                yield return null;
            }

            yield return Fade(1f, 0f);
            isLoading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            loadingScreen.gameObject.SetActive(true);
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime; // unscaled: funciona mesmo com o jogo pausado
                loadingScreen.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            loadingScreen.alpha = to;
            if (to == 0f) loadingScreen.gameObject.SetActive(false);
        }
    }
}
