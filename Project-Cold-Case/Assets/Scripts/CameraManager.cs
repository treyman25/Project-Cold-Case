using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject player;

    private float camX;
    private float playerX;
    private float speed;
    private Vector3 mousePosition;

    private bool isFollowingPlayer = false;
    private bool isFollowingMouse = false;
    private bool playerIsMoving = false;
    private bool playerIsCarrying = false;

    void Start()
    {
        speed = player.GetComponent<Player>().GetSpeed();
    }

    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        camX = transform.position.x;

        playerX = player.transform.position.x;

        playerIsMoving = player.GetComponent<Player>().IsMoving();
        playerIsCarrying = player.GetComponent<Player>().IsHoldingObject();

        if (!playerIsCarrying)
        {
            isFollowingMouse = false;
        }

        if (!isFollowingMouse && (playerIsMoving && camX - playerX > 5 || camX - playerX < -5))
        {
            isFollowingPlayer = true;
        }
        else if (playerIsCarrying && (camX - mousePosition.x > 6 || camX - mousePosition.x < -6))
        {
            isFollowingMouse = true;
        }


        if (isFollowingPlayer)
        {
            if (camX - playerX > 2)
            {
                transform.Translate(Time.deltaTime * -speed, 0, 0);
            }
            else if (camX - playerX < -2)
            {
                transform.Translate(Time.deltaTime * speed, 0, 0);
            }
            else
            {
                isFollowingPlayer = false;
            }
        }

        if (isFollowingMouse)
        {
            if (camX - mousePosition.x > 5 && mousePosition.x > -9)
            {
                transform.Translate(Time.deltaTime * -speed, 0, 0);
            }
            else if (camX - mousePosition.x < -5 && mousePosition.x < 21.8)
            {
                transform.Translate(Time.deltaTime * speed, 0, 0);
            }
        }
    }

    public void MoveToMurder()
    {
        transform.position = new Vector3(5, transform.position.y, transform.position.z);
    }
}
