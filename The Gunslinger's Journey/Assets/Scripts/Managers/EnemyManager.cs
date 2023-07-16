using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject diamanPrefab;
    [SerializeField] private GameObject sirkillPrefab;
    [SerializeField] Tilemap spawnableAreaTilemap;
    BoundsInt spawnBounds;
    List<GameObject> enemyDatabase;
    List<GameObject> allEnemies;

    public void MakeEnemies()
    {
        enemyDatabase = new List<GameObject>();
        enemyDatabase.Add(diamanPrefab);
        enemyDatabase.Add(sirkillPrefab);
        if (OverworldStats.TotalEnemies == -1) { OverworldStats.TotalEnemies = 5; }
        spawnBounds = spawnableAreaTilemap.cellBounds;
        SetUpEnemyList();
        SpawnEnemies();
    }

    public void SetUpEnemyList()
    {
        allEnemies = new List<GameObject>();
        for (int x = 0; x < OverworldStats.TotalEnemies; x++)
        {
            int index = Random.Range(0, enemyDatabase.Count);
            allEnemies.Add(Instantiate(enemyDatabase[index]));
        }
    }
    public void SpawnEnemies()
    {
        foreach (GameObject foe in allEnemies)
        {
            foe.transform.position = GetRandomTile();
            foe.GetComponent<Enemy>().SetUpEnemyOverworld();
        }
    }
    private Vector3Int GetRandomTile()
    {
        Vector3Int randomTile = Vector3Int.zero;
        bool ableToSpawn = false;
        while (ableToSpawn == false)
        {
            randomTile = new Vector3Int(Random.Range(spawnBounds.xMin, spawnBounds.xMax), Random.Range(spawnBounds.yMin, spawnBounds.yMax));
            if (spawnableAreaTilemap.HasTile(randomTile))
                ableToSpawn = true;
        }
        return randomTile;
    }
}

