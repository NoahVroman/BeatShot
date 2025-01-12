using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public enum GameState
{
    StartScreen,
    Playing,
    Paused,
    Settings
}

public enum StartMode
{
    StartScreen,
    Playing
}

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject hud;
    public GameObject statsHUD;
    public AudioSource backgroundMusic;
    public GameObject startScreen;
    public TMP_Dropdown songDropdown;
    public Button playButton;

    public RhythmReplayer rhythmReplayer;


    private bool isPaused = false;
    private GameState currentState = GameState.StartScreen; // Track the current game state
    private bool openedFromPauseMenu = false; // Track if settings menu was opened from the pause menu

    public StartMode initialGameMode = StartMode.StartScreen; // Choose StartScreen or Playing as the initial mode

    void Start()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playButton.onClick.AddListener(OnPlayButtonPressed);
        PopulateSongDropdown();
        startScreen.SetActive(true);

        songDropdown.onValueChanged.AddListener(OnSongSelectionChanged);

        if (initialGameMode == StartMode.Playing)
        {
            OnPlayButtonPressed(); // Start directly in the Playing mode
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (currentState)
            {
                case GameState.StartScreen:
                    break;

                case GameState.Playing:
                    PauseGame();
                    break;

                case GameState.Paused:
                    if (settingsMenu.activeSelf)
                    {
                        OpenPauseMenu();
                    }
                    else
                    {
                        ResumeGame();
                    }
                    break;

                case GameState.Settings:
                    if (openedFromPauseMenu)
                    {
                        OpenPauseMenu();
                    }
                    else
                    {
                        OpenStartScreen();
                    }
                    break;
            }
        }

    }

    void PopulateSongDropdown()
    {
        songDropdown.ClearOptions();
        songDropdown.AddOptions(System.Enum.GetNames(typeof(RhythmReplayer.SongSelection)).ToList());
    }

    private void OnSongSelectionChanged(int index)
    {
        RhythmReplayer.SongSelection selectedSong = (RhythmReplayer.SongSelection)index;
        rhythmReplayer.SetSongSelection(selectedSong);
    }

    public void OnPlayButtonPressed()
    {
        int selectedIndex = songDropdown.value;
        RhythmReplayer.SongSelection selectedSong = (RhythmReplayer.SongSelection)selectedIndex;
        rhythmReplayer.SetSongSelection(selectedSong);

        startScreen.SetActive(false);
        hud.SetActive(true);
        statsHUD.SetActive(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentState = GameState.Playing; // Update state to Playing
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        hud.SetActive(false);

        if (backgroundMusic != null)
        {
            backgroundMusic.Pause();
        }

        currentState = GameState.Paused; // Update state to Paused
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        hud.SetActive(true);
        statsHUD.SetActive(true);

        if (backgroundMusic != null)
        {
            backgroundMusic.UnPause();
        }

        currentState = GameState.Playing; // Update state to Playing
    }

    public void OpenSettingsMenuFromPause()
    {
        openedFromPauseMenu = true; // Settings were opened from pause menu
        hud.SetActive(true);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);

        if (statsHUD != null)
        {
            statsHUD.SetActive(false);
        }

        currentState = GameState.Settings; // Update state to Settings
    }

    public void OpenSettingsMenuFromStartScreen()
    {
        openedFromPauseMenu = false; // Settings were opened from start screen
        hud.SetActive(true);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);

        if (statsHUD != null)
        {
            statsHUD.SetActive(false);
        }

        currentState = GameState.Settings; // Update state to Settings
    }

    public void OpenPauseMenu()
    {
        hud.SetActive(false);
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);

        if (statsHUD != null)
        {
            statsHUD.SetActive(true);
        }

        currentState = GameState.Paused; // Update state to Paused
    }

    public void OpenStartScreen()
    {
        startScreen.SetActive(true);
        hud.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        statsHUD.SetActive(false);

        currentState = GameState.StartScreen; // Update state to StartScreen
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
