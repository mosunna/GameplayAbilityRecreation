using System.Numerics;
using UnityEngine;

public class FirstPersonPOV : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform player;
    public float mouseSensitivty = 2;
    float cameraVerticalRotation = 0; //Camera faces forward at the start of the game
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Collecting mouse input
        float xInput = Input.GetAxis("Mouse X") * mouseSensitivty;
        float yInput = Input.GetAxis("Mouse Y") * mouseSensitivty;

        //Rotating the camera around the x-axis (up and down)
        cameraVerticalRotation -= yInput;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
        transform.localEulerAngles = UnityEngine.Vector3.right * cameraVerticalRotation;

        //Rotating the player and camera round the y-axis (left and right)
        player.Rotate(UnityEngine.Vector3.up * xInput);
    }
}
