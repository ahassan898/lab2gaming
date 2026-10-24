using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSaw : EnemyController
{
    public Transform player;

    void FixedUpdate()
    {
        //The player is destroyed on Game Over, so there is nothing left to follow
        if (player == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, player.transform.position,
            maxSpeed * Time.deltaTime);
    }

    //Since the saw is not supposed to flip if it collides with player, the Trigger function will be
    //overridden as follows:
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            FindObjectOfType<PlayerStats>().TakeDamage(damage);
        }
        else if (other.tag == "Wall")
        {
            Flip();
        }
    }
}
