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

    private void Start()
    {
        AudioManager.Instance.ChangeMusicTrack("main_menu_music", "Main Menu music", false, true);
    }

    public void NewGame()
    {
        SceneController.Instance.TransitionAndLoadScene(gameSceneIndex, true, true);
        AudioManager.Instance.PlaySFX("interface_select");
        AudioManager.Instance.ChangeMusicTrack("music_yes_mask", "Music yes", false, true);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        mainPanel.SetActive(false);
        AudioManager.Instance.PlaySFX("interface_select");
        AudioManager.Instance.ChangeMusicTrack("music_credits", "Main Menu music", false, true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
        AudioManager.Instance.PlaySFX("interface_select");
        AudioManager.Instance.ChangeMusicTrack("main_menu_music", "Main Menu music", false, true);
    }

    public void ExitGame()
    {
        AudioManager.Instance.PlaySFX("interface_select");
        Debug.Log("Exit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
