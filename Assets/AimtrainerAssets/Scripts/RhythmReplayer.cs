using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class RhythmReplayer : MonoBehaviour
{
    public enum SongSelection
    {
        Gasolina,
        Treasure,
        K
    }

    public AudioSource audioSource;
    public GameObject objectToSpawn;
    public TMP_Text countdownText;
    [SerializeField] private TMP_Dropdown songDropdown;

    public AudioClip gasolinaClip;
    public AudioClip treasureClip;
    public AudioClip KClip;

    private Dictionary<SongSelection, AudioClip> songMap; // Map enum to audio clip
    private List<float> timestamps = new List<float>();
    private int currentTimestampIndex = 0;
    private float customTime = 0f;
    public float delayBeforeStart = 3f;
    [SerializeField] private Vector3 transformToSpawnAt;

    void Start()
    {
        songMap = new Dictionary<SongSelection, AudioClip>
        {
            { SongSelection.Gasolina, gasolinaClip },
            { SongSelection.Treasure, treasureClip },
            { SongSelection.K, KClip },
        };

        PopulateDropdownWithEnum();
        songDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        OnDropdownValueChanged(songDropdown.value);
    }

    void Update()
    {
        if (timestamps != null && currentTimestampIndex < timestamps.Count)
        {
            customTime += Time.deltaTime;
            Debug.Log($"Custom Time: {customTime} Current Timestamp Time: {timestamps[currentTimestampIndex]}");
            if (customTime >= timestamps[currentTimestampIndex])
            {
                Instantiate(objectToSpawn, transformToSpawnAt + new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0), Quaternion.identity);
                currentTimestampIndex++;
            }
        }
    }

    // Populate the dropdown with song names from the SongSelection enum
    private void PopulateDropdownWithEnum()
    {
        songDropdown.ClearOptions();
        List<string> songNames = new List<string>();

        foreach (SongSelection song in System.Enum.GetValues(typeof(SongSelection)))
        {
            songNames.Add(song.ToString());
        }

        songDropdown.AddOptions(songNames);
    }

    // Called when the dropdown value changes
    private void OnDropdownValueChanged(int selectedIndex)
    {
        SongSelection selectedSong = (SongSelection)selectedIndex;

        if (songMap.ContainsKey(selectedSong))
        {
            audioSource.clip = songMap[selectedSong];
            LoadTimestampsForSong(selectedSong);
        }
        else
        {
            Debug.LogError("Selected song does not have a corresponding audio clip assigned.");
        }
    }

    private void LoadTimestampsForSong(SongSelection selectedSong)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, $"{selectedSong.ToString()}_timestamps.json");
        Vector3 endPos = new Vector3(0, 0, 8);

        float distance = Vector3.Distance(transformToSpawnAt, endPos);
        float timeToReachDestination = distance / 5f;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            TimestampData data = JsonUtility.FromJson<TimestampData>(json);
            timestamps = data.timestamps;
            Debug.Log($"Loaded timestamps for {selectedSong}: {timestamps.Count}");

            customTime = timeToReachDestination;
            currentTimestampIndex = 0; // Reset the timestamp index
        }
        else
        {
            Debug.LogError($"Timestamps file not found at: {filePath}");
            timestamps.Clear(); // Clear timestamps if the file doesn't exist
        }

        StartCoroutine(StartRecordingWithDelay());
    }

    private IEnumerator StartRecordingWithDelay()
    {
        float countdown = delayBeforeStart;
        while (countdown > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(countdown).ToString();
            }
            yield return new WaitForSeconds(1f);
            countdown -= 1f;
        }

        if (countdownText != null)
        {
            countdownText.text = "Go!";
        }

        audioSource.Play();

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
        {
            countdownText.text = "";
        }
    }

    public void SetSongSelection(SongSelection selectedSong)
{
    int selectedIndex = (int)selectedSong; // Convert enum to index
    if (songMap.ContainsKey(selectedSong))
    {
        audioSource.clip = songMap[selectedSong];
        LoadTimestampsForSong(selectedSong); // Load timestamps for the new song
    }
    else
    {
        Debug.LogError($"Song {selectedSong} does not have a corresponding audio clip.");
    }
}

}
