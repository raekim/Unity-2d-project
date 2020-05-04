using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public float minGroundNormalY = .65f;
    public float gravityModifier = 1f;

    protected Vector2 targetVelocity;
    protected bool grounded;
    protected Vector2 groundNormal;

    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFiler;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.1f;

    private void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        contactFiler.useTriggers = false;
        contactFiler.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFiler.useLayerMask = true;
    }

    void Update()
    {
        targetVelocity = Vector2.zero; // clear target velocity
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
        velocity.x = targetVelocity.x;

        grounded = false;

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);  // xMovement

        move = Vector2.up * deltaPosition;

        Movement(move, true);   // yMovement
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if(distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFiler, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            for(int i = 0; i < count; ++i)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; ++i)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                if(currentNormal.y > minGroundNormalY)
                {
                    grounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(velocity, currentNormal);
                if(projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        rb2d.position = rb2d.position + move.normalized * distance;
    }
}
