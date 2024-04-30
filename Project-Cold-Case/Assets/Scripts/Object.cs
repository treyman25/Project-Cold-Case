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
    [SerializeField] private float hiderX;
    [SerializeField] private float hiderY;
    [SerializeField] private bool isVariant;

    // Animation
    SpriteRenderer SR;

    // Inspectable
    public string[] myText;
    private string inspectText;

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
    private GameObject myCollision;
    public GameObject[] variant;
    private bool specialComboApplied = false;
    private GameObject createdVariant;
    private GameObject createdVariant2;

    // Sound
    public AudioClip[] audioclips;


    private void Start()
    {
        originalPosition = transform.position;

        originalPosition = transform.position;

        SR = GetComponent<SpriteRenderer>();

        canHide = isHider;

        inspectText = null;
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

    public bool IsHidden()
    {
        return isHidden;
    }

    public bool CanHide()
    {
        return canHide && hiddenObject == null;
    }
    
    public void HideObject(GameObject hidden)
    {
        hiddenObject = hidden;
        inspectText += " It holds " + hidden.tag.ToLower() + ".";
    }

    public GameObject GetHidden()
    {
        return hiddenObject;
    }

    public Vector2 GetHiderBounds()
    {
        return new Vector2(hiderX, hiderY);
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
                if (CheckHidable(collision.gameObject) && !isHidden && collision.gameObject.GetComponent<Object>().isMoving == false)
                {
                    collision.gameObject.GetComponent<Object>().HideObject(this.gameObject);
                    Hide(true);
                }

                if (GetSpecialComboId(collision.gameObject) != 0  && !specialComboApplied)
                {
                    myCollision = collision.gameObject;
                    ApplySpecialComboId(GetSpecialComboId(collision.gameObject), myCollision);
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

        if (createdVariant != null)
        {
            createdVariant.GetComponent<Object>().ResetObject();
            Destroy(createdVariant);
        }

        if (createdVariant2 != null)
        {
            createdVariant2.GetComponent<Object>().ResetObject();
            Destroy(createdVariant2);
        }

        Hide(false);

        if (hiddenObject != null)
        {
            hiddenObject = null;
        }

        canHide = isHider;

        specialComboApplied = false;

        if (isVariant)
        {
            Destroy(gameObject);
        }

        if (gameObject.CompareTag("Cheese"))
        {
            Hide(true);
        }

        inspectText = null;
    }

    public bool HasBeenMoved()
    {
        return hasBeenMoved;
    }

    public void Moved()
    {
        hasBeenMoved = true;
        if (hiddenObject)
        {
            float offset = GetVerticalOffset(hiddenObject, gameObject);
            hiddenObject.transform.position = transform.position;
            hiddenObject.transform.Translate(0, offset, 0);
        }

    }

    public string GetInspectText(int id)
    {
        if (inspectText != null)
        {
            return inspectText;
        }
        else
        {
            return myText[id];
        }
    }

    public GameObject[] GetVariants()
    {
        return variant;
    }

    public void Break()
    {
        if (brokenVersion != null)
        {
            float offset = GetVerticalOffset(brokenVersion, gameObject);

            myBrokenVersion = Instantiate(brokenVersion, transform.position, Quaternion.identity);
            myBrokenVersion.transform.Translate(0, offset, 2);

            AudioSource.PlayClipAtPoint(audioclips[0], transform.position, 1f);

            Hide(true);
            canHide = false;
            if (hiddenObject)
            {
                hiddenObject.GetComponent<Object>().Hide(false);

                hiddenObject = null;
            }

            if (CompareTag("Vase"))
            {
                ApplySpecialComboId(6);
            }
        }
    }

    public void Hide(bool value)
    {
        isHidden = value;
        if (SR != null)
        {
            SR.enabled = !value;
        }
        
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = !value;
        }
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
        else if (other.CompareTag("Printer") && this.CompareTag("Flash Drive"))
        {
            return 2;
        }
        return 0;
    }
    private void ApplySpecialComboId(int id, GameObject other)
    {
        specialComboApplied = true;
        Debug.Log("Special Combo #" + id);
        Vector3 otherPosition;
        float offset;

        switch (id)
        {
            case 1: // Loaded Trap
                otherPosition = other.transform.position;
                offset = GetVerticalOffset(other, variant[0]);

                Hide(true);
                other.GetComponent<Object>().Hide(true);

                createdVariant = Instantiate(variant[0], otherPosition, Quaternion.identity);
                createdVariant.transform.Translate(0, offset, 0);

                AudioSource.PlayClipAtPoint(audioclips[0], createdVariant.transform.position, 0.5f);

                break;

            case 2: // USB Printer
                otherPosition = other.transform.position;

                Hide(true);
                other.GetComponent<Object>().Hide(true);

                createdVariant = Instantiate(variant[0], otherPosition, Quaternion.identity);
                createdVariant.transform.Translate(.131f, 0, 0);

                Player p = GameObject.Find("Player").GetComponent<Player>();
                p.HasPrinted(true);

                break;

            default:
                break;
        }
    }

    public void ApplySpecialComboId(int id)
    {
        specialComboApplied = true;
        Debug.Log("Special Combo #" + id);

        float offset;

        switch (id)
        {
            case 1: // Strangled Note
                createdVariant = Instantiate(variant[2], new Vector3(4.17749357f, -3.05999994f, 0), Quaternion.identity);

                break;

            case 2: // Bloody Knife && Stab Note
                Hide(true);
                createdVariant = Instantiate(variant[0], new Vector3(transform.position.x, -3.41f, transform.position.z), Quaternion.identity);
                createdVariant2 = Instantiate(variant[1], new Vector3(4.17749357f, -3.05999994f, 0), Quaternion.identity);

                break;

            case 3: // Rat Trap
                Hide(true);
                createdVariant = Instantiate(variant[0], transform.position, Quaternion.identity);
                offset = GetVerticalOffset(gameObject, variant[0]);
                createdVariant.transform.Translate(0, offset, 0);

                break;

            case 4: // Moldy Trap
                Hide(true);
                createdVariant = Instantiate(variant[1], transform.position, Quaternion.identity);
                offset = GetVerticalOffset(gameObject, variant[1]);
                createdVariant.transform.Translate(0, offset, 0);

                break;

            case 5: // Moldy Cheese
                Hide(true);
                createdVariant = Instantiate(variant[1], transform.position, Quaternion.identity);

                break;

            case 6: // Flash Drive
                createdVariant = Instantiate(variant[0], transform.position, Quaternion.identity);
                createdVariant.transform.Translate(0, -.79f, 0);
                createdVariant.transform.Rotate(0, 0, 90);

                break;

            default:
                break;
        }
    }

    private float GetVerticalOffset(GameObject first, GameObject second)
    {
        float firstHeight = first.GetComponent<Collider2D>().bounds.extents.y;
        float secondHeight = second.GetComponent<Collider2D>().bounds.extents.y;

        return (firstHeight - secondHeight) + (secondHeight / 4);
    }
}
