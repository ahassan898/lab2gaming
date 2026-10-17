
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject CurrentCheckpoint;
   // public GameObject player;

    void Start()
    {
        CurrentCheckpoint = null;
    }

    void Update()
    {
    }

    public void RespawnPlayer()
    {
    
         FindObjectOfType<move>().transform.position = CurrentCheckpoint.transform.position;
        
    }
}
