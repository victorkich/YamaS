using UnityEngine;
using System; // System is still needed for Action<>

public class SpawnBall : MonoBehaviour
{
    public GameObject objectToSpawn;
    public BoxCollider spawnArea;
    public float yOffset = 0.1f;

    public GameObject spawnedObject; // Variável para manter referência ao objeto spawnado

    public event Action<GameObject> OnBallSpawned;

    private void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        // Destrua o objeto spawnado anteriormente, se existir
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
        }

        Vector3 spawnPosition = GetRandomPositionInSpawnArea();
        spawnPosition.y += yOffset; // Adiciona a compensação ao longo do eixo Y
        spawnedObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity); // Armazena a referência ao novo objeto spawnado
        OnBallSpawned?.Invoke(spawnedObject);
    }

    private void OnDestroy()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
        }
    }

    private Vector3 GetRandomPositionInSpawnArea()
    {
        Vector3 min = spawnArea.bounds.min;
        Vector3 max = spawnArea.bounds.max;
        float x = UnityEngine.Random.Range(min.x, max.x); // Especifique UnityEngine aqui
        float y = min.y;
        float z = UnityEngine.Random.Range(min.z, max.z); // Especifique UnityEngine aqui

        return new Vector3(x, y, z);
    }
}
