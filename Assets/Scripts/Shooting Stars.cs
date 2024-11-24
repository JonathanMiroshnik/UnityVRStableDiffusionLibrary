using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingStars : MonoBehaviour
{
    public GameObject shootingStarPrefab;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private float spawnRadius = 50f;
    [SerializeField] private float spawnHeight = 25f;

    private float nextSpawnTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnShootingStar();
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnShootingStar()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-spawnRadius, spawnRadius), spawnHeight, Random.Range(-spawnRadius, spawnRadius));
        GameObject shootingStar = Instantiate(shootingStarPrefab, spawnPosition, Quaternion.identity);
        Rigidbody rb = shootingStar.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.right * speed;
        }
    }
}
