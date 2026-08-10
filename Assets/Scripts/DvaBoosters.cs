
using System.Collections;
using UnityEngine;


public class DvaBoosters : MonoBehaviour
{
    [Header("Player References")]
    public Transform player; //Who actually gets flown around. Assign the player object in Unity
    public Camera playerPOV; //Camera the flight direction is read from, established in Unity
    public MonoBehaviour movementScript; //Optional. The character script, switched off mid-flight so the two don't fight

    [Header("Boost Variables")]
    public float baseSpeed = 5f; //Should match the speed on the character script, the buff maths off this
    public float speedMultiplier = 2.18f; //The +118% move speed buff
    public float maxBoostRange = 24f; //Hard cap on distance, the flight ends early once she covers it
    public float steerRate = 6f; //How quickly the flight turns to follow the camera. Higher is twitchier

    [Header("Timing Variables")]
    public float maxBoostDuration = 2f; //Full length of the flight if she never cancels it
    public float minBoostDuration = 0.4f; //Cancelling is locked out until this much has passed
    public float boostersCooldown = 5f; //Starts when the flight ends, not when it is triggered

    private bool isBoosting = false; //True for the whole flight
    private float cooldownTimer = 0f; //Counts down towards 0 before boosters can be used again
    private CharacterController playerController; //Optional, only used so the flight moves her cleanly

    public bool IsBoosting //So other scripts can check without touching the flight
    { 
        get { return isBoosting; } 
    }
    

    IEnumerator performBoosters()
    {
        isBoosting = true;

        if(movementScript != null)
        {
            movementScript.enabled = false; //Stops the character script's gravity and WASD fighting the flight
        }

        Vector3 boostDirection = playerPOV.transform.forward; //Full 3D forward, so looking up flies her up
        float boostSpeed = baseSpeed * speedMultiplier;
        float elapsed = 0f; //Tracking how long the flight has been going
        float distanceTravelled = 0f; //Tracking distance against the 24m cap

        Debug.Log("Boosters started at " + boostSpeed + " m/s");

        while(elapsed < maxBoostDuration && distanceTravelled < maxBoostRange)
        {
            //Aim is re-read every frame and eased towards, so she banks into a turn instead of
            //snapping direction the instant the mouse moves. That easing is the manoeuvring
            boostDirection = Vector3.Slerp(boostDirection, playerPOV.transform.forward, steerRate * Time.deltaTime).normalized;

            Vector3 previousPosition = player.position;

            if(playerController != null)
            {
                playerController.Move(boostDirection * boostSpeed * Time.deltaTime); //No gravity term, the flight ignores it
            }
            else
            {
                player.position += boostDirection * boostSpeed * Time.deltaTime;
            }

            //Measured from where she actually ended up rather than from intent, so a wall she is
            //grinding along stops eating into her range
            distanceTravelled += Vector3.Distance(previousPosition, player.position);

            elapsed += Time.deltaTime;

            //Cancelling is locked out for the first 0.4s, after that shift cuts the flight short
            if(elapsed >= minBoostDuration && Input.GetKeyDown(KeyCode.LeftShift))
            {
                Debug.Log("Boosters cancelled early at " + elapsed + "s");
                break;
            }

            yield return null; // Waiting a frame and looping
        }

        if(movementScript != null)
        {
            movementScript.enabled = true;
        }

        isBoosting = false;
        cooldownTimer = boostersCooldown; //Timed from the end, so a full 2s flight still gets the whole gap after it

        Debug.Log("Boosters ended after " + elapsed + "s and " + distanceTravelled + "m");
    }

    void BoostersInputCheck()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) && !isBoosting && cooldownTimer <= 0f)
        {
            StartCoroutine(performBoosters());
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(player == null || playerPOV == null)
        {
            Debug.LogError("DvaBoosters needs Player and Player POV assigned in the Inspector");
            enabled = false;
            return;
        }

        playerController = player.GetComponent<CharacterController>(); //Optional, a plain Transform works fine too
    }

    // Update is called once per frame
    void Update()
    {
        if(cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        BoostersInputCheck();
    }
}
