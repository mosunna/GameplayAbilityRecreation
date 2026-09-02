using UnityEngine;

public class TopDownCaptureCamera : MonoBehaviour
{
    [Header("Gameplay Camera Swap")]
    public Camera gameplayCamera; //Your existing first person camera. Leave empty if you don't want the swap behaviour
    public KeyCode toggleKey = KeyCode.C; //Key that swaps between gameplayCamera and this capture camera

    private bool captureCameraActive = true; //Tracks which of the two cameras is currently live

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(gameplayCamera != null)
        {
            captureCameraActive = GetComponent<Camera>().enabled; //Match whatever state was left set in the Inspector
            applyActiveState();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameplayCamera != null && Input.GetKeyDown(toggleKey))
        {
            captureCameraActive = !captureCameraActive;
            applyActiveState();
        }
    }

    void applyActiveState()
    {
        Camera myCamera = GetComponent<Camera>();

        myCamera.enabled = captureCameraActive;

        if(gameplayCamera != null)
        {
            gameplayCamera.enabled = !captureCameraActive;
        }
    }
}
