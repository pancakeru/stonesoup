using UnityEngine;

public class SeelieHomeScript : MonoBehaviour
{

    public bool seelieIsHome = false; // Set to true when the seelie reaches its home
    public bool playerIsClose = false; // Set to true when the player is close enough to trigger the seelie following

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerIsClose && !seelieIsHome)
        {
            //if the player is within range of this object, set playerIsClose to true
            //this will serve as a way for the seelie to know when to stop following the player and move towards this location instead
            if (Vector3.Distance(transform.position, GameObject.Find("player_tile").transform.position) < 5f)
            {
                playerIsClose = true;
            } else {
                playerIsClose = false;
            }
        }


        if (seelieIsHome)
        {
           // reward the player with something (tbd for now)
        }

    }
}
