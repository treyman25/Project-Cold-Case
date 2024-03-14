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

        if (isMoving)
        {
            if ((Vector2)transform.position == targetPosition)
            {
                HandleArrival();
            }

            if (hasClicked)
            {
                Move();
            }
        }

        if (clickedObject != null & !isMoving && hasChosenMove)
        {
            MoveObject();
        }
    }

    private void UpdateMousePosition()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void MoveToLocation(Vector2 target)
    {
        targetPosition = target;

        myIndicator = Instantiate(targetIndicator, new Vector3(targetPosition.x, targetPosition.y, 1), Quaternion.identity);

        isMoving = true;
    }

    private void HandleArrival()
    {
        if (myIndicator != null)
        {
            Destroy(myIndicator);
        }

        if (clickedObject != null)
        {
            DisplayButtons();

            if (heldObject.activeSelf)
            {
                heldObject.SetActive(!heldObject.activeSelf);

                clickedObject.GetComponent<Object>().SetTransparency(1f);

                if (!clickedObject.GetComponent<Object>().HasBeenMoved())
                {
                    AM.UsedAction(clickedObject, "Moved");
                    clickedObject.GetComponent<Object>().Moved();
                }

                DeselectObject();
            }
        }

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
            Debug.Log(b);
        }

        return b != null;
    }

    private void DropObject()
    {
        clickedObject.GetComponent<Object>().SetTransparency(1f);

        clickedObject = null;

        heldObject.SetActive(!heldObject.activeSelf);
    }

    private void DeselectObject()
    {
        HideButtons();
        clickedObject = null;
    }

    private void ProcessClick()
    {
        clickedCollider = Physics2D.OverlapPoint(mousePosition);

        if (CheckMouseOverButton())
        {

        }
        else if (CheckClickedOnObject())
        {
            HideButtons();

            clickedObject = clickedCollider.transform.gameObject;

            if (clickedObject.GetComponent<Object>().IsMovable())
            {
                clickedObject.GetComponent<Object>().SetMoving(true);
            }

            savedPosition = clickedObject.transform.position;

            offset = clickedObject.transform.position - mousePosition;

            float targetX = clickedObject.GetComponent<Object>().GetInteractTarget(transform.position.x);
            targetPosition = new Vector2(targetX, transform.position.y);

            myIndicator = Instantiate(targetIndicator, new Vector3(targetPosition.x, targetPosition.y, 1), Quaternion.identity);

            isMoving = true;
        }
        else if (CheckClickedToPlaceObject())
        {
            clickedObject.GetComponent<Object>().SetMoving(false);

            if (clickedObject.GetComponent<Object>().IsOverlapping())
            {
                clickedObject.transform.position = savedPosition;

                DropObject();

                hasChosenMove = false;
            }
            else
            {
                float targetX = clickedObject.GetComponent<Object>().GetInteractTarget(transform.position.x);
                MoveToLocation(new Vector2(targetX, transform.position.y));
            }
        }
        else if (CheckClickedToMove())
        {
            DeselectObject();

            MoveToLocation(new Vector2(mousePosition.x, transform.position.y));
        }

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
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

        transform.position = new Vector3(transform.position.x, transform.position.y, myZLevel);
    }

    private void MoveObject()
    {
        clickedObject.transform.position = new Vector3(mousePosition.x + offset.x, clickedObject.transform.position.y, clickedObject.transform.position.z);

        AdjustForShelves();

        if (clickedObject.GetComponent<Object>().GetTransparency() != .5f)
        {
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
