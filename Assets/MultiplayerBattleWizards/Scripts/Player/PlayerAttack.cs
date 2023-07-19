using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the casting of spells.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Spells")]
    public SpellContainer curSpell;  // Player's current spell.
    public bool canAttack;           // Can the player attack?

    [Header("Component")]
    public Transform spellSpawnPos;

    // Private components.
    private Player player;
    private Rigidbody2D rig;

    #region Subscribing to Input Events

    void OnEnable ()
    {
        player.input.onAttackDown.AddListener(OnAttackInput);
    }

    void OnDisable ()
    {
        player.input.onAttackDown.RemoveListener(OnAttackInput);
    }

    #endregion

    // Sets components referenced from the 'Player' class.
    public void SetComponents (Player playerClass)
    {
        player = playerClass;
        rig = player.rig;
    }

    // Called when the 'Attack' key is pressed.
    // Invoked from the 'onAttackDown' event in PlayerInput.
    void OnAttackInput ()
    {
        if(curSpell.CanCast() && canAttack)
            CastSpell(true, spellSpawnPos.position);
    }

    // Casts the player's current spell.
    [PunRPC]
    public void CastSpell (bool isMine, Vector3 spawnPos)
    {
        curSpell.OnCast();

        if(curSpell.spell.castType == SpellCastType.Projectile)
        {
            // Create projectile.
            GameObject projectile = Instantiate(curSpell.spell.projectilePrefab, spawnPos, Quaternion.identity);
            SpellProjectile projScript = projectile.GetComponent<SpellProjectile>();

            // Calculate velocity.
            Vector2 projVelocity = new Vector2(curSpell.spell.projectileSpeed * player.move.facingDirection, 0);

            // Rotate to face its moving direction.
            projectile.transform.localScale = new Vector3(player.move.facingDirection, 1, 1);

            // Initiate it with the data it needs.
            projScript.Initiate(new SpellProjectileData(player.networkPlayer.networkID, isMine, curSpell.spell, projVelocity));
        }
        else if(curSpell.spell.castType == SpellCastType.Self)
        {
            GameObject spellObj = new GameObject("SelfSpell");
            SpellSelf self = spellObj.AddComponent<SpellSelf>();

            self.Initiate(player.networkPlayer.networkID, isMine, curSpell.spell);
        }

        // Play sound effect.
        AudioManager.inst.Play(player.audioSource, curSpell.spell.castSfx, true);

        // Spawn a projectile for all other clients.
        if(player.photonView.isMine)
        {
            player.photonView.RPC("CastSpell", PhotonTargets.Others, false, spellSpawnPos.position);
            player.onCastSpell.Invoke();
        }

        // Show the cooldown on the player's UI.
        player.ui.SetCooldown();
    }

    // Called when the player is given a new spell.
    [PunRPC]
    public void GiveSpell (SpellData spell)
    {
        curSpell = new SpellContainer(spell);
        player.ui.spellIcon.sprite = spell.icon;
    }

    // Called when the player is given a new spell.
    [PunRPC]
    public void GiveSpell (string spellName)
    {
        SpellData spell = SpellManager.inst.GetSpell(spellName);
        curSpell = new SpellContainer(spell);
        player.ui.spellIcon.sprite = spell.icon;
    }
}