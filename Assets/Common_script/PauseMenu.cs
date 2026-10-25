using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject menuPanel;           // the panel that holds the three buttons
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Disable While Paused")]
    public Behaviour[] disableWhilePaused; // e.g. your camera look script

    private bool isPaused;

    void Start()
    {
        SetPaused(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
            SetPaused(!isPaused);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;

        if (menuPanel != null)
            menuPanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        foreach (Behaviour b in disableWhilePaused)
        {
            if (b != null) b.enabled = !paused;
        }
    }

    // Hook these up to the buttons' OnClick events
    public void Resume()
    {
        SetPaused(false);
    }

    public void ResetGame()
    {
        Time.timeScale = 1f; // must be restored, or the reloaded scene stays frozen
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}