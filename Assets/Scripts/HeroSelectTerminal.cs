
using UnityEngine;


[System.Serializable]
public class HeroLoadout
{
    public string heroName = "New Hero"; //What the menu button says
    public MonoBehaviour[] abilityScripts; //Every ability script this hero owns. Plural so a hero can have more than one
    public string controlsHint = ""; //Shown under the buttons, e.g. "Shift/RMB Blink - E Recall"
}


public class HeroSelectTerminal : MonoBehaviour
{
    [Header("Player References")]
    public Camera playerPOV; //The look ray is cast from here, established in Unity
    public MonoBehaviour cameraLookScript; //FirstPersonPOV. Switched off while the menu is open
    public MonoBehaviour movementScript; //CharacterMovement. Never part of a loadout, always forced back on

    [Header("Interaction Variables")]
    public float interactRange = 4f; //How close she has to be for the prompt to show
    public KeyCode interactKey = KeyCode.F; //Opens and closes the menu
    public LayerMask interactLayers = ~0; //What the look ray is allowed to hit. Everything by default

    [Header("Menu Variables")]
    public float menuWidth = 280f;
    public float buttonHeight = 28f;

    [Header("Hero Roster")]
    public int startingHero = 0; //Which loadout is equipped when the scene starts
    public HeroLoadout[] heroes; //Add a hero by adding an entry here. No code changes needed

    private bool isLookedAt = false; //True while the terminal is under the crosshair and in range
    private bool menuOpen = false;
    private int currentHero = -1; //Which loadout is live right now
    private int pendingHero = -1; //Button presses are queued here and handled in Update, not mid-layout
    private GUIStyle centeredLabel; //Built on the first OnGUI pass, GUI.skin isn't valid before then


    bool checkLookedAt()
    {
        RaycastHit hit;

        //Cast from the camera rather than the player so it follows where she is actually aiming, and
        //so a wall standing between her and the terminal blocks the prompt
        if(Physics.Raycast(playerPOV.transform.position, playerPOV.transform.forward, out hit, interactRange, interactLayers))
        {
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }

    void setLoadoutEnabled(int heroIndex, bool isEnabled)
    {
        if(heroes == null || heroIndex < 0 || heroIndex >= heroes.Length)
        {
            return;
        }

        if(heroes[heroIndex] == null || heroes[heroIndex].abilityScripts == null)
        {
            return; //Half filled Inspector slot, nothing to switch
        }

        for(int i = 0; i < heroes[heroIndex].abilityScripts.Length; i++)
        {
            if(heroes[heroIndex].abilityScripts[i] != null)
            {
                heroes[heroIndex].abilityScripts[i].enabled = isEnabled;
            }
        }
    }

    void disableAllLoadouts()
    {
        if(heroes == null)
        {
            return;
        }

        //Looping the whole roster means it doesn't matter how many heroes exist or how many
        //scripts each of them owns
        for(int i = 0; i < heroes.Length; i++)
        {
            setLoadoutEnabled(i, false);
        }
    }

    void restoreMovement()
    {
        //An ability cut off mid-coroutine can leave the character script switched off (D.Va's flight
        //does exactly that), which would strand the player with no way to move again
        if(movementScript != null)
        {
            movementScript.enabled = true;
        }
    }

    void equipHero(int heroIndex)
    {
        if(heroes == null || heroIndex < 0 || heroIndex >= heroes.Length || heroes[heroIndex] == null)
        {
            return;
        }

        disableAllLoadouts();
        setLoadoutEnabled(heroIndex, true);
        restoreMovement();

        currentHero = heroIndex;
        Debug.Log("Equipped " + heroes[heroIndex].heroName);

        closeMenu();
    }

    void openMenu()
    {
        menuOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if(cameraLookScript != null)
        {
            cameraLookScript.enabled = false; //Otherwise mouse movement spins the camera while she clicks buttons
        }

        //Abilities go quiet while the menu is up, so a right click on a button can't also fire a blink
        disableAllLoadouts();
        restoreMovement();
    }

    void closeMenu()
    {
        menuOpen = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if(cameraLookScript != null)
        {
            cameraLookScript.enabled = true;
        }

        setLoadoutEnabled(currentHero, true); //Whatever is equipped wakes back up
    }

    void ensureStyles()
    {
        if(centeredLabel == null)
        {
            centeredLabel = new GUIStyle(GUI.skin.label);
            centeredLabel.alignment = TextAnchor.MiddleCenter;
            centeredLabel.wordWrap = true;
        }
    }

    void drawInteractPrompt()
    {
        //Sat just under the middle of the screen so it reads as a crosshair prompt
        Rect promptArea = new Rect(Screen.width * 0.5f - 120f, Screen.height * 0.5f + 20f, 240f, 24f);
        GUI.Label(promptArea, "[" + interactKey + "] Hero Select", centeredLabel);
    }

    void drawHeroMenu()
    {
        int heroCount = 0;
        if(heroes != null)
        {
            heroCount = heroes.Length;
        }

        //Height is worked out from the roster, so a new hero grows the menu on its own
        float menuHeight = 90f + heroCount * (buttonHeight + 4f);
        Rect menuArea = new Rect(Screen.width * 0.5f - menuWidth * 0.5f, Screen.height * 0.5f - menuHeight * 0.5f, menuWidth, menuHeight);

        GUI.Box(menuArea, "SELECT HERO");

        GUILayout.BeginArea(new Rect(menuArea.x + 12f, menuArea.y + 28f, menuWidth - 24f, menuHeight - 40f));

        for(int i = 0; i < heroCount; i++)
        {
            if(heroes[i] == null)
            {
                continue; //Empty Inspector slot, skip it rather than drawing a blank button
            }

            string buttonLabel = heroes[i].heroName;

            if(i == currentHero)
            {
                buttonLabel = "> " + buttonLabel + " (equipped)";
            }

            if(GUILayout.Button(buttonLabel, GUILayout.Height(buttonHeight)))
            {
                pendingHero = i; //Handled in Update so the layout isn't changed halfway through drawing it
            }
        }

        GUILayout.Space(6f);

        if(currentHero >= 0 && currentHero < heroCount && heroes[currentHero] != null)
        {
            GUILayout.Label(heroes[currentHero].controlsHint, centeredLabel);
        }

        GUILayout.Label("Esc to close", centeredLabel);

        GUILayout.EndArea();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerPOV == null)
        {
            Debug.LogError("HeroSelectTerminal needs Player POV assigned in the Inspector");
            enabled = false;
            return;
        }

        equipHero(startingHero); //Everything except the starting loadout begins switched off
    }

    // Update is called once per frame
    void Update()
    {
        if(pendingHero >= 0)
        {
            equipHero(pendingHero);
            pendingHero = -1;
        }

        isLookedAt = checkLookedAt();

        if(!menuOpen && isLookedAt && Input.GetKeyDown(interactKey))
        {
            openMenu();
        }
        else if(menuOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(interactKey)))
        {
            closeMenu();
        }
    }

    void OnGUI()
    {
        ensureStyles();

        if(menuOpen)
        {
            drawHeroMenu();
        }
        else if(isLookedAt)
        {
            drawInteractPrompt();
        }
    }
}
