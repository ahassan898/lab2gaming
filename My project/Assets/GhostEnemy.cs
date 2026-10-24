using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEnemy : EnemyController
{
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.flipX = false;
    }

    void FixedUpdate()
    {
        //The ghost sprite faces left, so it flies left until it is flipped
        if (sr.flipX == true)
        {
            this.GetComponent<Rigidbody2D>().velocity =
                new Vector2(maxSpeed, this.GetComponent<Rigidbody2D>().velocity.y);
        }
        else
        {
            this.GetComponent<Rigidbody2D>().velocity =
                new Vector2(-maxSpeed, this.GetComponent<Rigidbody2D>().velocity.y);
        }
    }

    //Because ghosts can pass through walls, the Trigger function is overridden from the Superclass
    //to remove the flip function upon hitting a wall
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player was hit by " + gameObject.name + " (damage " + damage + ")");
            FindObjectOfType<PlayerStats>().TakeDamage(damage);
            Flip();
        }
    }
}
