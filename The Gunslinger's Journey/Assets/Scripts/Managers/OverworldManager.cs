using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class OverworldManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
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
    private Vector3 playerWorldPosition;
    private Vector3 cameraLocalPosition;
    private Vector3 cameraWorldPosition;
    [SerializeField] private List<GameObject> allGameObjects;
    [SerializeField] private List<GameObject> allEnemies;
    public GameObject enemyToFight;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = new Vector3(PlayerStats.XCoordinate, PlayerStats.YCoordinate);
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
        foreach (GameObject obj in allGameObjects)
        {
            if (obj.TryGetComponent<Enemy>(out Enemy script))
            {
                allEnemies.Add(obj);
            }
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
        //if (Input.GetKeyDown(KeyCode.BackQuote)) { playerScript.FlowHeal(); }
        PlayerStats.XCoordinate = playerWorldPosition.x;
        PlayerStats.YCoordinate = playerWorldPosition.y;
        foreach (GameObject foe in allEnemies)
        {
            Rigidbody2D foeRigidBody = foe.GetComponent<Rigidbody2D>();
            List<Collider2D> listOfContacts = new List<Collider2D>();
            int numOfContacts = foe.GetComponent<Rigidbody2D>().GetContacts(listOfContacts);
            Debug.Log(numOfContacts);

            if (playerCollider.IsTouching(foeRigidBody.GetComponent<Collider2D>()))
                ToBattle(foe);
            else if (numOfContacts > 0)
                enemyManager.RespawnEnemy(foe);
        }
    }
    private void ToBattle(GameObject enemy)
    {
        enemyToFight = enemy;
        DontDestroyOnLoad(enemyToFight);
        PlayerStats.Health = playerScript.GetHitPoints();
        PlayerStats.Flow = playerScript.GetFlowPoints();
        BattleStats.Timer = 0;
        SceneManager.LoadScene(0);
    }
}
