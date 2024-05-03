using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TextPrinter : MonoBehaviour
{
    public string myText1;
    public string myText2;

    public TextMeshProUGUI myTMPro1;
    public TextMeshProUGUI myTMPro2;

    private bool isDone1 = false;
    private bool isDone2 = false;

    private bool isPrinting = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PrintText(myText1, 1));
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPrinting)
        {
            if (Input.GetMouseButtonDown(0) && isDone2)
            {
                StartCoroutine(CloseIntro());
            }
            else if (isDone1 && myTMPro2.text == "")
            {
                StartCoroutine(PrintText(myText2, 2));
            }
        }
    }

    IEnumerator PrintText(string printText, int id)
    {
        isPrinting = true;

        float printSpeed = .05f;

        yield return new WaitForSeconds(.5f);

        string currentText = "";
        TextMeshProUGUI currentTMPro = null;

        switch (id)
        {
            case 1:
                currentTMPro = myTMPro1;
                break;

            case 2:
                currentTMPro = myTMPro2;
                break;

            default:
                break;
        }

        for (int i = 0; i < printText.Length; i++)
        {
            printSpeed = .05f;

            if (Input.GetMouseButton(0))
            {
                printSpeed = .025f;
            }

            currentText += printText[i];

            currentTMPro.text = currentText;

            yield return new WaitForSeconds(printSpeed);
        }

        switch (id)
        {
            case 1:
                isDone1 = true;
                break;

            case 2:
                isDone2 = true;
                break;

            default:
                break;
        }

        isPrinting = false;
    }

    IEnumerator CloseIntro()
    {
        myTMPro1.text = "";
        myTMPro2.text = "";

        yield return new WaitForSeconds(.5f);

        SceneManager.LoadScene("SampleScene");
    }

}
