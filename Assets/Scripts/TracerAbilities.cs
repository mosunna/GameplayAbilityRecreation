
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TracerAbilities : MonoBehaviour
{
    [Header("Player References")]
    public Transform player; //Who the abilities move around. Assign the player object in Unity
    public Camera playerPOV; //Used for the blink direction when she is standing still
    public MonoBehaviour movementScript; //Optional. Switched off during a recall so it can't move a disabled controller

    [Header("Blink Variables")]
    public float blinkDistance = 7f;
    public float blinkDuration = 0.1f; //Long enough that the dash sweeps visibly instead of teleporting
    public int maxBlinkCharges = 3; //How many blinks can be banked at once
    public float blinkChargeCooldown = 3f; //How long one spent charge takes to come back
    public float blinkRecovery = 0.1f; //Forced gap after a blink ends, stops all 3 charges firing at once

    [Header("Recall Variables")]
    public float recallDuration = 3f; //How far back she is rewound (3 seconds)
    public float recallPlaybackDuration = 1.25f; //How long the rewind itself takes to play out on screen
    public float recallCooldown = 13f; //Starts the moment recall is used, not when the rewind finishes
    public float timeInterval = 0.01f; //How often player position gets saved

    private CharacterController controller; //Optional, only used so the abilities move her cleanly
    private Vector3 movementDirection; //Rebuilt from input each frame, same as the character script does it

    private int blinkCharges; //Charges currently available
    private float chargeRechargeTimer = 0f; //Counts up towards blinkChargeCooldown
    private float blinkRecoveryTimer = 0f; //Counts down towards 0 before another blink is allowed
    private bool isBlinking = false; //True while a blink coroutine is mid-flight

    private Queue<Vector3> positionHistory; //Queue that saves a history of player position
    private float timer = 0f; //Timer keeping track of time as it passes
    private int historyCapacity; //How many entries 3 seconds of history actually works out to
    private float recallCooldownTimer = 0f; //Counts down towards 0 before recall is allowed again
    private bool isRecalling = false; //Blocks blinking and saving while the rewind is playing

    // -- FOR UI --
    public bool IsRecalling //So other scripts can check without touching the rewind
    { 
        get {return isRecalling; }
    }
    public bool IsBlinking
    {
        get {return isBlinking;}
    }
    public int BlinkCharges
    {
        get{return blinkCharges;}
    }
    // -- FOR UI --


    IEnumerator performBlink(Vector3 blinkDirection)
    {
        isBlinking = true;
        float elapsed = 0f; //Tracking how much time has passed each frame

        while(elapsed < blinkDuration && !isRecalling) //A recall starting mid-blink cancels the blink
        {
            float step = (blinkDistance / blinkDuration) * Time.deltaTime; //How far she moves on a specific frame

            if(controller != null)
            {
                controller.Move(blinkDirection * step);
            }
            else
            {
                player.position += blinkDirection * step;
            }

            elapsed += Time.deltaTime;
            yield return null; // Waiting a frame and looping
        }

        isBlinking = false;
        blinkRecoveryTimer = blinkRecovery; //Recovery is timed from the end of the dash, not the start
    }

    void BlinkInputCheck()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1))
        {
            if(blinkCharges <= 0 || isBlinking || blinkRecoveryTimer > 0f)
            {
                return; //Out of charges, already dashing, or still recovering from the last one
            }

            blinkCharges--;
            Debug.Log("Blink used (" + blinkCharges + "/" + maxBlinkCharges + " charges left)");

            Vector3 blinkDirection;

            if(movementDirection.sqrMagnitude > 0.01f)
            {
                blinkDirection = movementDirection.normalized; //Keeps diagonal blinks the same length as straight ones
            }
            else
            {
                blinkDirection = playerPOV.transform.forward; //Standing still, so she blinks where she is looking
            }

            StartCoroutine(performBlink(blinkDirection));
        }
    }

    IEnumerator performRecall()
    {
        isRecalling = true;
        recallCooldownTimer = recallCooldown; //Cooldown runs from the moment it fires, so the rewind counts towards it

        //Oldest saved position sits at index 0, newest at the end. Where she is standing right now
        //isn't in the queue yet, so it gets tacked on as the final point of the path
        Vector3[] savedPositions = positionHistory.ToArray();
        Vector3[] rewindPath = new Vector3[savedPositions.Length + 1];
        savedPositions.CopyTo(rewindPath, 0);
        rewindPath[rewindPath.Length - 1] = player.position;

        //The character script has to go quiet first, otherwise its own Update keeps calling Move on a
        //controller this coroutine is about to switch off
        if(movementScript != null)
        {
            movementScript.enabled = false;
        }

        //The rewind retraces ground she already stood on, so collision is turned off to stop the
        //controller from catching on geometry (and to allow the position to be set directly)
        if(controller != null)
        {
            controller.enabled = false;
        }

        float elapsed = 0f;

        while(elapsed < recallPlaybackDuration)
        {
            float progress = elapsed / recallPlaybackDuration; //0 = current position, 1 = 3 seconds ago

            //Walking the path backwards. Lerping between neighbouring entries keeps the rewind smooth
            //instead of snapping between the 300 saved points
            float samplePoint = (1f - progress) * (rewindPath.Length - 1);
            int lowerIndex = Mathf.FloorToInt(samplePoint);
            int upperIndex = Mathf.Min(lowerIndex + 1, rewindPath.Length - 1);

            player.position = Vector3.Lerp(rewindPath[lowerIndex], rewindPath[upperIndex], samplePoint - lowerIndex);

            elapsed += Time.deltaTime;
            yield return null; // Waiting a frame and looping
        }

        player.position = rewindPath[0]; //Landing exactly on the oldest position rather than near it

        if(controller != null)
        {
            controller.enabled = true;
        }

        if(movementScript != null)
        {
            movementScript.enabled = true;
        }

        positionHistory.Clear(); //History belongs to the old timeline, so it starts fresh from here
        timer = 0f;
        isRecalling = false;

        Debug.Log("Recall finished at " + rewindPath[0]);
    }

    void RecallInputCheck()
    {
        if(Input.GetKeyDown(KeyCode.E) && positionHistory.Count > 0 && recallCooldownTimer <= 0f)
        {
            StartCoroutine(performRecall());
        }
    }

    void tickAbilityTimers()
    {
        if(recallCooldownTimer > 0f)
        {
            recallCooldownTimer -= Time.deltaTime;
        }

        if(blinkRecoveryTimer > 0f)
        {
            blinkRecoveryTimer -= Time.deltaTime;
        }

        if(blinkCharges >= maxBlinkCharges)
        {
            chargeRechargeTimer = 0f; //Nothing to refill, so the recharge clock sits idle
            return;
        }

        //Charges come back one at a time, so spending all 3 refills them at 3s, 6s and 9s
        chargeRechargeTimer += Time.deltaTime;
        if(chargeRechargeTimer >= blinkChargeCooldown)
        {
            blinkCharges++;
            chargeRechargeTimer -= blinkChargeCooldown; //Keeping the leftover stops the refill rate drifting
            Debug.Log("Blink charge restored (" + blinkCharges + "/" + maxBlinkCharges + ")");
        }
    }

    void savePosition()
    {
        timer += Time.deltaTime;
        if(timer < timeInterval)
        {
            return;
        }
        else
        {
            positionHistory.Enqueue(player.position);
            timer = 0;
        }

        if(positionHistory.Count > historyCapacity)
        {
            positionHistory.Dequeue();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(player == null || playerPOV == null)
        {
            Debug.LogError("TracerAbilities needs Player and Player POV assigned in the Inspector");
            enabled = false;
            return;
        }

        controller = player.GetComponent<CharacterController>(); //Optional, a plain Transform works fine too
        historyCapacity = Mathf.CeilToInt(recallDuration / timeInterval); //3s / 0.01s = 300 saved positions
        positionHistory = new Queue<Vector3>();
        blinkCharges = maxBlinkCharges; //Starting the game with a full set of blinks
    }

    // Update is called once per frame
    void Update()
    {
        tickAbilityTimers(); //Runs before the recall guard so cooldowns keep ticking during the rewind

        //The rewind drives the transform itself, so blinking and saving pause until it ends
        if(isRecalling)
        {
            return;
        }

        //Rebuilt here rather than read off the character script, since that keeps it private
        movementDirection = player.right * Input.GetAxisRaw("Horizontal") + player.forward * Input.GetAxisRaw("Vertical");

        BlinkInputCheck();

        RecallInputCheck();

        savePosition();
    }
}
