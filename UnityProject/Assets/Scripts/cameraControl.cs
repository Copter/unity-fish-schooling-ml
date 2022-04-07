using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cameraControl : MonoBehaviour
{
    private Vector3 ResetCamera; // original camera position
    private Vector3 Origin; // place where mouse is first pressed
    private Vector3 Difference; // change in position of mouse relative to origin

    public GameObject followWho;

    private float ResetZoom;
    public float framesFollowed = 0;

    public Text uiText;
    public string followName = "";

    // Start is called before the first frame update
    void Start()
    {
        ResetCamera = Camera.main.transform.position;
        ResetZoom = Camera.main.orthographicSize;

        uiText = GameObject.Find("UI Text").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (followWho != null)
        {
            transform.position = new Vector3(followWho.transform.position.x, followWho.transform.position.y, transform.position.z);
            framesFollowed += 1;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (framesFollowed > 1)
            {
                followWho = null;
                followName = "";
            }
        }

        if (Input.GetMouseButton(0))
        {
            Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            transform.position = Origin - Difference;
        }

        if (Input.GetKeyDown(KeyCode.Space)) // reset camera to original position
        {
            transform.position = ResetCamera;
            Camera.main.orthographicSize = ResetZoom;
        }

        Camera.main.orthographicSize -= 8 * Input.mouseScrollDelta.y;
        if (Camera.main.orthographicSize < 8) Camera.main.orthographicSize = 8;

        if (Input.GetMouseButton(1)) followWho = null;

        // Game Speed Controls
        if (Input.GetKeyDown(KeyCode.J))
        {
            Time.timeScale += -0.25f;
            Time.timeScale = Mathf.Round(100 * Time.timeScale) / 100;
            print("Simulation Speed = " + Time.timeScale);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Time.timeScale += 0.25f;
            Time.timeScale = Mathf.Round(100 * Time.timeScale) / 100;
            print("Simulation Speed = " + Time.timeScale);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Time.timeScale = 1f;
            print("Simulation Speed = " + Time.timeScale);
        }

        uiText.text = "Camera Position: " + transform.position
            + "\nCamera Zoom: " + Mathf.Round(100 * ResetZoom / Camera.main.orthographicSize) / 100
            + "x\nSimulation Speed: " + Time.timeScale + "x";

        if (followName != "") {
            uiText.text += "\nFollowing: " + followName;
        }
    }
}