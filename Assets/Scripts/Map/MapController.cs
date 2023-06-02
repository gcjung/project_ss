using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private float speed = 2.0f;
    private float resetDistance = -9.6f;

    private Vector3 resetPoint;

    private void Awake()
    {
        resetPoint = new Vector3(19.2f, 0, 0);
    }

    private void Update()
    {
        if(PlayerController.CurrentPlayerState == PlayerState.Moving)
        {
            this.transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);

            if (transform.position.x <= resetDistance)
            {
                transform.Translate(resetPoint);
            }

        }        
    }
}
