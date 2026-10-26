using UnityEngine;

public class StartMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject startPanel;      // the panel with the title and buttons

    [Header("Options")]
    public bool showAfterReset = false; // false = skip this screen when Reset reloads the scene

    // Other scripts (like PauseMenu) check this
    public static bool IsOpen { get; private set; }

    // Survives scene reloads, so Reset doesn't show the title again
    private static bool hasShownOnce = false;

    private camera lookScript;

    void Awake()
    {
        // Awake runs before every Start, so PauseMenu sees the right value
        IsOpen = showAfterReset || !hasShownOnce;
    }

    void Start()
    {
        lookScript = FindFirstObjectByType<camera>();

        if (IsOpen)
            Open();
        else if (startPanel != null)
            startPanel.SetActive(false);
    }

    void Open()
    {
        hasShownOnce = true;
        if (startPanel != null) startPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (lookScript != null) lookScript.enabled = false;
    }

    // Hook this to the Play button's OnClick
    public void StartGame()
    {
        IsOpen = false;
        if (startPanel != null) startPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (lookScript != null) lookScript.enabled = true;
    }

    // Hook this to the Quit button's OnClick
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}