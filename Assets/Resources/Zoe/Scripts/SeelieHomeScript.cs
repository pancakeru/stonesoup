using System.Runtime.InteropServices;
using UnityEngine;

public class SeelieHomeScript : Tile
{
    public bool seelieIsHome = false; // Set to true when the seelie reaches its home
    public bool playerIsClose = false; // Set to true when the player is close enough to trigger the seelie following
    private Tile playerTile;

    public Sprite glowRadiusSprite; // Assign a sprite with a circular gradient in the inspector

    // Healing mode state
    private bool healingMode = false;
    private float healingRadius = 3.5f;
    private float healCooldown = 0.5f; // seconds between heals
    private float healTimer = 0f;
    private int maxPlayerHealth = 3;
    private Color healingColor = Color.green;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    // Glow effect
    private GameObject glowObj;
    private SpriteRenderer glowRenderer;
    private float glowFlickerTimer = 0f;
    private float glowFlickerSpeed = 4f; // Flicker speed

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
        // Only set color, heal, and glow if healingMode is active
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

            // Flicker the glow opacity
            if (glowRenderer != null)
            {
                glowFlickerTimer += Time.deltaTime * glowFlickerSpeed;
                float alpha = Mathf.Lerp(0.3f, 0.6f, (Mathf.Sin(glowFlickerTimer) + 1f) / 2f);
                Color c = glowRenderer.color;
                c.a = alpha;
                glowRenderer.color = c;
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

        if (glowObj == null && glowRadiusSprite != null)
        {
            glowObj = new GameObject("GlowRadius");
            glowObj.transform.SetParent(transform, false);
            glowObj.transform.localPosition = Vector3.zero;
            glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = glowRadiusSprite;
            glowRenderer.color = new Color(0f, 1f, 0f, 0.4f); 
            glowRenderer.sortingOrder = (spriteRenderer != null ? spriteRenderer.sortingOrder : 0) - 1;
            glowObj.transform.localScale = new Vector3(1.5f, 2.5f, 1f); // Uniform scale for perfect circle
        }
    }

    // Called by the Seelie when it reaches this home
    public void OnSeelieArrived()
    {
       // Debug.Log("[SeelieHomeScript] OnSeelieArrived called");
        seelieIsHome = true;
        StartHealingMode();
        // Additional logic (rewards, animations) can be placed here.
    }
}
