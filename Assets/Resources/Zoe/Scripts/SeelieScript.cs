using UnityEngine;

public class SeelieScript : Tile
{
    private Tile playerTile;
    private Tile seelieHomeTile;
    private Vector3 followOffset = new Vector3(0, 0, 1.5f); 
    private float detectionRadius = 10f;
    private float homeDetectionRadius = 3f;
    private float followSpeed = 8f; 
    private bool isFollowing = false;
    private bool isReturningHome = false; 
    // For debug: track previous states to detect transitions
    private bool prevFollowingState = false;
    private bool prevReturningHome = false;
    private SpriteRenderer spriteRenderer;
    private bool spriteFound = false;
    private float homeSnapYOffset = 1.5f; // vertical offset above home when settling
    private bool isLockedAtHome = false; // once true, Seelie stops following forever
    private bool isGlidingToHome = false; // true while gliding to settle position
    private Vector3 homeSettleTarget;
    private float homeSettleSpeed = 5f;

    // seelieHome GameObject removed: we find the nearest Tile named "seelie_home" at init

    public override void init()
    {
        base.init();
        // Cache SpriteRenderer for 2D flip behavior (children allowed)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteFound = true;
        }
    }

    void Update()
    {
        if (isLockedAtHome) return;

        // Robustly assign player and seelieHomeTile if missing (handles spawn order issues)
        if (playerTile == null)
        {
            GameObject playerObj = GameObject.Find("player_tile");
            if (playerObj != null)
                playerTile = playerObj.GetComponent<Tile>();
        }
        if (seelieHomeTile == null)
        {
            Tile[] allTiles = GameObject.FindObjectsOfType<Tile>();
            float bestDist = float.MaxValue;
            foreach (Tile t in allTiles)
            {
                if ((t.gameObject.name == "seelie_home" || t.gameObject.name.Contains("seelie_home")) && t.GetComponent<SeelieHomeScript>() != null)
                {
                    float d = Vector3.Distance(transform.position, t.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        seelieHomeTile = t;
                    }
                }
            }
            if (seelieHomeTile == null)
            {
                Debug.LogWarning("[SeelieScript] No SeelieHomeScript found on any seelie_home tile!");
            }
        }

        // Glide to home settle position if triggered
        if (isGlidingToHome)
        {
            float dist = Vector3.Distance(transform.position, homeSettleTarget);
            Debug.Log($"[SeelieScript] Gliding home. Distance to settle target: {dist}");
            if (dist > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, homeSettleTarget, homeSettleSpeed * Time.deltaTime);
            }
            else
            {
                Debug.Log("[SeelieScript] Arrived at home settle target!");
                transform.position = homeSettleTarget;
                isGlidingToHome = false;
                isLockedAtHome = true;
                if (seelieHomeTile == null)
                {
                    Debug.LogError("[SeelieScript] seelieHomeTile is null on arrival!");
                }
                else
                {
                    var home = seelieHomeTile != null ? seelieHomeTile.GetComponent<SeelieHomeScript>() : null;
                    if (home != null)
                    {
                        home.OnSeelieArrived();
                        home.StartHealingMode();
                    }
                }
            }
            return;
        }
        // Compute distance to player only if we have a reference
        float distanceToPlayer = float.MaxValue;
        if (playerTile != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTile.transform.position);
        }

        // Check if home is nearby (if it exists)
        float distanceToHome = seelieHomeTile != null ? Vector3.Distance(transform.position, seelieHomeTile.transform.position) : float.MaxValue;
        bool homeIsNearby = distanceToHome < homeDetectionRadius && seelieHomeTile != null;

        // Detect player: use sight-based detection combined with distance
        bool playerIsVisible = playerTile != null && canSeeTile(playerTile);
        if (playerTile != null && playerIsVisible && distanceToPlayer < detectionRadius)
        {
            isFollowing = true;
        }
        else if (playerTile != null && distanceToPlayer > detectionRadius + 2f)
        {
            isFollowing = false;
        }

        // If home is nearby, prioritize returning home
        if (homeIsNearby && !isReturningHome)
        {
            isFollowing = false;
            isReturningHome = true;
        }

        // Follow the player if in range and home hasn't triggered
        if (isFollowing && !isReturningHome)
        {
            if (playerTile == null)
            {
                // Can't follow without a player reference
                isFollowing = false;
            }
            else
            {
            Vector3 targetPosition = playerTile.transform.position - playerTile.transform.forward * followOffset.z;
            targetPosition.y = playerTile.transform.position.y + followOffset.y;

            // Use physics-based velocity movement for smoother follow
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (body != null)
            {
                body.linearVelocity = direction * followSpeed;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }

            // 2D-friendly facing: flip sprite left/right instead of LookAt
            if (spriteFound && playerTile != null)
            {
                float dx = playerTile.transform.position.x - transform.position.x;
                if (Mathf.Abs(dx) > 0.01f)
                {
                    // Assume default sprite faces right; flip when player is to the left
                    spriteRenderer.flipX = dx > 0f;
                }
            }
            else
            {
                // Ensure we don't rotate the sprite into a thin line — preserve local rotation on Y/Z
                Vector3 e = transform.eulerAngles;
                transform.eulerAngles = new Vector3(e.x, 0f, e.z);
            }
            }
        }
        else if (!isFollowing && body != null)
        {
            // Stop velocity when not following
            body.linearVelocity = Vector2.zero;
        }

        // Return to home: if close enough to home settle position, start gliding to settle
        if (isReturningHome && seelieHomeTile != null && !isGlidingToHome && !isLockedAtHome)
        {
            Vector3 homePos = seelieHomeTile.transform.position;
            Vector3 settleTarget = new Vector3(homePos.x, homePos.y + homeSnapYOffset, transform.position.z);
            float distToSettle = Vector3.Distance(transform.position, settleTarget);
            if (distToSettle < 10f) // threshold for "close enough" to start glide (increased)
            {
                StartGlideToHomeAndLock();
            }
            else
            {
                // Move toward home as before
                Vector3 direction = (homePos - transform.position).normalized;
                if (body != null)
                {
                    body.linearVelocity = direction * followSpeed;
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, homePos, followSpeed * Time.deltaTime);
                }
            }
        }

        // State transitions handled silently

        prevFollowingState = isFollowing;
        prevReturningHome = isReturningHome;
    }

    // Start gliding the Seelie to its home's X and above by homeSnapYOffset, then lock and notify home on arrival
    private void StartGlideToHomeAndLock()
    {
        if (seelieHomeTile == null)
        {
            Debug.LogError("[SeelieScript] StartGlideToHomeAndLock called but seelieHomeTile is null!");
            return;
        }
        Vector3 pos = transform.position;
        Vector3 homePos = seelieHomeTile.transform.position;
        homeSettleTarget = new Vector3(homePos.x, homePos.y + homeSnapYOffset, pos.z);
        Debug.Log($"[SeelieScript] Starting glide to home settle target: {homeSettleTarget}");
        isGlidingToHome = true;
        isFollowing = false;
        isReturningHome = false;
    }

    // Start following when the player touches the Seelie (supports trigger and collision, 3D and 2D)
    void OnTriggerEnter(Collider other)
    {
        
        // If we don't yet have a playerTile, detect and assign it by name or Tile component
        Tile otherTile = other.GetComponent<Tile>();
        if (playerTile == null)
        {
            if (otherTile != null && (other.gameObject.name == "player_tile" || other.gameObject.name.Contains("player_tile")))
            {
                playerTile = otherTile;
            }
        }

        if (playerTile != null && (other.gameObject == playerTile.gameObject || otherTile == playerTile))
        {
            isFollowing = true;
            isReturningHome = false;
        }

        // No longer needed: handled by proximity in Update
    }

    void OnCollisionEnter(Collision collision)
    {
        Tile otherTile = collision.gameObject.GetComponent<Tile>();
        if (playerTile == null)
        {
            if (otherTile != null && (collision.gameObject.name == "player_tile" || collision.gameObject.name.Contains("player_tile")))
            {
                playerTile = otherTile;
            }
        }

        if (playerTile != null && (collision.gameObject == playerTile.gameObject || otherTile == playerTile))
        {
            isFollowing = true;
            isReturningHome = false;
        }

        if (!isLockedAtHome && !isGlidingToHome && seelieHomeTile != null && otherTile == seelieHomeTile)
        {
            StartGlideToHomeAndLock();
        }
    }

    // 2D physics variants in case project uses 2D colliders
    void OnTriggerEnter2D(Collider2D other)
    {
        Tile otherTile = other.GetComponent<Tile>();
        if (playerTile == null)
        {
            if (otherTile != null && (other.gameObject.name == "player_tile" || other.gameObject.name.Contains("player_tile")))
            {
                playerTile = otherTile;
            }
        }

        if (playerTile != null && (other.gameObject == playerTile.gameObject || otherTile == playerTile))
        {
            isFollowing = true;
            isReturningHome = false;
        }

        if (!isLockedAtHome && !isGlidingToHome && seelieHomeTile != null && otherTile == seelieHomeTile)
        {
            StartGlideToHomeAndLock();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Tile otherTile = collision.gameObject.GetComponent<Tile>();
        if (playerTile == null)
        {
            if (otherTile != null && (collision.gameObject.name == "player_tile" || collision.gameObject.name.Contains("player_tile")))
            {
                playerTile = otherTile;
            }
        }

        if (playerTile != null && (collision.gameObject == playerTile.gameObject || otherTile == playerTile))
        {
            isFollowing = true;
            isReturningHome = false;
        }

        if (!isLockedAtHome && !isGlidingToHome && seelieHomeTile != null && otherTile == seelieHomeTile)
        {
            StartGlideToHomeAndLock();
        }
    }
}
