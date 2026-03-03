using System.Runtime.CompilerServices;

using UnityEngine;

public class candy_script : Tile
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

    void ResetPlayer()
    {
        Player.instance.moveSpeed /= 1.5f;
        Player.instance.moveAcceleration /= 1.5f;
        Player.instance.GetComponent<Rigidbody2D>().sharedMaterial = new PhysicsMaterial2D() {
            bounciness = 0f,
            friction = 0.4f,
            bounceCombine = UnityEngine.PhysicsMaterialCombine2D.Average,
            frictionCombine = UnityEngine.PhysicsMaterialCombine2D.Average
        };
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ApplyEffects();
    }

    void ApplyEffects()
    {
        this.gameObject.GetComponent<Collider2D>().enabled = false;
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        Player.instance.moveSpeed *= 1.5f;
        Player.instance.moveAcceleration *= 1.5f;
        Player.instance.GetComponent<Rigidbody2D>().sharedMaterial = new PhysicsMaterial2D() {
            bounciness = 2f,
            friction = 0.4f,
            bounceCombine = UnityEngine.PhysicsMaterialCombine2D.Average,
            frictionCombine = UnityEngine.PhysicsMaterialCombine2D.Average
        };
        // Set player sprite color to purple
        SpriteRenderer sr = Player.instance.GetComponent<SpriteRenderer>();
        sr.color = new Color(0.6f, 0f, 0.8f, 1f); // purple
        StartCoroutine(ResetPlayerCoroutine());
    }

    private System.Collections.IEnumerator ResetPlayerCoroutine()
    {
        yield return new WaitForSeconds(5f);
        ResetPlayer();
        // Reset player sprite color to white
        SpriteRenderer sr = Player.instance.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
    }

}
