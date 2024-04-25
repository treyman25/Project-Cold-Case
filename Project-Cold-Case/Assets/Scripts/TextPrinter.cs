using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TextPrinter : MonoBehaviour
{
    public string myText;

    public TextMeshProUGUI myTMPro;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PrintText(myText));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PrintText(string printText)
    {
        yield return new WaitForSeconds(.5f);

        string currentText = "";

        for (int i = 0; i < printText.Length; i++)
        {
            currentText += printText[i];

            myTMPro.text = currentText;

            yield return new WaitForSeconds(.05f);
        }


        yield return new WaitForSeconds(2);

        myTMPro.text = "";

        yield return new WaitForSeconds(.5f);

        SceneManager.LoadScene("SampleScene");
    }
}