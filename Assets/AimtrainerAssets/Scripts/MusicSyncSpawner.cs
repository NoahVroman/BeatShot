using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSyncSpawner : MonoBehaviour
{
    [Header("Cube Settings")]
    public GameObject cubePrefab;
    public Vector3 gridStartPosition = Vector3.zero;

    public float linePositionZ = 0f;
    public float cubeSpeed = 5f;

    [Header("Grid Settings")]
    [SerializeField] private int gridSize = 3;
    [SerializeField] private float spacing = 1.5f;

    [Header("Beat Settings")]
    [SerializeField] private float timeBetweenBeats = 0.5f;  // Time between each beat (in seconds)

    private HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();
    private List<GameObject> activeCubes = new List<GameObject>();

    private void Update()
    {
        UpdateCubeMovement();
    }

    public void SpawnCubeAtBeat()
    {
        if (cubePrefab == null)
        {
            Debug.LogWarning("Cube prefab is not assigned.");
            return;
        }


        Vector3 centerOffset = new Vector3(gridSize / 2f * spacing, gridSize / 2f * spacing, 0);
        Vector3 startPosition = gridStartPosition - centerOffset;

        usedPositions.RemoveWhere(pos => !Physics.CheckSphere(startPosition + new Vector3(pos.x * spacing, pos.y * spacing, 0), 0.1f));

        if (usedPositions.Count >= gridSize * gridSize)
        {
            Debug.LogWarning("Grid is full. No more cubes can be added.");
            return;
        }

        Vector2Int randomPosition;
        do
        {
            int x = Random.Range(0, gridSize);
            int y = Random.Range(0, gridSize);
            randomPosition = new Vector2Int(x, y);
        } while (usedPositions.Contains(randomPosition));

        usedPositions.Add(randomPosition);

        Vector3 spawnPosition = startPosition + new Vector3(randomPosition.x * spacing, randomPosition.y * spacing, -10f);

        var cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        activeCubes.Add(cube);
    }

    private void UpdateCubeMovement()
    {
        // Move all active cubes towards the target position (linePositionZ)
        for (int i = activeCubes.Count - 1; i >= 0; i--)
        {
            var cube = activeCubes[i];
            if (cube == null)
            {
                activeCubes.RemoveAt(i);
                continue;
            }

            Vector3 targetPosition = new Vector3(cube.transform.position.x, cube.transform.position.y, linePositionZ);
            cube.transform.position = Vector3.MoveTowards(cube.transform.position, targetPosition, cubeSpeed * Time.deltaTime);

            // Destroy the cube when it reaches the target position
            if (Mathf.Abs(cube.transform.position.z - linePositionZ) < 0.5f)
            {
                activeCubes.RemoveAt(i);
                Destroy(cube);
            }
        }
    }
}