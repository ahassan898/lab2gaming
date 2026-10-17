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
    private Animator anim;
    private bool grounded;
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
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

        if (Input.GetKeyDown(space) && grounded)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumph);
        }

        anim.SetFloat("speed", Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x));

        anim.SetFloat("height", GetComponent<Rigidbody2D>().velocity.y);

        anim.SetBool("grounded", grounded );


    }

        void FixedUpdate()
        {
            grounded = Physics2D.OverlapCircle(groundcheck.position, r, whatIsGround);
        }
        
    }
