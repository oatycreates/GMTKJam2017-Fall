using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlatform : MonoBehaviour
{
    /// <summary>
    /// Number of hits to destroy the block, if it is set to less than zero, the block won't be destroyable.
    /// </summary>
    public int hitsToDestroy = -1;

    /// <summary>
    /// Prefab to spawn when the crumble block is destroyed.
    /// </summary>
    public GameObject crumbleBlockDestroyedPrefab = null;

    // Cached component references
    private Transform m_transform = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        if (hitsToDestroy >= 0 && !crumbleBlockDestroyedPrefab)
        {
            Debug.LogError("crumbleBlockDestroyedPrefab not set and hitsToDestroy is >= 0!");
        }
        m_transform = gameObject.GetComponent<Transform>();
        if (!m_transform)
        {
            Debug.LogError("Could not find component Transform");
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {

    }

    /// <summary>
    /// Handles destroying the block once a sufficient number of hits have been sustained.
    /// </summary>
    /// <param name="other">Object colliding with this block.</param>
    /// <param name="collisionOther">Collision information from the other object.</param>
    public void HandleBlockDamage(GameObject other, Collision2D collisionOther)
    {
        // Ignore hits for tough blocks
        if (hitsToDestroy != -1)
        {
            hitsToDestroy -= 1;
            Debug.Log("Block hit! Hits left to destroy: " + hitsToDestroy);
            if (hitsToDestroy <= 0)
            {
                // TODO: Use object pools for disabling block platforms instead of destroying

                // Spawn the destroy block and attach to this block's parent
                GameObject.Instantiate(crumbleBlockDestroyedPrefab, m_transform.position, m_transform.rotation, m_transform.parent);

                GameObject.Destroy(gameObject);
            }
        }
    }
}
