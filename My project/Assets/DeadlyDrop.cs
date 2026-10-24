
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyDrop : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player fell into " + gameObject.name + " - respawning at the last checkpoint");
            FindObjectOfType<LevelManager>().RespawnPlayer();
        }
    }
}
