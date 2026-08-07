
using System.Collections;
using UnityEngine;


public class SombraTranslocator : MonoBehaviour
{
    [Header("Player References")]
    public Transform player; //Who actually gets teleported. Assign the player object in Unity
    public Camera playerPOV; //Camera the beacon is thrown from, established in Unity

    [Header("Beacon Variables")]
    public GameObject beaconPrefab; //Optional visual. A plain sphere is built at runtime if left empty
    public float throwForce = 15f; //How hard the beacon gets tossed forward
    public float beaconGravity = -12f; //Arc on the toss, matching the gravity in the character script
    public float beaconRadius = 0.25f; //Size of the beacon and of the sweep that looks for terrain
    public float throwOffset = 0.75f; //How far in front of the camera it spawns so it doesn't clip her
    public float beaconLifetime = 15f; //How long a beacon sits waiting before it gives up on its own

    [Header("Timing Variables")]
    public float castTime = 0.4f; //0 + 0.4s recovery, blocks acting again straight after the toss
    public float teleportDelay = 0.25f; //The 0.25s the translocation itself takes before she arrives
    public float translocatorCooldown = 6f; //Starts once the beacon is spent, not when it is thrown

    [Header("Teleport Variables")]
    public LayerMask terrainLayers = ~0; //What the beacon treats as terrain. Everything by default
    public float teleportHeightOffset = 1f; //Lifts the arrival point so she doesn't land inside the floor

    private GameObject activeBeacon; //The beacon currently out in the world, if any
    private Vector3 beaconVelocity; //Beacon's own velocity while it is arcing through the air
    private bool beaconActive = false; //True from the moment it leaves her hand until she teleports/it expires
    private bool isTranslocating = false; //True during the 0.25s teleport delay
    private float castTimer = 0f; //Counts down towards 0 before another cast is allowed
    private float cooldownTimer = 0f; //Counts down towards 0 before another beacon can be thrown
    private CharacterController playerController; //Optional, only used so the teleport doesn't fight a controller


    IEnumerator throwBeacon()
    {
        castTimer = castTime;

        activeBeacon = spawnBeacon(playerPOV.transform.position + playerPOV.transform.forward * throwOffset);
        beaconVelocity = playerPOV.transform.forward * throwForce;
        beaconActive = true;

        StartCoroutine(performTranslocation()); //Ttarting the 0.25s delay on translocation

        bool hasLanded = false;
        float elapsed = 0f; //Tracking how long this beacon has been out

        Debug.Log("Beacon thrown");

        //The beacon keeps living through the teleport delay, so a translocation started mid-flight
        //pulls her to wherever the beacon manages to get to
        while(beaconActive && elapsed < beaconLifetime)
        {
            if(!hasLanded)
            {
                beaconVelocity.y += beaconGravity * Time.deltaTime; //Manual gravity keeps the arc predictable
                Vector3 step = beaconVelocity * Time.deltaTime;

                RaycastHit hit;

                //Sweeping ahead of the beacon instead of moving first and checking after, so a hard
                //throw can't tunnel straight through a thin wall
                if(Physics.SphereCast(activeBeacon.transform.position, beaconRadius, step.normalized, out hit, step.magnitude, terrainLayers))
                {
                    activeBeacon.transform.position = hit.point + hit.normal * beaconRadius;
                    hasLanded = true;

                    Debug.Log("Beacon hit terrain, translocating");
                }
                else
                {
                    activeBeacon.transform.position += step;
                }
            }

            elapsed += Time.deltaTime;
            yield return null; // Waiting a frame and looping
        }

        if(beaconActive) //Loop ended on lifetime rather than on a teleport, so the beacon just fizzles
        {
            Debug.Log("Beacon expired");
            clearBeacon();
        }
    }

    IEnumerator performTranslocation()
    {
        isTranslocating = true;

        yield return new WaitForSeconds(teleportDelay); //The 0.25s before she actually arrives

        if(activeBeacon != null)
        {
            //Read at the end of the delay rather than the start, so a beacon still in the air keeps
            //arcing and she lands wherever it got to
            Vector3 destination = activeBeacon.transform.position + Vector3.up * teleportHeightOffset;

            if(playerController != null)
            {
                playerController.enabled = false; //A CharacterController won't accept a direct position change while enabled
                player.position = destination;
                playerController.enabled = true;
            }
            else
            {
                player.position = destination;
            }

            Debug.Log("Translocated to " + destination);
        }

        clearBeacon();
        isTranslocating = false;
    }

    GameObject spawnBeacon(Vector3 spawnPosition)
    {
        if(beaconPrefab != null)
        {
            return Instantiate(beaconPrefab, spawnPosition, Quaternion.identity);
        }

        //Nothing assigned, so a sphere stands in for it. Its collider is stripped because the flight
        //sweep already handles collision and a live collider would snag on the player on the way out
        GameObject fallbackBeacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fallbackBeacon.name = "Translocator Beacon";
        fallbackBeacon.transform.localScale = Vector3.one * (beaconRadius * 2f);
        fallbackBeacon.transform.position = spawnPosition;
        Destroy(fallbackBeacon.GetComponent<Collider>());

        return fallbackBeacon;
    }

    void clearBeacon()
    {
        beaconActive = false;
        cooldownTimer = translocatorCooldown; //Single place the beacon ends, so teleporting and expiring both start it

        if(activeBeacon != null)
        {
            Destroy(activeBeacon);
            activeBeacon = null;
        }
    }

    void TranslocatorInputCheck()
    {
        if(!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if(isTranslocating || castTimer > 0f)
        {
            return; //Already teleporting, or still recovering from the throw
        }

        if(beaconActive)
        {
            StartCoroutine(performTranslocation()); //Works in mid-flight just as well as after it lands
        }
        else if(cooldownTimer <= 0f) //Cooldown only gates throwing a new beacon, never returning to a live one
        {
            StartCoroutine(throwBeacon());
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(player == null || playerPOV == null)
        {
            Debug.LogError("SombraTranslocator needs Player and Player POV assigned in the Inspector");
            enabled = false;
            return;
        }

        playerController = player.GetComponent<CharacterController>(); //Optional, a plain Transform works fine too
    }

    // Update is called once per frame
    void Update()
    {
        if(castTimer > 0f)
        {
            castTimer -= Time.deltaTime;
        }

        if(cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        TranslocatorInputCheck();
    }
}
