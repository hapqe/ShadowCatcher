using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    new Rigidbody2D rigidbody;
    new BoxCollider2D collider;
    Animator animator;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    [SerializeField]
    float speed = 5f;

    [SerializeField]
    float jumpHeight = 2f;

    [SerializeField]
    float timeToJumpApex = .7f;

    [SerializeField]
    float squashLimit = .2f;

    [SerializeField]
    LayerMask groundMask;

    float gravity;

    float jumpVelocity;

    Vector2 velocity;

    [SerializeField]
    GameObject deathParticles;

    private void Start()
    {
        gravity = -(2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        jumpVelocity = -gravity * timeToJumpApex;
        startSquashLimit = squashLimit;
    }

    private void FixedUpdate()
    {
        Vector2 move = new Vector2(-Input.GetAxisRaw("Horizontal"), 0);
        velocity += Vector2.up * gravity * Time.fixedDeltaTime;
        rigidbody.position += move * speed * Time.fixedDeltaTime + velocity * Time.fixedDeltaTime;

        if (IsGrounded())
        {
            velocity.y = 0;
            if (Input.GetButtonDown("Jump"))
            {
                animator.SetTrigger("Jump");
                velocity = Vector2.up * jumpVelocity;
            }
        }

        UpdateAnimator(move);
    }

    void UpdateAnimator(Vector2 move)
    {
        if(move.x > 0){
            Debug.Log("right");
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if(move.x < 0){
            Debug.Log("left");
            GetComponent<SpriteRenderer>().flipX = true;
        }
        if(Math.Abs(move.x) > 0){
            animator.SetBool("Walk", true);
        }
        else{
            animator.SetBool("Walk", false);
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        foreach (ContactPoint2D contact in other.contacts)
        {
            var offset = contact.point - (Vector2)transform.position;
            var penetration = Vector2.Dot(-contact.normal, offset);
            var offsetSign = new Vector2(Mathf.Sign(offset.x), Mathf.Sign(offset.y));
            penetration += Vector2.Dot(contact.normal, collider.bounds.extents * offsetSign);
            penetration = Mathf.Abs(penetration);

            if (penetration > squashLimit)
            {
                Die();
            }
        }
    }

    bool dead;
    private void Die()
    {
        if(!dead) {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            GetComponent<SpriteRenderer>().enabled = false;
            speed = 0;
            StartCoroutine(Restart());
        }
        dead = true;
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private bool IsGrounded()
    {
        var rayLenght = collider.bounds.extents.y - velocity.y * Time.fixedDeltaTime + .1f;
        Debug.DrawRay(transform.position, Vector2.down * rayLenght, Color.red);
        return Physics2D.Raycast(transform.position, Vector2.down, rayLenght, groundMask);
    }

    float startSquashLimit;
    public void SetDisableSquashing(bool disable)
    {
        squashLimit = disable ? 100 : startSquashLimit;
    }
}
