using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Script : MonoBehaviour
{
    private Player player;
    [SerializeField] private PlayerInput playerInput;

    void Start()
    {
        player = PlayerManager_Script.player;
        playerInput = new PlayerInput();
    }

    void Update()
    {
        player.Move(playerInput.dir);
        playerInput.Update();

    }

}
