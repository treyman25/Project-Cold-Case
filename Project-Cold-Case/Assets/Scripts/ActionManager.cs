using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    private int actionCount = 0;
    private int maxActions = 3;

    public GameObject[] actionUI;

    private Player player;

    private GameObject[] allObjects;

    private GameObject object1;
    private GameObject object2;
    private GameObject object3;

    private string action1;
    private string action2;
    private string action3;


    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        allObjects = player.GetObjects();
    }

    void Update()
    {
        checkUsedAllActions();
    }

    private void checkUsedAllActions()
    {
        if (actionCount >= maxActions)
        {
            player.ChangeToPresent();
            actionCount = 0;
        }
    }

    public void UsedAction(GameObject passedObject, string action)
    {
        switch (actionCount)
        {
            case 0:
                object1 = passedObject;
                action1 = action;
                break;
            case 1:
                object2 = passedObject;
                action2 = action;
                break;
            case 2:
                object3 = passedObject;
                action3 = action;
                break;
            default:
                break;
        }

        if (actionCount < actionUI.Length)
        {
            actionUI[actionCount].SetActive(true);
        }

        actionCount++;
    }

    public void ResetActions()
    {
        foreach (var indicator in actionUI)
        {
            indicator.SetActive(false);
        }

        actionCount = 0;

        if (object1 != null && action1 != null)
        {
            Debug.Log("Object 1: " + object1 + " ... " + action1);
            Debug.Log("Object 2: " + object2 + " ... " + action2);
            Debug.Log("Object 3: " + object3 + " ... " + action3);
        }

        object1 = null;
        object2 = null;
        object3 = null;
        action1 = "";
        action2 = "";
        action3 = "";
    }

    public void ApplyCombos()
    {
        GameObject knife = allObjects[1];
        GameObject timeMachine = allObjects[0];

        if (!knife.GetComponent<Object>().IsHidden())
        {
            if (knife.transform.position.x < timeMachine.transform.position.x)
            {
                knife.GetComponent<Object>().ApplySpecialComboId(2);
                player.PlaceBlood();
            }
            else
            {
                knife.GetComponent<Object>().ApplySpecialComboId(1);
            }
        }

        GameObject loadedTrap = GameObject.Find("Mousetrap_Set(Clone)");
        GameObject cheese = allObjects[2];

        if (loadedTrap != null && loadedTrap.transform.position.y < -3.2f && !player.CanBreak())
        {
            player.TriggerCanBreak();
            loadedTrap.GetComponent<Object>().ApplySpecialComboId(3);
        }
        else if (loadedTrap != null)
        {
            loadedTrap.GetComponent<Object>().ApplySpecialComboId(4);
        } else if (cheese.transform.position.y < -3.2f)
        {
            cheese.GetComponent<Object>().Hide(true);
        }
        else if (cheese.transform.position.x > 7  || cheese.transform.position.x < 4)
        {
            cheese.GetComponent<Object>().ApplySpecialComboId(5);
        }
        else
        {
            cheese.GetComponent<Object>().Hide(true);
        }

        GameObject frame = allObjects[11];

        // Update if moved
        frame.transform.position = new Vector3(0.328000009f, -0.400000006f, 0);

        GameObject books = allObjects[8];

        //if (books.transform.position.x > -1 && books.transform.position.x < 5)
        //{
        //    books.GetComponent<Object>().Hide(true);
        //    player.ScatterBooks();
        //}

        GameObject pickles = allObjects[13];
        pickles.GetComponent<Object>().Hide(true);
    }
}
