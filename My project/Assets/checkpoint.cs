using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    //public GameObject CurrentCheckpoint;
    //public GameObject player;

    void Start()
    {
        //CurrentCheckpoint = null;
    }

    void Update()
    {
    }

   /* public void RespwanPlayer()
    {
        if (CurrentCheckpoint != null && player != null)
        {
            player.transform.position = CurrentCheckpoint.transform.position;
        }
    }*/

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (FindObjectOfType<LevelManager>().CurrentCheckpoint != gameObject)
                Debug.Log("Checkpoint reached: " + (transform.parent != null ? transform.parent.name : name));
            FindObjectOfType<LevelManager>().CurrentCheckpoint = gameObject;
        }
    }
}