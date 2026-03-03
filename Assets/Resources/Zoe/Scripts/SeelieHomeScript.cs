using System.Runtime.InteropServices;
using UnityEngine;

public class SeelieHomeScript : Tile
{
    public bool seelieIsHome = false; // Set to true when the seelie reaches its home
    public bool playerIsClose = false; // Set to true when the player is close enough to trigger the seelie following
    private Tile playerTile;

    // Healing mode state
    private bool healingMode = false;
    private float healingRadius = 3.5f;
    private float healCooldown = 0.5f; // seconds between heals
    private float healTimer = 0f;
    private int maxPlayerHealth = 3;
    private Color healingColor = Color.green;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    public override void init()
    {
        base.init();
        // Get reference to the player tile
        GameObject playerObj = GameObject.Find("player_tile");
        if (playerObj != null)
        {
            playerTile = playerObj.GetComponent<Tile>();
        }
        // Cache sprite renderer and original color
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update() {
        // Only set color and heal if healingMode is active
        if (healingMode)
        {
            Debug.Log($"[SeelieHomeScript] Healing mode active");
            if (spriteRenderer != null)
            {
                if (spriteRenderer.color != Color.green)
                {
                    Debug.Log("[SeelieHomeScript] Changing sprite color to green");
                }
                spriteRenderer.color = Color.green;
            }

            if (Player.instance != null)
            {
                float dist = Vector3.Distance(transform.position, Player.instance.transform.position);
                Debug.Log($"[SeelieHomeScript] Player distance: {dist}");
                if (dist < healingRadius)
                {
                    healTimer -= Time.deltaTime;
                    Debug.Log($"[SeelieHomeScript] Player in healing radius. healTimer: {healTimer}");
                    if (healTimer <= 0f)
                    {
                        if (Player.instance.health < maxPlayerHealth)
                        {
                            Debug.Log($"[SeelieHomeScript] Healing player. Old health: {Player.instance.health}");
                            Player.instance.health += 1;
                            if (Player.instance.health > maxPlayerHealth)
                                Player.instance.health = maxPlayerHealth;
                            Debug.Log($"[SeelieHomeScript] New player health: {Player.instance.health}");
                        }
                        healTimer = healCooldown;
                    }
                }
                else
                {
                    healTimer = 0f;
                }
            }
        }

        // Remove auto-StartHealingMode from Update; healingMode should only be set by OnSeelieArrived
    }

    // Call this to enable healing mode (turns green, enables healing)
    public void StartHealingMode()
    {
        Debug.Log("[SeelieHomeScript] StartHealingMode called");
        healingMode = true;
        if (spriteRenderer != null)
        {
            Debug.Log("[SeelieHomeScript] Setting sprite color to healingColor");
            spriteRenderer.color = healingColor;
        }
    }

    // Called by the Seelie when it reaches this home
    public void OnSeelieArrived()
    {
        Debug.Log("[SeelieHomeScript] OnSeelieArrived called");
        seelieIsHome = true;
        StartHealingMode();
        // Additional logic (rewards, animations) can be placed here.
    }
}
