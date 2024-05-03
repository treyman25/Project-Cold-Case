using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TextPrinter : MonoBehaviour
{
    public string myText;

    public TextMeshProUGUI myTMPro;

    private bool isDone1 = false;

    private bool isDone2 = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PrintText(myText));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isDone1)
        {
            StartCoroutine(CloseIntro());
        }
    }

    IEnumerator PrintText(string printText)
    {
        float printSpeed = .05f;

        yield return new WaitForSeconds(.5f);

        string currentText = "";

        for (int i = 0; i < printText.Length; i++)
        {
            printSpeed = .05f;

            if (Input.GetMouseButton(0))
            {
                printSpeed = .025f;
            }

            currentText += printText[i];

            myTMPro.text = currentText;

            yield return new WaitForSeconds(printSpeed);
        }

        isDone1 = true;
    }

    IEnumerator CloseIntro()
    {
        myTMPro.text = "";

        yield return new WaitForSeconds(.5f);

        SceneManager.LoadScene("SampleScene");
    }

}
