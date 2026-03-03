using UnityEngine;

public class balloon_script : Tile
{
    private Tile playerTile;

    public override void init()
    {
        base.init();
        GameObject playerObj = GameObject.Find("player_tile");
        if (playerObj != null)
        {
            playerTile = playerObj.GetComponent<Tile>();
        }
    }

    

}
