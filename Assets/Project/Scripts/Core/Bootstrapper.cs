using UnityEngine;

namespace MestreDoPC
{
    public class Bootstrapper : MonoBehaviour
    {
        private static bool initialized;

        private void Start()
        {
            if (initialized) { Destroy(gameObject); return; }
            initialized = true;
            DontDestroyOnLoad(gameObject);
            SceneLoader.Instance.Load(SceneNames.MainMenu);
        }
    }
}
