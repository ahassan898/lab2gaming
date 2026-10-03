using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{

    public float movespeed;
    public float jumph;
    public KeyCode left;
    public KeyCode right;
    public KeyCode space;
    
    public Transform groundcheck;
    public float r;
    public LayerMask whatIsGround;
    private bool isGrounded;

    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(left))
        {
         GetComponent<Rigidbody2D>().velocity = new Vector2(-movespeed, GetComponent<Rigidbody2D>().velocity.y);
         if (GetComponent<SpriteRenderer>()!= null)
         {
          GetComponent<SpriteRenderer>().flipX = true;
         }
        }

        if(Input.GetKey(right))
        {
         GetComponent<Rigidbody2D>().velocity = new Vector2(movespeed, GetComponent<Rigidbody2D>().velocity.y);
         if (GetComponent<SpriteRenderer>()!= null)
         {
          GetComponent<SpriteRenderer>().flipX = false;
         }
        }

        if (Input.GetKeyDown(space) && isGrounded)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumph);
        }

    }

        void FixedUpdate()
        {
            isGrounded = Physics2D.OverlapCircle(groundcheck.position, r, whatIsGround);
        }
        
    }
