using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    [SerializeField] private int objectID;
    [SerializeField] private bool isInspectable;
    [SerializeField] private bool isMovable;
    [SerializeField] private bool isBreakable;
    [SerializeField] private bool isHider;
    [SerializeField] private float HiderX;
    [SerializeField] private float HiderY;

    // Animation
    //private Animator myAnimator;
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

    // Hidable
    private bool isHidden = false;
    private bool canHide = false;
    private GameObject hiddenObject;

    // Combos
    public GameObject[] specialComboObject;
    private bool specialComboApplied = false;
    private GameObject[] createdSpecialComboObject;


    private void Start()
    {
        originalPosition = transform.position;

        //myAnimator = GetComponent<Animator>();
        originalPosition = transform.position;

        SR = GetComponent<SpriteRenderer>();

        canHide = isHider;
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

    public bool CanHide()
    {
        return canHide && hiddenObject == null;
    }
    
    public void HideObject(GameObject hidden)
    {
        hiddenObject = hidden;
    }

    public GameObject GetHidden()
    {
        return hiddenObject;
    }

    public Vector2 GetHiderBounds()
    {
        return new Vector2(HiderX, HiderY);
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
        if (collision.gameObject.GetComponent<Object>() != null)
        { 
            if (isMoving)
            {
                if (!isOverlapping && !CheckHidable(collision.gameObject) && GetSpecialComboId(collision.gameObject) == 0)
                {
                    isOverlapping = true;

                    OverlapTint(true);
                }
            }
            else
            {
                if (CheckHidable(collision.gameObject) && !isHidden)
                {
                    collision.gameObject.GetComponent<Object>().HideObject(this.gameObject);
                    Hide(true);
                }

                if (GetSpecialComboId(collision.gameObject) != 0  && !specialComboApplied)
                {
                    ApplySpecialComboId(GetSpecialComboId(collision.gameObject), collision.gameObject);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Object>() != null)
        {
            isOverlapping = false;

            OverlapTint(false);
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
        }

        Hide(false);

        canHide = isHider;
        hiddenObject = null;

        specialComboApplied = false;
        if (createdSpecialComboObject != null && createdSpecialComboObject.Length > 0)
        {
            foreach (var createdObject in createdSpecialComboObject)
            {
                Destroy(createdObject);
            }
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
            float offset = GetVerticalOffset(brokenVersion, gameObject);

            myBrokenVersion = Instantiate(brokenVersion, transform.position, Quaternion.identity);
            myBrokenVersion.transform.Translate(0, offset, 2);

            Hide(true);
            canHide = false;
            if (hiddenObject)
            {
                hiddenObject.GetComponent<Object>().Hide(false);
                hiddenObject = null;
            }
        }
    }

    public void Hide(bool value)
    {
        isHidden = value;
        SR.enabled = !value;
        GetComponent<Collider2D>().enabled = !value;
    }

    private void OverlapTint(bool value)
    {
        if (value)
        {
            SR.color = Color.red;
        }
        else
        {
            SR.color = Color.white;
        }
    }

    private bool CheckHidable(GameObject hider)
    {
        if (hider.GetComponent<Object>().CanHide())
        {
            Bounds myBounds = GetComponent<Collider2D>().bounds;
            Vector2 hiderBounds = hider.GetComponent<Object>().GetHiderBounds();
            if (myBounds.size.x < hiderBounds.x && myBounds.size.y < hiderBounds.y)
            {
                return true;
            }
            if (myBounds.size.y < hiderBounds.x && myBounds.size.x < hiderBounds.y)
            {
                return true;
            }
        }
        return false;
    }

    private int GetSpecialComboId(GameObject other)
    {
        if (other.CompareTag("Mousetrap") && this.CompareTag("Cheese"))
        {
            return 1;
        }
        return 0;
    }
    private void ApplySpecialComboId(int id, GameObject other)
    {
        specialComboApplied = true;

        switch (id)
        {
            case 1:
                Hide(true);
                other.GetComponent<Object>().Hide(true);

                float offset = GetVerticalOffset(other, specialComboObject[0]);

                createdSpecialComboObject[0] = Instantiate(specialComboObject[0], other.transform.position, Quaternion.identity);
                createdSpecialComboObject[0].transform.Translate(0, offset, 0);
                break;

            default:
                break;
        }
    }

    private float GetVerticalOffset(GameObject first, GameObject second)
    {
        float firstHeight = first.GetComponent<Collider2D>().bounds.size.y;
        float secondHeight = second.GetComponent<Collider2D>().bounds.size.y;

        return (firstHeight - secondHeight) / 4;
    }
}
