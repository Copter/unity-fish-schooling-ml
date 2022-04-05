using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControl : MonoBehaviour
{
    private Vector3 dragOrigin;
    private Vector3 diff;
    private bool drag = false;

    public GameObject followWho;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (followWho != null)
        {
            transform.position = new Vector3(followWho.transform.position.x, followWho.transform.position.y, transform.position.z);
        }

        if (Input.GetMouseButton(0))
        {
            diff = Camera.main.ScreenToViewportPoint(Input.mousePosition) - Camera.main.transform.position;
            if (!drag)
            {
                dragOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                drag = true;
            }

            print("dragOrigin: " + dragOrigin + "; diff: " + diff);
        }
        else
        {
            drag = false;
        }

        if (drag)
        {
            Camera.main.transform.position = dragOrigin - diff;
        }

        //if (Input.GetMouseButton(1))
        //{
        Camera.main.orthographicSize -= Input.mouseScrollDelta.y;
        //}

        if (Input.GetMouseButton(1))    followWho = null;

    }
}
