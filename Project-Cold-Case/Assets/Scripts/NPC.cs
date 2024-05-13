using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float myZLevel = -1.1f;
    public TextMeshProUGUI inspectText;
    private Animator Anim;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private AudioSource source;
    public GameObject blackOverlay;
    public GameObject quitButton;

    void Start()
    {
        Anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();

        inspectText.text = "";
        inspectText.gameObject.SetActive(false);

        StartCoroutine(StartGameFadeIn(1));

        Cursor.visible = false;
    }

    void Update()
    {
        if (isMoving)
        {
            if ((Vector2)transform.position == targetPosition)
            {
                HandleArrival();
            }

            Move();
        }
    }

    private void MoveToLocation(Vector2 target)
    {
        targetPosition = target;

        isMoving = true;
        source.Play();
        Anim.SetBool("isMoving", true);
    }

    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

        transform.position = new Vector3(transform.position.x, transform.position.y, myZLevel);
    }

    private void HandleArrival()
    {
        isMoving = false;
        source.Stop();
        Anim.SetBool("isMoving", false);
    }

    private IEnumerator StartGameFadeIn(float time)
    {
        yield return new WaitForEndOfFrame();


        SpriteRenderer fader = blackOverlay.GetComponent<SpriteRenderer>();
        Color currentColor = fader.color;

        while (fader.color.a > 0)
        {
            yield return new WaitForEndOfFrame();
            currentColor.a -= Time.deltaTime / time;
            fader.color = currentColor;
        }

        blackOverlay.SetActive(false);
        MoveToLocation(new Vector2(-5.15999985f, -1.33000004f));

        yield return new WaitForSeconds(4.5f);

        StartCoroutine(PrintInspectText("This is Alex's apartment, let's see what I can figure out from the scene."));

        yield return new WaitForSeconds(2);

        blackOverlay.SetActive(true);
        while (fader.color.a < 1)
        {
            yield return new WaitForEndOfFrame();
            currentColor.a += Time.deltaTime / time;
            fader.color = currentColor;
        }

        yield return new WaitForSeconds(1);

        Cursor.visible = true;
        quitButton.SetActive(true);
    }

    IEnumerator PrintInspectText(string printText)
    {
        inspectText.gameObject.SetActive(true);

        string currentText = "";

        float printSpeed = .05f;

        for (int i = 0; i < printText.Length; i++)
        {
            currentText += printText[i];

            inspectText.text = currentText;

            yield return new WaitForSeconds(printSpeed);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
