using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager_Script : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] LayerMask groundMask;
    private GameObject playerGO;
    public static Player player;
    
    void Start()
    {
        playerGO = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player = new Player(playerGO, groundMask);
        this.GetComponent<PlayerController_Script>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
