using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [Tooltip("Scene name")]
    [SerializeField] private string gameSceneName = "Tutorial";

    [Header("UI")]
    [Tooltip("Main panel")]
    [SerializeField] private GameObject mainPanel;

    [Tooltip("Options panel")]
    [SerializeField] private GameObject optionsPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame()
    {
        //SceneManager.LoadScene(gameSceneName);
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
