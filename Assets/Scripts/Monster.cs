using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : PhysicsObject
{
    public Animator animator;
    bool playerNear;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        targetVelocity = Vector2.right;

        animator.SetBool("Walking", true);
    }
    
}
