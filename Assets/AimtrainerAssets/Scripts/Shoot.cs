using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Shoot : MonoBehaviour
{
    public float shootRange = Mathf.Infinity;
    public Color rayColor = Color.red;
    public int pointsPerHit = 10;

    public TMP_Text scoreText; // Reference to the TMP Text for the score
    public TMP_Text accuracyText; // Reference to the TMP Text for accuracy

    private Camera _camera;
    private int score = 0;
    private int totalShots = 0;
    private int successfulHits = 0;
    private int missedShots = 0;

    private void Start()
    {
        _camera = Camera.main;
        UpdateScoreText();
        UpdateAccuracyText();
    }

    private void OnEnable()
    {
        // Subscribe to the missed shot event
        Target.OnMissedShot += HandleMissedShot;
    }

    private void OnDisable()
    {
        // Unsubscribe from the missed shot event to avoid memory leaks
        Target.OnMissedShot -= HandleMissedShot;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            totalShots++;
            ShootProjectile();
        }
    }

    public void ShootProjectile()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootRange))
        {
            if (hit.collider.CompareTag("Target"))
            {
                Destroy(hit.collider.gameObject);
                AddScore(pointsPerHit);
                successfulHits++;
            }
        }

        UpdateAccuracyText();
    }

    private void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = score.ToString("F0");
    }

    private void UpdateAccuracyText()
    {
        float accuracy = totalShots > 0 ? (float)successfulHits / totalShots * 100 : 0;
        accuracyText.text = $"{accuracy:F0}%";
    }

    private void HandleMissedShot()
    {
        missedShots++;
        totalShots++; // Count missed shots as part of the total
        UpdateAccuracyText();
    }
}
