using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    private bool isPaused;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private void OnEnable()
    {
        if (GameInput.Instance != null)
            GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void OnDisable()
    {
        if (GameInput.Instance != null)
            GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void Start()
    {
        resumeButton.onClick.AddListener(Resume);
        mainMenuButton.onClick.AddListener(GoToMainMenu);

        ActivateChildren(false);
    }

    private void GameInput_OnPauseAction(object sender, System.EventArgs e)
    {
        TogglePause();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        ActivateChildren(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void ActivateChildren(bool active)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }

    private void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        ActivateChildren(false);
        Time.timeScale = 1f;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f; // importante antes de cambiar escena
        SceneManager.LoadScene("Scenes/MainMenu"); // asegúrate que coincide
    }
}
