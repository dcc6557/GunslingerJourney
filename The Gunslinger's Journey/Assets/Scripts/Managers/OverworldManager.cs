using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class OverworldManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    private Player playerScript;
    GameObject playerObject;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = new Vector3(0.0f, 0.0f);
        playerScript = playerObject.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
