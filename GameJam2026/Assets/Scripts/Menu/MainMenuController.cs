using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private int gameSceneIndex = 1;

    [Header("UI")]
    [Tooltip("Main panel")]
    [SerializeField] private GameObject mainPanel;

    [Tooltip("Options panel")]
    [SerializeField] private GameObject optionsPanel;

    public void NewGame()
    {
        SceneController.Instance.TransitionAndLoadScene(gameSceneIndex, true, true);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
