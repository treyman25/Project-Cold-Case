using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    [SerializeField] private int objectID;
    [SerializeField] private bool isInspectable;
    [SerializeField] private bool isMovable;
    [SerializeField] private bool isBreakable;

    // Animation
    private Animator myAnimator;
    SpriteRenderer SR;

    // Inspectable
    public string myText;

    // Movable
    private bool isOverlapping = false;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private float pickupBuffer = .75f;
    private bool hasBeenMoved = false;

    // Breakable
    public GameObject brokenVersion;
    private GameObject myBrokenVersion;


    private void Start()
    {
        originalPosition = transform.position;

        myAnimator = GetComponent<Animator>();
        originalPosition = transform.position;

        SR = GetComponent<SpriteRenderer>();
    }

    public bool IsInspectable()
    {
        return isInspectable;
    }

    public bool IsMovable()
    {
        return isMovable;
    }

    public bool IsBreakable()
    {
        return isBreakable;
    }

    public bool IsOverlapping()
    {
        return isOverlapping;
    }

    public void SetMoving(bool value)
    {
        isMoving = value;

        if (isMoving)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Object>() != null && isMoving && !isOverlapping)
        {
            isOverlapping = true;

            if (myAnimator != null)
            {
                myAnimator.SetBool("isOverlapping", true);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Object>() != null)
        {
            isOverlapping = false;
            if (myAnimator != null)
            {
                myAnimator.SetBool("isOverlapping", false);
            }
        }
    }

    public float GetInteractTarget(float playerX)
    {
        if (playerX < transform.position.x)
        {
            return transform.position.x - (GetComponent<Collider2D>().bounds.size.x) / 2 - pickupBuffer;
        }
        else
        {
            return transform.position.x + (GetComponent<Collider2D>().bounds.size.x) / 2 + pickupBuffer;
        }
    }

    public void SetTransparency(float percentage)
    {
        Color newColor = SR.color;
        newColor.a = percentage;
        SR.color = newColor;
    }

    public float GetTransparency()
    {
        return SR.color.a;
    }

    public void ResetObject()
    {
        transform.position = originalPosition;
        hasBeenMoved = false;

        if (myBrokenVersion != null)
        {
            Destroy(myBrokenVersion);
            SR.enabled = true;
            GetComponent<Collider2D>().enabled = true;
        }
    }

    public bool HasBeenMoved()
    {
        return hasBeenMoved;
    }

    public void Moved()
    {
        hasBeenMoved = true;
    }

    public string GetInspectText()
    {
        return myText;
    }

    public void Break()
    {
        if (brokenVersion != null)
        {
            myBrokenVersion = Instantiate(brokenVersion, transform.position, Quaternion.identity);
            SR.enabled = false;
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
