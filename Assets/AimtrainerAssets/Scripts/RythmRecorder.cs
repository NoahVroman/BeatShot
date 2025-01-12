using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Collections;

public class RhythmRecorder : MonoBehaviour
{
    public AudioSource audioSource;
    public string songName = "default_song";
    private List<float> timestamps = new List<float>();
    public float delayBeforeStart = 3f;
    public bool enableEditing = true;
    private float customTime = 0f; // Tracks time manually
    private bool isRecording = false; // Flag to determine when the timer should run
    public float startOffset = -3f; // Allows negative timestamps to account for early spawns
    public TMP_Text countdownText; // To show countdown on screen

    void Start()
    {
        StartCoroutine(StartRecordingWithDelay());
    }

    void Update()
    {
        if (isRecording)
        {
            customTime += Time.deltaTime;

            if (Input.GetMouseButtonDown(0)) // Record timestamp on left click
            {
                timestamps.Add(customTime);
                Debug.Log($"Timestamp recorded: {customTime}");
            }
        }
    }

    private void AdjustAllTimestamps(float adjustment)
    {
        // Adjust all timestamps by the given adjustment (for start delay and travel time)
        for (int i = 0; i < timestamps.Count; i++)
        {
            timestamps[i] += adjustment;
            Debug.Log($"Adjusted timestamp at index {i} to: {timestamps[i]}");
        }
    }

    private IEnumerator StartRecordingWithDelay()
    {
        Debug.Log($"Get ready! The song will start in {delayBeforeStart} seconds.");

        float countdown = delayBeforeStart;
        while (countdown > 0)
        {
            Debug.Log(countdown);

            yield return new WaitForSeconds(1f);
            countdown -= 1f;
        }

        if (countdownText != null)
        {
            countdownText.text = "Go!";
        }

        customTime = 0f;
        isRecording = true; // Start recording
        audioSource.Play(); // Start the song
    }

    void OnApplicationQuit()
    {
        isRecording = false;

        if (string.IsNullOrEmpty(songName))
        {
            return;
        }

        // Adjust timestamps for both start delay and travel time
        Vector3 startPos = new Vector3(0, 0, 20);
        Vector3 endPos = new Vector3(0, 0, 8);
        float distance = Vector3.Distance(startPos, endPos);
        float timeToReachDestination = distance / 5f; // Speed is 5 units per second

        AdjustAllTimestamps(delayBeforeStart); // Apply both offsets

        SaveTimestamps();
    }

    private void SaveTimestamps()
    {
        TimestampData data = new TimestampData { timestamps = timestamps };
        string json = JsonUtility.ToJson(data, true);
        string filePath = Application.dataPath + $"/{songName}_timestamps.json";
        File.WriteAllText(filePath, json);
        Debug.Log($"Timestamps saved to: {filePath}");
    }
}

[System.Serializable]
public class TimestampData
{
    public List<float> timestamps;
}
