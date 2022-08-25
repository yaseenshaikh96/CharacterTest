using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public GameObject playerGO{get; private set;}
    private LayerMask groundLayer;

    public Player(GameObject playerGO, LayerMask groundLayer)
    {
        this.playerGO = playerGO;
        this.groundLayer = groundLayer;
    }

    public void Move(Vector3 dir)
    {
        playerGO.transform.position += dir;
    }
}
