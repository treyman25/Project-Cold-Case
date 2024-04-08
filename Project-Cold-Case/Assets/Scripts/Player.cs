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
    [SerializeField] private float floorY = -3.5f;
    [SerializeField] private float leftBound = -8;
    [SerializeField] private float rightBound = 16;

    // Objects
    public GameObject[] objects;

    // Utility
    private Vector3 mousePosition;
    private bool hasClicked = false;
    private bool canClick = true;

    // Character Movement
    private Animator Anim;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private Vector3 savedPosition;
    private bool isFacingRight = true;

    // Object Movement
    private Collider2D clickedCollider;
    private GameObject clickedObject;
    private Vector3 offset;
    private bool hasChosenMove = false;
    private bool isHoldingObject = false;

    // Object Breaking
    private bool canBreak = false;

    // Menus
    public GameObject myCanvas;
    public GameObject inspectButton;
    public GameObject moveButton;
    public GameObject breakButton1;
    public GameObject breakButton2;
    public GameObject goBackButton;
    public GameObject openFridgeButton;

    // Time Travel
    public GameObject darkOverlay;
    public GameObject dayWindow1;
    public GameObject dayWindow2;
    private bool inPast = false;
    private ActionManager AM;
    public GameObject transitionPast;
    public GameObject transitionPresent;

    // Text
    public TextMeshProUGUI floorText;

    // Special Objects
    public GameObject fridge;
    public GameObject fridgeInterior;
    private bool fridgeOpen = false;
    public GameObject fixedTimeMachine;
    public GameObject timeMachine;

    // Sound
    private AudioSource source;
    public AudioClip pastClip;
    public AudioClip presentClip;
    public AudioClip pickupClip;
    public AudioClip putdownClip;
    public AudioClip fridgeClip;


    void Start()
    {
        Anim = GetComponent<Animator>();
        AM = GameObject.Find("ActionManager").GetComponent<ActionManager>();
        source = GetComponent<AudioSource>();

        ApplyStartCondition();
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

        isMoving = true;
        source.Play();
        Anim.SetBool("isMoving", true);
    }

    private void HandleArrival()
    {
        if (clickedObject != null)
        {
            Turn(clickedObject.transform.position.x);

            DisplayButtons();

            if (isHoldingObject)
            {
                isHoldingObject = false;
                AudioSource.PlayClipAtPoint(putdownClip, transform.position, 6f);

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
        source.Stop();
        Anim.SetBool("isMoving", false);
    }

    private bool CheckClickedToMove()
    {
        return hasChosenMove == false && isMoving == false && canMove == true && mousePosition.x > leftBound && mousePosition.x < rightBound;
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
        clickedObject.GetComponent<Object>().SetTransparency(1f);

        clickedObject = null;

        isHoldingObject = false;
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

            if (clickedObject.GetComponent<Object>().IsMovable() && inPast)
            {
                clickedObject.GetComponent<Object>().SetMoving(true);
            }

            savedPosition = clickedObject.transform.position;

            offset = clickedObject.transform.position - mousePosition;

            float targetX = clickedObject.GetComponent<Object>().GetInteractTarget(transform.position.x);
            targetPosition = new Vector2(targetX, transform.position.y);

            isMoving = true;
            source.Play();
            Anim.SetBool("isMoving", true);
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

            float objectHeight = clickedObject.GetComponent<BoxCollider2D>().bounds.size.y;

            objectCenter = new Vector3(objectCenter.x, objectCenter.y - (objectHeight/2), objectCenter.z);

            if (objectCenter.x > 11 && objectCenter.x < 15 && mousePosition.y > -2f)
            {
                clickedObject.transform.position = new Vector3(originalPosition.x, -1.75f + (objectHeight/2), originalPosition.z);
            }
            else if (objectCenter.x > -.4 && objectCenter.x < 3.4)
            {
                if (mousePosition.y > 0)
                {
                    clickedObject.transform.position = new Vector3(originalPosition.x, .457f + (objectHeight / 2), originalPosition.z);
                }
                else if (mousePosition.y > -1)
                {
                    clickedObject.transform.position = new Vector3(originalPosition.x, -.68f + (objectHeight / 2), originalPosition.z);
                }
                else if (mousePosition.y > -2)
                {
                    clickedObject.transform.position = new Vector3(originalPosition.x, -1.9f + (objectHeight / 2), originalPosition.z);
                }
                else
                {
                    clickedObject.transform.position = new Vector3(originalPosition.x, floorY + (objectHeight / 2), originalPosition.z);
                }
            }
            else
            {
                clickedObject.transform.position = new Vector3(originalPosition.x, floorY + (objectHeight/2), originalPosition.z);
            }
        }
    }

    private void Move()
    {
        Turn(targetPosition.x);

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

        transform.position = new Vector3(transform.position.x, transform.position.y, myZLevel);
    }

    private void Turn (float targetX)
    {
        Vector3 myScale = transform.localScale;
        Vector3 canvasScale = myCanvas.transform.localScale;

        if (targetX < transform.position.x && isFacingRight)
        {
            isFacingRight = false;
            myScale.x *= -1;
            canvasScale.x *= -1;
            inspectButton.transform.Translate(-3.5f, 0, 0);
            moveButton.transform.Translate(-3.5f, 0, 0);
            breakButton1.transform.Translate(-3.5f, 0, 0);
            breakButton2.transform.Translate(-3.5f, 0, 0);
            goBackButton.transform.Translate(-3.5f, 0, 0);
            openFridgeButton.transform.Translate(-3.5f, 0, 0);
        }
        else if (targetX > transform.position.x && !isFacingRight)
        {
            isFacingRight = true;
            myScale.x *= -1;
            canvasScale.x *= -1;
            inspectButton.transform.Translate(3.5f, 0, 0);
            moveButton.transform.Translate(3.5f, 0, 0);
            breakButton1.transform.Translate(3.5f, 0, 0);
            breakButton2.transform.Translate(3.5f, 0, 0);
            goBackButton.transform.Translate(3.5f, 0, 0);
            openFridgeButton.transform.Translate(3.5f, 0, 0);
        }

        transform.localScale = myScale;
        myCanvas.transform.localScale = canvasScale;
    }

    private void MoveObject()
    {
        if (mousePosition.x + offset.x < 16.2 && mousePosition.x + offset.x > - 8)
        {
            clickedObject.transform.position = new Vector3(mousePosition.x + offset.x, clickedObject.transform.position.y, clickedObject.transform.position.z);
        }

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
        isHoldingObject = true;
        AudioSource.PlayClipAtPoint(pickupClip, transform.position, 1);
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
        openFridgeButton.SetActive(false);
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

        if (clickedObject.CompareTag("Fridge") && !fridgeOpen && inPast)
        {
            if (hasChosenMove == false)
            {
                openFridgeButton.SetActive(true);
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

        transitionPast.SetActive(true);
        transitionPast.GetComponent<Animator>().SetTrigger("PlayTransition");
        AudioSource.PlayClipAtPoint(pastClip, Camera.main.transform.position, .05f);

        yield return new WaitForSeconds(1.5f);

        transitionPast.SetActive(false);

        inPast = true;

        foreach (var item in objects)
        {
            if (!item.CompareTag("TimeMachine"))
            {
                item.GetComponent<Object>().ResetObject();
            }
        }

        AM.ResetActions();

        ShowDayDecor(true);

        canClick = true;

        CloseFridge();
    }

    IEnumerator PresentTransition()
    {
        canClick = false;

        yield return new WaitForSeconds(.5f);

        transitionPresent.SetActive(true);
        transitionPresent.GetComponent<Animator>().SetTrigger("PlayTransition");
        AudioSource.PlayClipAtPoint(presentClip, Camera.main.transform.position, .1f);

        yield return new WaitForSeconds(1.5f);

        transitionPresent.SetActive(false);

        inPast = false;

        ShowDayDecor(false);

        canClick = true;

        AM.ApplyCombos();
        AM.ResetActions();

        if (fridgeOpen)
        {
            CloseFridge();
        }

        objects[5].GetComponent<Object>().Hide(true);
    }

    private void ShowDayDecor(bool value)
    {
        darkOverlay.SetActive(!value);
        dayWindow1.SetActive(value);
        dayWindow2.SetActive(value);
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
            fixedTimeMachine.SetActive(true);
            timeMachine.SetActive(false);
            canBreak = true;
        }
    }

    public float GetSpeed()
    {
        return speed;
    }

    public bool CanBreak()
    {
        return canBreak;
    }

    public bool IsHoldingObject()
    {
        return isHoldingObject;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public void OpenFridge()
    {
        fridgeOpen = true;
        AudioSource.PlayClipAtPoint(fridgeClip, transform.position, .4f);
        fridgeInterior.SetActive(true);
        AM.UsedAction(fridge, "Opened");
        objects[2].GetComponent<Object>().Hide(false);
        DeselectObject();
    }

    public void CloseFridge()
    {
        fridgeOpen = false;
        fridgeInterior.SetActive(false);
    }

    private void ApplyStartCondition()
    {
        objects[5].GetComponent<Object>().Hide(true);
        objects[1].GetComponent<Object>().ApplySpecialComboId(2);
        objects[2].GetComponent<Object>().Hide(true);
    }
}
