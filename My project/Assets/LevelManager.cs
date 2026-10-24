
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject CurrentCheckpoint;
   // public GameObject player;
    private Vector3 startPosition;

    void Start()
    {
        CurrentCheckpoint = null;
        startPosition = FindObjectOfType<move>().transform.position;
    }

    void Update()
    {
    }

    public void RespawnPlayer()
    {
    
        // No checkpoint reached yet: send the player back to where the level started
        if (CurrentCheckpoint == null)
        {
            FindObjectOfType<move>().transform.position = startPosition;
            return;
        }

         FindObjectOfType<move>().transform.position = CurrentCheckpoint.transform.position;
        
    }
}
