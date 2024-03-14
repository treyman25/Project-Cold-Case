using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    // Serialize Fields
    [SerializeField] private float speed = 5;
    [SerializeField] private float myZLevel = -1.1f;
    [SerializeField] private bool canMove = true;

    // Objects
    public GameObject[] objects;

    // Utility
    public GameObject eye;
    private Vector3 mousePosition;
    private bool hasClicked = false;
    private bool canClick = true;

    // Character Movement
    public GameObject targetIndicator;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private Vector3 savedPosition;
    private GameObject myIndicator;

    // Object Movement
    public GameObject heldObject;
    private Collider2D clickedCollider;
    private GameObject clickedObject;
    private Vector3 offset;
    private bool hasChosenMove = false;

    // Object Breaking
    private bool canBreak = false;

    // Menus
    public GameObject inspectButton;
    public GameObject moveButton;
    public GameObject breakButton1;
    public GameObject breakButton2;
    public GameObject goBackButton;

    // Time Travel
    public GameObject pastBackground;
    private bool inPast = false;
    private ActionManager AM;

    // Text
    public TextMeshProUGUI floorText;


    void Start()
    {
        AM = GameObject.Find("ActionManager").GetComponent<ActionManager>();
    }

    void Update()
    {
        UpdateMousePosition();

        if (Input.GetMouseButtonDown(0) && canClick)
        {
            ProcessClick();
        }

        UpdateEyes();

        // Can't move until destination is reached
        if (isMoving)
        {
            // If destination is reached
            if ((Vector2)transform.position == targetPosition)
            {
                HandleArrival();
            }

            // Stops movement on Start
            if (hasClicked)
            {
                Move();
            }
        }

        // If object has been clicked on
        if (clickedObject != null & !isMoving && hasChosenMove)
        {
            MoveObject();
        }
    }

    private void UpdateMousePosition()
    {
        // Records the mouse position
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void UpdateEyes()
    {
        // Eyeball Movement
        eye.transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - new Vector3(transform.position.x, transform.position.y));
    }

    IEnumerator Pulse()
    {
        // Make bigger
        transform.localScale = transform.localScale * 1.1f;

        // Wait
        yield return new WaitForSeconds(.1f);

        // Make smaller
        transform.localScale = transform.localScale / 1.1f;
    }

    private void MoveToLocation(Vector2 target)
    {
        // Sets target location
        targetPosition = target;

        // Creates a little target indicator
        myIndicator = Instantiate(targetIndicator, new Vector3(targetPosition.x, targetPosition.y, 1), Quaternion.identity);

        // Start moving
        isMoving = true;
    }

    private void HandleArrival()
    {
        // If there is still an indicator
        if (myIndicator != null)
        {
            // Get rid of it
            Destroy(myIndicator);
        }

        // If an object has been clicked
        if (clickedObject != null)
        {
            // Display the appropriate buttons
            DisplayButtons();

            // If placing, not picking up
            if (heldObject.activeSelf)
            {
                // Toggle held object
                heldObject.SetActive(!heldObject.activeSelf);

                // Make opaque
                clickedObject.GetComponent<Object>().SetTransparency(1f);

                if (!clickedObject.GetComponent<Object>().HasBeenMoved())
                {
                    AM.UsedAction(clickedObject, "Moved");
                    clickedObject.GetComponent<Object>().Moved();
                }

                // Release object
                DeselectObject();
            }
        }

        // Trigger pulse for feedback
        StartCoroutine(Pulse());

        // Stop moving
        isMoving = false;
    }

    private bool CheckClickedToMove()
    {
        return hasChosenMove == false && isMoving == false && canMove == true && mousePosition.x > -8 && mousePosition.x < 8;
    }

    private bool CheckClickedOnObject()
    {
        return hasChosenMove == false && clickedCollider != null && clickedCollider.transform.gameObject.GetComponent<Object>() != null && !isMoving;
    }

    private bool CheckClickedToPlaceObject()
    {
        return clickedObject != null && !isMoving && hasChosenMove;
    }

    private bool CheckMouseOverButton()
    {

        Button b = null;

        if (clickedCollider != null)
        {
            b = clickedCollider.transform.gameObject.GetComponent<Button>();
        }

        return b != null;
    }

    private void DropObject()
    {
        // Make opaque
        clickedObject.GetComponent<Object>().SetTransparency(1f);

        // Release object
        clickedObject = null;

        // Toggle held object
        heldObject.SetActive(!heldObject.activeSelf);
    }

    private void DeselectObject()
    {
        HideButtons();
        clickedObject = null;
    }

    private void ProcessClick()
    {

        // Grab any overlapping collider
        clickedCollider = Physics2D.OverlapPoint(mousePosition);

        if (CheckMouseOverButton())
        {

        }
        // Clicking on an object for the first time
        else if (CheckClickedOnObject())
        {
            HideButtons();

            // Get the object
            clickedObject = clickedCollider.transform.gameObject;

            // Start the object's movement ability
            if (clickedObject.GetComponent<Object>().IsMovable())
            {
                clickedObject.GetComponent<Object>().SetMoving(true);
            }

            // Save the objects most recent stagnant position
            savedPosition = clickedObject.transform.position;

            // Save the offset between the mouse and the object's origin
            offset = clickedObject.transform.position - mousePosition;

            // Set new target position
            float targetX = clickedObject.GetComponent<Object>().GetInteractTarget(transform.position.x);
            targetPosition = new Vector2(targetX, transform.position.y);

            // Creates a little target indicator
            myIndicator = Instantiate(targetIndicator, new Vector3(targetPosition.x, targetPosition.y, 1), Quaternion.identity);

            // Start moving
            isMoving = true;
        }
        // Placing the object
        else if (CheckClickedToPlaceObject())
        {
            // Stop clicked object movement
            clickedObject.GetComponent<Object>().SetMoving(false);

            // If the released object is on another object
            if (clickedObject.GetComponent<Object>().IsOverlapping())
            {
                // Return it to its last saved position
                clickedObject.transform.position = savedPosition;

                DropObject();

                hasChosenMove = false;
            }
            else
            {
                // Move to chosen location
                float targetX = clickedObject.GetComponent<Object>().GetInteractTarget(transform.position.x);
                MoveToLocation(new Vector2(targetX, transform.position.y));
            }
        }
        // Clicking on open space
        else if (CheckClickedToMove())
        {
            DeselectObject();

            MoveToLocation(new Vector2(mousePosition.x, transform.position.y));
        }

        // Stops automatic movement on Start
        if (!hasClicked)
        {
            hasClicked = true;
        }
    }

    private void AdjustForShelves()
    {
        if (clickedObject != null)
        {
            Vector3 originalPosition = clickedObject.transform.position;
            Vector3 objectCenter = mousePosition + offset;

            if (objectCenter.x > 3 && objectCenter.x < 7 && objectCenter.y > -2.5)
            {
                clickedObject.transform.position = new Vector3(originalPosition.x, -1.75f, originalPosition.z);
            }
            else
            {
                clickedObject.transform.position = new Vector3(originalPosition.x, -3.84f, originalPosition.z);
            }
        }
    }

    private void Move()
    {
        // Move towards target
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

        // Stay in front
        transform.position = new Vector3(transform.position.x, transform.position.y, myZLevel);
    }

    private void MoveObject()
    {
        // Move the object with the mouse on the x axis
        clickedObject.transform.position = new Vector3(mousePosition.x + offset.x, clickedObject.transform.position.y, clickedObject.transform.position.z);

        AdjustForShelves();

        // Makes the object transparent
        if (clickedObject.GetComponent<Object>().GetTransparency() != .5f)
        {
            // Make half transparent
            clickedObject.GetComponent<Object>().SetTransparency(.5f);
        }
    }

    public void ChooseMoveObject()
    {
        hasChosenMove = true;
        HideButtons();
        heldObject.SetActive(true);
    }

    public void ChooseInspectObject()
    {
        string printText = "";

        if (clickedObject != null)
        {
            printText = clickedObject.GetComponent<Object>().GetInspectText();
        }

        StartCoroutine(PrintInspectText(printText));
        DeselectObject();
    }

    public void ChooseBreakObject()
    {
        clickedObject.GetComponent<Object>().Break();
        AM.UsedAction(clickedObject, "Broken");
        DeselectObject();
    }

    IEnumerator PrintInspectText(string printText)
    {
        canClick = false;

        string currentText = "";

        for (int i = 0; i < printText.Length; i++)
        {
            currentText += printText[i];

            floorText.text = currentText;

            yield return new WaitForSeconds(.05f);
        }

        canClick = true;

        yield return new WaitForSeconds(2);

        floorText.text = "";
    }

    private void HideButtons()
    {
        moveButton.SetActive(false);
        inspectButton.SetActive(false);
        breakButton1.SetActive(false);
        breakButton2.SetActive(false);
        goBackButton.SetActive(false);
    }

    private void DisplayButtons()
    {
        if (clickedObject.GetComponent<Object>().IsMovable() && inPast)
        {
            if (hasChosenMove == false)
            {
                moveButton.SetActive(true);
            }
            else
            {
                hasChosenMove = false;
            }
        }

        if (clickedObject.GetComponent<Object>().IsInspectable())
        {
            inspectButton.SetActive(true);
        }

        if (clickedObject.GetComponent<Object>().IsBreakable() && inPast && canBreak)
        {
            if (clickedObject.GetComponent<Object>().IsMovable())
            {
                breakButton1.SetActive(true);
            }
            else
            {
                breakButton2.SetActive(true);
            }

        }

        if (clickedObject.CompareTag("TimeMachine"))
        {
            if (hasChosenMove == false)
            {
                goBackButton.SetActive(true);
            }
        }
    }

    public void ChangeToPresent()
    {
        Debug.Log("Present");
        StartCoroutine(PresentTransition());
    }

    public void ChangeToPast()
    {
        Debug.Log("Past");
        DeselectObject();
        StartCoroutine(PastTransition());
    }

    IEnumerator PastTransition()
    {
        canClick = false;

        goBackButton.SetActive(false);

        yield return new WaitForSeconds(1);

        inPast = true;

        foreach (var item in objects)
        {
            item.GetComponent<Object>().ResetObject();
        }

        AM.ResetActions();

        pastBackground.SetActive(true);

        canClick = true;
    }

    IEnumerator PresentTransition()
    {
        canClick = false;

        yield return new WaitForSeconds(1);

        inPast = false;

        pastBackground.SetActive(false);

        canClick = true;

        AM.ApplyCombos();
        AM.ResetActions();
    }

    public GameObject[] GetObjects()
    {
        return objects;
    }

    public void TriggerCanBreak()
    {
        if (!canBreak)
        {
            StartCoroutine(PrintInspectText("\"You feel stronger. Like you can break things.\""));
            canBreak = true;
        }
    }
}
