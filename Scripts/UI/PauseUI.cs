using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseUI : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pausePanel;

    private void Start()
    {
        pausePanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnPauseStateChanged += HandlePauseStateChanged;
        }
    }

    private void OnDisable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnPauseStateChanged -= HandlePauseStateChanged;
        }
    }

    private void HandlePauseStateChanged(bool isPaused)
    {
        if (isPaused)
        {
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        pausePanel.SetActive(true);
    }

    private void HidePauseMenu()
    {
        pausePanel.SetActive(false);
    }

    public void OnResumeClick()
    {
        HidePauseMenu();
        ViewManager.Instance.Unpause();
    }

    public void OnMainMenuClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

}