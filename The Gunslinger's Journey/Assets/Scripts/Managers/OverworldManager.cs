using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEditor.Timeline;

public class OverworldManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap wallTiles;
    [SerializeField] private Tilemap floorTiles;
    [SerializeField] private EnemyManager enemyManager;
    bool frameAfterStart = false;
    private Player playerScript;
    private Vector2 xMove = new Vector3(2f, 0.0f);
    private Vector2 yMove = new Vector3(0.0f, 2f);
    GameObject playerObject;
    Rigidbody2D playerCollider;
    GameObject exitObject;
    GameObject keyObject;
    private Vector3 playerWorldPosition;
    private Vector3 cameraLocalPosition;
    private Vector3 cameraWorldPosition;
    [SerializeField] private List<GameObject> allGameObjects;
    [SerializeField] private List<GameObject> allEnemies;
    public GameObject enemyToFight;
    BoundsInt spawnBounds;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = Instantiate(playerPrefab);
        exitObject = Instantiate(exitPrefab);
        keyObject = Instantiate(keyPrefab);
        spawnBounds = floorTiles.cellBounds;

        if (PlayerStats.SpawnSet) { Respawn(playerObject, PlayerStats.XCoordinate, PlayerStats.YCoordinate); }
        else
        {
            Spawn(playerObject, true);
            PlayerStats.XCoordinate = playerObject.transform.position.x;
            PlayerStats.YCoordinate = playerObject.transform.position.y;
            PlayerStats.SpawnSet = true;
        }

        if (OverworldStats.ExitSet) { Respawn(exitObject, OverworldStats.XCoordinateExit, OverworldStats.YCoordinateExit); }
        else
        {
            Spawn(exitObject);
            OverworldStats.XCoordinateExit = exitObject.transform.position.x;
            OverworldStats.YCoordinateExit = exitObject.transform.position.y;
            OverworldStats.ExitSet = true;
        }

        if (!OverworldStats.CanExit)
        {
            if (OverworldStats.KeySet) { Respawn(keyObject, OverworldStats.XCoordinateKey, OverworldStats.YCoordinateKey); }
            else
            {
                Spawn(keyObject);
                OverworldStats.XCoordinateKey = keyObject.transform.position.x;
                OverworldStats.YCoordinateKey = keyObject.transform.position.y;
                OverworldStats.KeySet = true;
            }
        }

        playerScript = playerObject.GetComponent<Player>();
        playerCollider = playerObject.GetComponent<Rigidbody2D>();
        mainCamera.transform.SetParent(playerObject.transform);
        mainCamera.transform.localPosition = new Vector3(0, 0, -10);
        playerWorldPosition = playerObject.transform.position;
        cameraLocalPosition = mainCamera.transform.localPosition;
        cameraWorldPosition = mainCamera.transform.position;
        playerScript.SetUpCharacter(PlayerStats.Health, PlayerStats.Flow);
        playerScript.ModifyHealth();
        playerScript.ModifyFlow();
        enemyManager.MakeEnemies();
        allGameObjects = new List<GameObject>();
        allEnemies = new List<GameObject>();
        allGameObjects.AddRange(FindObjectsOfType<GameObject>());

        foreach (GameObject obj in allGameObjects) {
            if (obj.TryGetComponent<Enemy>(out Enemy script)) { allEnemies.Add(obj);}
        }
        enemyManager.SpawnEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        playerCollider.velocity = Vector2.zero;
        playerWorldPosition = playerObject.transform.position;
        cameraLocalPosition = mainCamera.transform.localPosition;
        cameraWorldPosition = mainCamera.transform.position;
        if (Input.GetKey(KeyCode.W)) { playerCollider.velocity += yMove; }
        if (Input.GetKey(KeyCode.S)) { playerCollider.velocity -= yMove; }
        if (Input.GetKey(KeyCode.A)) { playerCollider.velocity -= xMove; }
        if (Input.GetKey(KeyCode.D)) { playerCollider.velocity += xMove; }
        if (Input.GetKeyDown(KeyCode.BackQuote)) { TryHeal(); }
        PlayerStats.XCoordinate = playerWorldPosition.x;
        PlayerStats.YCoordinate = playerWorldPosition.y;
        foreach (GameObject foe in allEnemies)
        {
            Rigidbody2D foeRigidBody = foe.GetComponent<Rigidbody2D>();
            List<Collider2D> listOfContacts = new List<Collider2D>();
            int numOfContacts = foe.GetComponent<Rigidbody2D>().GetContacts(listOfContacts);

            if (playerCollider.IsTouching(foeRigidBody.GetComponent<Collider2D>()))
                ToBattle(foe);
            else if (numOfContacts > 0)
                enemyManager.RespawnEnemy(foe);
        }
        if (playerCollider.IsTouching(exitObject.GetComponent<Collider2D>()) && OverworldStats.CanExit)
            ToExit();
        if (playerCollider.IsTouching(keyObject.GetComponent<Collider2D>()))
        {
            keyObject.SetActive(false);
            OverworldStats.CanExit = true;
        }
    }
    private void ToBattle(GameObject enemy)
    {
        enemyToFight = enemy;
        DontDestroyOnLoad(enemyToFight);
        PlayerStats.Health = playerScript.GetHitPoints();
        PlayerStats.Flow = playerScript.GetFlowPoints();
        BattleStats.Timer = 0;
        SceneManager.LoadScene(2);
    }
    private void ToExit()
    {
        ResetOverworldVariables();
        SceneManager.LoadScene(0);
    }
    private void TryHeal()
    {
        if (playerScript.GetFlowPoints() < 12)
        {
            Debug.Log("Not enough Flow!!!");
        }
        else if (playerScript.GetHitPoints() == playerScript.GetMaxHitPoints())
        {
            Debug.Log("Health is full!!!");
        }
        else
        {
            playerScript.FlowHeal();
            playerScript.ModifyFlow(-12);
            playerScript.ModifyHealth();
        }
    }

    public void Spawn(GameObject thingToSpawn, bool isPlayer = false)
    {
        int numOfContacts;
        do
        {
            List<Collider2D> pointsOfContact = new List<Collider2D>();
            thingToSpawn.transform.position = GetRandomTile(isPlayer);
            numOfContacts = thingToSpawn.GetComponent<Rigidbody2D>().GetContacts(pointsOfContact);
            Debug.Log(thingToSpawn.name + " points of contact: " + numOfContacts);
        } while (numOfContacts > 0);
    }
    public void Respawn(GameObject thingToRespawn, float x, float y) { thingToRespawn.transform.position = new Vector3(x, y); }
    private Vector3Int GetRandomTile(bool isPlayer)
    {
        Vector3Int randomTile = Vector3Int.zero;
        bool ableToSpawn = false;
        while (!ableToSpawn)
        {
            ableToSpawn = true;
            randomTile = new Vector3Int(Random.Range(spawnBounds.xMin, spawnBounds.xMax), Random.Range(spawnBounds.yMin, spawnBounds.yMax));
            if (!floorTiles.HasTile(randomTile) || CheckIfNearWall(randomTile))
                ableToSpawn = false;
            if (!isPlayer)
            {
                if (Vector3.Distance(randomTile, new Vector3(PlayerStats.XCoordinate, PlayerStats.YCoordinate)) < 5)
                    ableToSpawn = false;
            }
        }
        return randomTile;
    }
    private bool CheckIfNearWall(Vector3Int tile)
    {
        if (wallTiles.HasTile(new Vector3Int(tile.x - 1, tile.y)))
            return true;
        else if (wallTiles.HasTile(new Vector3Int(tile.x + 1, tile.y)))
            return true;
        else if (wallTiles.HasTile(new Vector3Int(tile.x, tile.y - 1)))
            return true;
        else if (wallTiles.HasTile(new Vector3Int(tile.x, tile.y + 1)))
            return true;
        return false;
    }
    private void ResetOverworldVariables()
    {
        OverworldStats.ExitSet = false;
        OverworldStats.KeySet = false;
        OverworldStats.CanExit = false;
        PlayerStats.SpawnSet = false;

    }
}
