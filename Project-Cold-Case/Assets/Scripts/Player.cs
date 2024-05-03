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
    public GameObject blackOverlay;
    private Vector3 mousePosition;
    private bool hasClicked = false;
    private bool canClick = false;
    private SpriteRenderer SR;
    public Texture2D objectCursor;
    private bool isDefaultCursor = true;

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
    public GameObject openLockboxButton;
    public GameObject inspectPageButton;

    // Time Travel
    private bool inPast = false;
    private ActionManager AM;
    public GameObject transitionPast;
    public GameObject transitionPresent;
    public GameObject HUD;

    // Text
    public TextMeshProUGUI inspectText;
    public TextMeshProUGUI ObjectText1;
    public TextMeshProUGUI ObjectText2;
    public TextMeshProUGUI ObjectText3;
    private bool finishedTextOnScreen = false;
    private bool isPrinting = false;

    // Special Objects
    public GameObject fridge;
    public GameObject fridgeInterior;
    private bool fridgeOpen = false;
    public GameObject fixedTimeMachine;
    public GameObject timeMachine;
    private bool hasPrinted = false;

    // Sound
    private AudioSource source;
    public AudioClip pastClip;
    public AudioClip presentClip;
    public AudioClip pickupClip;
    public AudioClip putdownClip;
    public AudioClip fridgeClip;

    // Decor
    public GameObject darkOverlay;
    public GameObject dayWindow1;
    public GameObject dayWindow2;
    public GameObject floorBlood;
    public GameObject fridgeBlood;
    public GameObject scatteredBooks;
    public GameObject tape;

    // Dialogues
    private bool canBreakText = false;
    private bool firstPastText = false;
    private bool firstPresentText = false;
    private bool printStartText = true;

    // Pausing
    public GameObject pauseMenu;
    public GameObject quitMenu;
    private bool couldClick = false;
    private bool isPaused = false;

    // Overlays
    public GameObject printedOverlay;

    void Start()
    {
        Anim = GetComponent<Animator>();
        AM = GameObject.Find("ActionManager").GetComponent<ActionManager>();
        SR = GetComponent<SpriteRenderer>();
        source = GetComponent<AudioSource>();

        Cursor.visible = false;

        StartCoroutine(StartGameFadeIn(1));

        inspectText.text = "";
        inspectText.gameObject.SetActive(false);
        ObjectText1.gameObject.SetActive(false);
        ObjectText2.gameObject.SetActive(false);
        ObjectText3.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateMousePosition();

        if (Input.GetMouseButtonDown(0) && canClick)
        {
            ProcessClick();
        }

        if (Input.GetMouseButtonDown(0) && finishedTextOnScreen)
        {
            EraseInspectText();
            CloseOverlay();
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }
    }

    private void UpdateMousePosition()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        ObjectCursor(CheckMouseOverObject());
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
                Anim.SetTrigger("Place");

                clickedObject.GetComponent<Object>().SetTransparency(1f);

                if (!clickedObject.GetComponent<Object>().HasBeenMoved())
                {
                    AM.UsedAction(clickedObject, "Moved");
                    
                }

                clickedObject.GetComponent<Object>().Moved();

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

    private bool CheckMouseOverObject()
    {
        Collider2D touchedCollider = Physics2D.OverlapPoint(mousePosition);
        Object o = null;

        if (touchedCollider != null)
        {
            o = touchedCollider.transform.gameObject.GetComponent<Object>();
        }

        return o != null;
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
            openLockboxButton.transform.Translate(-3.5f, 0, 0);
            inspectPageButton.transform.Translate(-3.5f, 0, 0);
            ObjectText1.transform.Translate(-3.5f, 0, 0);
            ObjectText2.transform.Translate(-3.5f, 0, 0);
            ObjectText3.transform.Translate(-3.5f, 0, 0);
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
            openLockboxButton.transform.Translate(3.5f, 0, 0);
            inspectPageButton.transform.Translate(3.5f, 0, 0);
            ObjectText1.transform.Translate(3.5f, 0, 0);
            ObjectText2.transform.Translate(3.5f, 0, 0);
            ObjectText3.transform.Translate(3.5f, 0, 0);
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
        if (clickedObject.CompareTag("Dumbbell") && !canBreak)
        {
            StartCoroutine(PrintInspectText("I don't feel strong enough to move that right now."));
            DeselectObject();
            return;
        }
        clickedObject.GetComponent<Object>().SetMoving(true);
        hasChosenMove = true;
        HideButtons();
        isHoldingObject = true;
        AudioSource.PlayClipAtPoint(pickupClip, transform.position, 1);
        Anim.SetTrigger("Grab");
    }

    public void ChooseInspectObject()
    {
        string printText = "";
        int id = 0;

        if (!inPast)
        {
            id = 1;
        }


        if (clickedObject != null)
        {
            printText = clickedObject.GetComponent<Object>().GetInspectText(id);
        }

        StartCoroutine(PrintInspectText(printText));
        DeselectObject();
    }

    public void ChooseBreakObject()
    {
        if (canBreak)
        {
            clickedObject.GetComponent<Object>().Break();
            AM.UsedAction(clickedObject, "Broken");
        }
        else
        {
            string printText = "";

            if (clickedObject != null)
            {
                printText = "I don't feel strong enough to break that right now.";
            }

            StartCoroutine(PrintInspectText(printText));
        }
        DeselectObject();
    }

    IEnumerator PrintInspectText(string printText)
    {
        canClick = false;
        isPrinting = true;

        inspectText.gameObject.SetActive(true);

        string currentText = "";

        float printSpeed = .05f;

        for (int i = 0; i < printText.Length; i++)
        {
            printSpeed = .05f;

            if (Input.GetMouseButton(0))
            {
                printSpeed = .025f;
            }

            currentText += printText[i];

            inspectText.text = currentText;

            yield return new WaitForSeconds(printSpeed);
        }

        isPrinting = false;
        finishedTextOnScreen = true;
    }

    private void EraseInspectText()
    {
        canClick = true;

        inspectText.text = "";
        inspectText.gameObject.SetActive(false);

        finishedTextOnScreen = false;
    }

    private void HideButtons()
    {
        moveButton.SetActive(false);
        inspectButton.SetActive(false);
        breakButton1.SetActive(false);
        breakButton2.SetActive(false);
        goBackButton.SetActive(false);
        openFridgeButton.SetActive(false);
        openLockboxButton.SetActive(false);
        inspectPageButton.SetActive(false);

        ObjectText1.gameObject.SetActive(false);
        ObjectText2.gameObject.SetActive(false);
        ObjectText3.gameObject.SetActive(false);
    }

    private void DisplayButtons()
    {
        int numButtons = 0;
        if (clickedObject.GetComponent<Object>().IsMovable() && inPast)
        {
            if (hasChosenMove == false)
            {
                moveButton.SetActive(true);
                numButtons++;
            }
            else
            {
                hasChosenMove = false;
            }
        }

        if (clickedObject.GetComponent<Object>().IsInspectable())
        {
            inspectButton.SetActive(true);
            numButtons++;
        }

        if (clickedObject.GetComponent<Object>().IsBreakable() && inPast)
        {
            if (clickedObject.GetComponent<Object>().IsMovable())
            {
                breakButton1.SetActive(true);
                numButtons++;
            }
            else
            {
                breakButton2.SetActive(true);
                numButtons++;
            }

        }

        if (clickedObject.CompareTag("Machine"))
        {
            if (hasChosenMove == false)
            {
                goBackButton.SetActive(true);
                numButtons++;
            }
        }

        if (clickedObject.CompareTag("Fridge") && !fridgeOpen && inPast)
        {
            if (hasChosenMove == false)
            {
                openFridgeButton.SetActive(true);
                numButtons++;
            }
        }

        if (clickedObject.CompareTag("Lockbox") && inPast)
        {
            if (hasChosenMove == false)
            {
                openLockboxButton.SetActive(true);
                numButtons++;
            }
        }

        if (clickedObject.CompareTag("Printer") && !clickedObject.GetComponent<Object>().IsInspectable())
        {
            if (hasChosenMove == false)
            {
                inspectPageButton.SetActive(true);
                numButtons++;
            }
        }

        switch (numButtons)
        {
            case 1:
                ObjectText1.gameObject.SetActive(true);
                ObjectText1.text = clickedObject.tag;
                break;
            case 2:
                ObjectText2.gameObject.SetActive(true);
                ObjectText2.text = clickedObject.tag;
                break;
            case 3:
                ObjectText3.gameObject.SetActive(true);
                ObjectText3.text = clickedObject.tag;
                break;
            default:
                break;
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

        if (!canBreak)
        {
            speed = 3.5f;
            SetTransparency(.8f);
        }

        transitionPast.SetActive(false);

        inPast = true;

        foreach (var item in objects)
        {
            if (!item.CompareTag("Machine"))
            {
                item.GetComponent<Object>().ResetObject();
            }
        }

        HUD.SetActive(true);
        AM.ResetActions();

        ShowDayDecor(true);
        floorBlood.SetActive(false);
        fridgeBlood.SetActive(false);
        scatteredBooks.SetActive(false);

        CloseFridge();

        if (!firstPastText)
        {
            StartCoroutine(PrintInspectText("???"));

            StartCoroutine(LookAround());

            while (isPrinting || finishedTextOnScreen)
            {
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(PrintInspectText("What happened to me? The crime scene is different, " +
                "as if no crime was committed."));

            while (isPrinting || finishedTextOnScreen)
            {
                yield return new WaitForEndOfFrame();
            }

            Turn(transform.position.x - 2 * (7 - transform.position.x));

            StartCoroutine(PrintInspectText("Based on the fact I was only told he was an ex-government " +
                "employee and the oddities of his living quarters..."));

            while (isPrinting || finishedTextOnScreen)
            {
                yield return new WaitForEndOfFrame();
            }

            Turn(7);

            StartCoroutine(PrintInspectText("He must've been working on time travel, " +
                "and this is the fruits of his labor."));

            while (isPrinting || finishedTextOnScreen)
            {
                yield return new WaitForEndOfFrame();
            }

            Turn(transform.position.x - 2 * (7 - transform.position.x));

            StartCoroutine(PrintInspectText("Let's see what I can do now that I've been sent back in time."));

            firstPastText = true;
        }
        else
        {

            canClick = true;
        }
    }

    IEnumerator PresentTransition()
    {
        canClick = false;

        yield return new WaitForSeconds(.5f);

        transitionPresent.SetActive(true);
        transitionPresent.GetComponent<Animator>().SetTrigger("PlayTransition");
        AudioSource.PlayClipAtPoint(presentClip, Camera.main.transform.position, .1f);

        yield return new WaitForSeconds(1.5f);

        speed = 5;
        SetTransparency(1);

        transitionPresent.SetActive(false);

        inPast = false;

        ShowDayDecor(false);

        canClick = true;

        AM.ApplyCombos();
        AM.ResetActions();
        HUD.SetActive(false);

        if (fridgeOpen)
        {
            CloseFridge();
        }

        if (!firstPresentText)
        {
            StartCoroutine(PrintInspectText("That's going to take some getting used to..."));

            while (isPrinting || finishedTextOnScreen)
            {
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(PrintInspectText("That aside, let's see how things have changed based on what I've done."));

            firstPresentText = true;
        }
        else if (canBreak && !canBreakText)
        {
            StartCoroutine(PrintInspectText("Alex must have fixed this wire before he was killed."));
            canBreakText = true;
        }
    }

    private void ShowDayDecor(bool value)
    {
        darkOverlay.SetActive(!value);
        dayWindow1.SetActive(value);
        dayWindow2.SetActive(value);
        tape.SetActive(!value);
    }

    public GameObject[] GetObjects()
    {
        return objects;
    }

    public void TriggerCanBreak()
    {
        if (!canBreak)
        {
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
        objects[2].transform.Translate(0, 0, -3);
        objects[13].GetComponent<Object>().Hide(false);
        objects[13].transform.Translate(0, 0, -3);
        DeselectObject();
    }

    public void CloseFridge()
    {
        fridgeOpen = false;
        fridgeInterior.SetActive(false);
    }

    public void OpenLockbox()
    {
        if (hasPrinted)
        {
            StartCoroutine(PrintInspectText("The code worked!"));
            objects[5].GetComponent<Object>().ApplySpecialComboId(7);
            AM.UsedAction(objects[5], "Opened");
        }
        else
        {
            StartCoroutine(PrintInspectText("It has a code but I'm not sure what it is quite yet."));
        }

        DeselectObject();
    }

    private IEnumerator StartGameFadeIn(float time)
    {
        yield return new WaitForEndOfFrame();
        AM.ApplyCombos();

        SpriteRenderer fader = blackOverlay.GetComponent<SpriteRenderer>();
        Color currentColor = fader.color;

        while (fader.color.a > 0)
        {
            yield return new WaitForEndOfFrame();
            currentColor.a -= Time.deltaTime / time;
            fader.color = currentColor;
        }

        blackOverlay.SetActive(false);
        canClick = true;

        if (printStartText)
        {
            StartCoroutine(PrintInspectText("This is Alex's apartment, let's see what I can figure out from the scene."));

            while (isPrinting || finishedTextOnScreen)
            {
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(PrintInspectText("The autopsy says he died from a stab wound, let's piece together what happened here."));
        }
    }

    private void SetTransparency(float percentage)
    {
        Color newColor = SR.color;
        newColor.a = percentage;
        SR.color = newColor;
    }
    public void HasPrinted(bool value)
    {
        hasPrinted = value;
    }

    public void PlaceBlood()
    {
        floorBlood.SetActive(true);
        fridgeBlood.SetActive(true);
    }

    public void ScatterBooks()
    {
        scatteredBooks.SetActive(true);
    }

    private void Pause()
    {
        couldClick = canClick;
        canClick = false;

        pauseMenu.SetActive(true);
        Time.timeScale = 0;

        isPaused = true;
    }

    public void Resume()
    {
        canClick = couldClick;

        if (!isPrinting && !finishedTextOnScreen)
        {
            canClick = true;
        }

        pauseMenu.SetActive(false);
        Time.timeScale = 1;

        isPaused = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ChooseQuit()
    {
        quitMenu.SetActive(true);
    }

    public void CancelQuit()
    {
        quitMenu.SetActive(false);
    }

    private void ObjectCursor(bool overObject)
    {
        if (!canClick)
        {
            Cursor.visible = false;
        }
        else
        {
            if (Cursor.visible == false)
            {
                Cursor.visible = true;
            }

            if (overObject && isDefaultCursor && !isPaused)
            {
                isDefaultCursor = false;
                Cursor.SetCursor(objectCursor, Vector2.zero, CursorMode.Auto);
            }
            else if (!overObject && !isDefaultCursor)
            {
                isDefaultCursor = true;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    IEnumerator LookAround()
    {
        yield return new WaitForSeconds(.5f);

        Turn(transform.position.x - 2 * (7 - transform.position.x));

        yield return new WaitForSeconds(.25f);

        Turn(7);

        yield return new WaitForSeconds(.5f);

        Turn(transform.position.x - 2 * (7 - transform.position.x));

        yield return new WaitForSeconds(.5f);

        Turn(7);
    }

    public void InspectPrintout()
    {
        printedOverlay.SetActive(true);

        string printText = clickedObject.GetComponent<Object>().GetInspectText(0);

        StartCoroutine(PrintInspectText(printText));

        DeselectObject();
    }

    private void CloseOverlay()
    {
        if (printedOverlay.activeSelf == true)
        {
            printedOverlay.SetActive(false);
        }
    }
}
