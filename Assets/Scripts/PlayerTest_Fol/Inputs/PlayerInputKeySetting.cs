using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlayerInputKeySetting : ScriptableObject
{
    [SerializeField] public KeyCode forward, backward, right, left, jump, run;
}