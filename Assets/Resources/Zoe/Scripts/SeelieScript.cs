using UnityEngine;

public class SeelieScript : MonoBehaviour
{
    private Transform player;
    private Vector3 followOffset = new Vector3(0, 0, 1.5f); // Distance behind player (adjust as needed)
    private float detectionRadius = 10f; // How close player needs to be to trigger following
    private float followSpeed = 5f; // How fast it follows (adjust for smoother/snappier movement)
    private bool isFollowing = false;
    private bool isReturningHome = false; // Track if the seelie is currently returning home

    public GameObject seelieHome; //final destination for the seelie, set in inspector

    void Start()
    {
        // Find the player_tile prefab on the Player layer
        GameObject playerObj = GameObject.Find("player_tile");
        if (playerObj != null && playerObj.layer == LayerMask.NameToLayer("Player"))
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within detection radius
        if (distanceToPlayer < detectionRadius)
        {
            isFollowing = true;
        }
        else if (distanceToPlayer > detectionRadius + 2f)
        {
            isFollowing = false;
        }

        // Follow the player if close enough
        if (isFollowing)
        {
            Vector3 targetPosition = player.position - player.forward * followOffset.z;
            targetPosition.y = player.position.y + followOffset.y;

            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            transform.LookAt(player.position);
        }


        if (seelieHome != null)
        {
           if(seelieHome.GetComponent<SeelieHomeScript>().playerIsClose && !isReturningHome)
           {
                isFollowing = false; // Stop following the player
                isReturningHome = true; // Start returning home
           }
        }

        if (isReturningHome)
        {
            Vector3 targetPosition = seelieHome.transform.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            // Check if we've reached home
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isReturningHome = false; // Stop moving once we reach home
                seelieHome.GetComponent<SeelieHomeScript>().seelieIsHome = true; // Mark the seelie as home
            }
        }
    }
}
