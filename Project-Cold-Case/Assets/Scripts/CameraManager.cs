using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject player;

    private float camX;
    private float playerX;
    private float speed;

    private bool isMoving = false;

    void Start()
    {
        speed = player.GetComponent<Player>().GetSpeed();
    }

    void Update()
    {
        camX = transform.position.x;

        playerX = player.transform.position.x;

        if (camX - playerX > 5 || camX - playerX < -5)
        {
            isMoving = true;
        }

        if (isMoving)
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
                isMoving = false;
            }
        }
    }
}
