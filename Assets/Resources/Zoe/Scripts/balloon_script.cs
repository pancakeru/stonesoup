using UnityEngine;

public class balloon_script : Tile
{
    private Tile playerTile;
    private bool isHeld = false;
    private Player playerRef;
     // Array of candy prefabs to spawn
    public GameObject[] candyPrefabs;

    public override void init()
    {
        base.init();
        GameObject playerObj = GameObject.Find("player_tile");
        if (playerObj != null)
        {
            playerTile = playerObj.GetComponent<Tile>();
        }
    }

    public override void pickUp(Tile tilePickingUsUp) {
        // Spawn a random candy prefab near the balloon's position, retrying if it overlaps a Wall
        base.pickUp(tilePickingUsUp);
        int idx = Random.Range(0, candyPrefabs.Length);
        GameObject prefab = candyPrefabs[idx];
        if (prefab != null)
        {
            Debug.Log("Spawning candy prefab: " + prefab.name);
            int maxTries = 10;
            bool spawned = false;
            int wallLayer = LayerMask.NameToLayer("Wall");
            int wallMask = wallLayer >= 0 ? (1 << wallLayer) : ~0;
            float checkRadius = 0.5f;
            for (int attempt = 0; attempt < maxTries && !spawned; ++attempt)
            {
                Vector3 spawnOffset = new Vector3(Random.Range(-3f, 3f), Random.Range(-2f, 2f), 0f);
                Vector3 spawnPos = transform.position + spawnOffset;
                Collider2D hit = Physics2D.OverlapCircle(spawnPos, checkRadius, wallMask);
                if (hit == null)
                {
                    Instantiate(prefab, spawnPos, Quaternion.identity);
                    spawned = true;
                }
            }
            if (!spawned)
            {
                Debug.LogWarning("Could not find a valid spawn position for candy after " + maxTries + " attempts.");
            }
        }
            
     
    }

    public override void dropped(Tile tileDroppingUs) {
        base.dropped(tileDroppingUs);
	}


    void Update() {
          
    }
}
