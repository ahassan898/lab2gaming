using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGhost : MonoBehaviour
{
    //Place the respawnable enemy prefab into this variable
    public Transform enemy;

    //Place the gameObject you want the enemy prefab to spawn at
    public Transform spawnPoint;

    void RespawnEnemy()
    {
        //Create an instance of the enemy at the spawn point's position and rotation values
        Instantiate(enemy, spawnPoint.transform.position, spawnPoint.transform.rotation);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            RespawnEnemy();
        }
    }
}
