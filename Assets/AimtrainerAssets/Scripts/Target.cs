using System; // For Action
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    public static event Action OnMissedShot; // Define a static event

    [Header("Cube Settings")]
    public float cubeSpeed = 5f;

    private Transform cameraTransform;
    private float beatDistance = 8f;

    [Header("UI Visualizer")]
    [SerializeField] private Image beatVisualizer;
    private Vector3 originalScale;

    private bool isVisualizerActive = false;

    private void Start()
    {
        if (beatVisualizer != null)
        {
            originalScale = new Vector3(1f, 1f, 1f);
            beatVisualizer.enabled = false;
        }
    }

    private void Update()
    {
        UpdateCubeMovement();
        UpdateBeatVisual();
    }

    private void UpdateCubeMovement()
    {
        transform.position += Vector3.back * cubeSpeed * Time.deltaTime;

        if (transform.position.z <= beatDistance)
        {
            Debug.Log("On the beat!");

        }



        if (transform.position.z <= beatDistance - 1f)
        {
            // Fire the missed shot event before destroying the object
            OnMissedShot?.Invoke();
            Destroy(gameObject);
        }
    }

    private void UpdateBeatVisual()
    {
        float distanceToBeat = transform.position.z - beatDistance;

        if (distanceToBeat <= 3f && !isVisualizerActive)
        {
            isVisualizerActive = true;
            beatVisualizer.enabled = true;
        }

        if (isVisualizerActive)
        {
            if (distanceToBeat >= 0)
            {
                float scale = Mathf.Lerp(0.5f, 1f, distanceToBeat / 3f);

                beatVisualizer.transform.localScale = originalScale * scale;
            }
            else
            {
                beatVisualizer.enabled = false;
            }
        }
    }
}
