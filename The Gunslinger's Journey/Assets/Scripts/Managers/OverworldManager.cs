using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class OverworldManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Camera mainCamera;
    private Player playerScript;
    private Vector3 xMove = new Vector3(0.01f, 0.0f);
    private Vector3 yMove = new Vector3(0.0f, 0.01f);
    GameObject playerObject;
    private Vector3 playerWorldPosition;
    private Vector3 playerLocalPosition;
    private Vector3 cameraWorldPosition;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = new Vector3(0.0f, 0.0f);
        playerScript = playerObject.GetComponent<Player>();
        playerObject.transform.SetParent(mainCamera.transform);
        playerWorldPosition = playerObject.transform.position;
        playerLocalPosition = playerObject.transform.localPosition;
        cameraWorldPosition = mainCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        playerWorldPosition = playerObject.transform.position;
        playerLocalPosition = playerObject.transform.localPosition;
        cameraWorldPosition = mainCamera.transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            if (cameraWorldPosition.y <= 4.2f && playerLocalPosition.y >= 0.0f)
                mainCamera.transform.position += yMove;
            else
            {
                if(playerLocalPosition.y <= 7.35f)
                    playerObject.transform.position += yMove;
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (cameraWorldPosition.y >= -5.0f && playerLocalPosition.y <= 0.0f)
                mainCamera.transform.position -= yMove;
            else
            {
                if(playerLocalPosition.y >= -6.7f)
                    playerObject.transform.position -= yMove;
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (cameraWorldPosition.x >= -6.0f && playerLocalPosition.x <= 0.0f)
                mainCamera.transform.position -= xMove;
            else
            {
                if(playerLocalPosition.x >= -14.9f)
                    playerObject.transform.position -= xMove;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (cameraWorldPosition.x <= 5.5f && playerLocalPosition.x >= 0.0f)
                mainCamera.transform.position += xMove;
            else
            {
                if(playerLocalPosition.x <= 15.0f)
                    playerObject.transform.position += xMove;
            }
        }
    }
}
