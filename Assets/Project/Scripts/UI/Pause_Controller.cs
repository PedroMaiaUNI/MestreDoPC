using UnityEditor;
using UnityEngine;

public class UI_Controller : MonoBehaviour
{

    public GameObject pauseMenu;

    public bool paused = false;

    public GameObject options;

    public bool active = true;

    private void Start()
    {
        HidePauseMenu();
        HideOptions();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
                HidePauseMenu();
            else
                ShowPauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        paused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    private void HidePauseMenu()
    {
        paused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ShowOptions()
    {
        active = true;
        options.SetActive(true);
    }

    private void HideOptions()
    {
        paused = false;
        options.SetActive(false);
    }

    public void BTN_resume()
    {
        HidePauseMenu();
    }

    public void BTN_Opcoes()
    {
        ShowOptions();
    }

    public void BTN_Voltar()
    {
        HideOptions();
    }

    public void BTN_Quit()
    {
        Application.Quit();



#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }

}