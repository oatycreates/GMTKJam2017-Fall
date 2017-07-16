using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lib;
using System;

public class PlayerPhysics : MonoBehaviour
{
    public AudioSource bounceSound = null;
    public AudioSource landSound = null;

    /// <summary>
    /// Percentage of the player's speed to conserve when bouncing off a wall.
    /// Set to values large than one to boost speed on contact.
    /// </summary>
    public float wallSpeedBouncePercent = 1.05f;

    /// <summary>
    /// Cached copy of the rigidbody's velocity updated each UpdateTick.
    /// </summary>
    private Vector2 m_rigidbodyVelocity = Vector2.zero;

    // Cached component references
    private Transform m_transform = null;
    private Rigidbody2D m_rigidbody = null;
    private PlayerInput m_playerInput = null;
    private PlayerAnimation m_playerAnimation = null;
    private PlayerRotation m_playerRotation = null;
    private GameManager m_gameManager = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        m_transform = gameObject.GetComponent<Transform>();
        if (!m_transform)
        {
            Debug.LogError("Could not find component Transform");
        }
        m_rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (!m_rigidbody)
        {
            Debug.LogError("Could not find component Rigidbody2D");
        }
        m_playerInput = gameObject.GetComponent<PlayerInput>();
        if (!m_playerInput) {
            Debug.LogError("Could not find component PlayerInput");
        }
        m_playerAnimation = gameObject.GetComponent<PlayerAnimation>();
        if (!m_playerAnimation)
        {
            Debug.LogError("Could not find component PlayerAnimation");
        }
        m_playerRotation = gameObject.GetComponent<PlayerRotation>();
        if (!m_playerRotation)
        {
            Debug.LogError("Could not find component PlayerRotation");
        }
        m_gameManager = GameObject.FindObjectOfType<GameManager>();
        if (!m_gameManager)
        {
            Debug.LogError("Could not find component GameManager");
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {
            VelocityModifyTick();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isActiveAndEnabled)
        {
            CollisionBounce(collision);
            HandleBlockHits(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider2d)
    {
        if (isActiveAndEnabled)
        {
            CollectCoins(collider2d);
            HandleSpikeHits(collider2d);
        }
    }

    /// <summary>
    /// Launches the player in the specified direction. Will give a small boost to the speed and change direction.
    /// </summary>
    /// <param name="launchDir">Direction vector to launch the player in.</param>
    public void LaunchPlayer(Vector2 launchDir)
    {
        float velIncreaseMult = 1.1f;
        float minLaunchVel = 10.0f;
        m_rigidbody.velocity = launchDir * Mathf.Max(m_rigidbody.velocity.magnitude * velIncreaseMult, minLaunchVel);
    }

    /// <summary>
    /// Called each update tick, use this function to modify the player's velocity or other physics quantities.
    /// Note that this method is not called during FixedUpdate so that may be needed in future.
    /// </summary>
    private void VelocityModifyTick()
    {
        m_rigidbodyVelocity = m_rigidbody.velocity;

        // Slow time while aiming for a dramatic effect
        if (m_playerInput.isSlingshotDown)
        {
            Time.timeScale = 0.1f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }

    /// <summary>
    /// Bounces the player off the collision if the collision direction is relevant.
    /// </summary>
    /// <param name="collision">Collision information.</param>
    private void CollisionBounce(Collision2D collision)
    {
        // Work out whether the contact is horizontal
        DirectionHelpers.Direction collisionDirection =
            DirectionHelpers.GetCollisionDirection(m_transform.position, collision.contacts);
        
        if (collisionDirection == DirectionHelpers.Direction.LEFT ||
            collisionDirection == DirectionHelpers.Direction.RIGHT)
        {
            // Reflect X component of collision velocity
            m_rigidbody.velocity = new Vector2(-m_rigidbodyVelocity.x, m_rigidbodyVelocity.y);

            // Apply bounce velocity slowdown or boost to new velocity vector
            m_rigidbody.velocity = m_rigidbody.velocity.normalized * (m_rigidbody.velocity.magnitude * wallSpeedBouncePercent);

            // Make the player face the new bounced velocity direction
            m_playerRotation.UpdatePlayerRotation(DirectionHelpers.RotationAngleForVector(m_rigidbody.velocity), false);

            // Play bounce animation
            m_playerAnimation.TriggerBounceAnim();

            bounceSound.Play();
        }

        // Landing/hitting a platform from above/below
        if (collisionDirection == DirectionHelpers.Direction.DOWN ||
            collisionDirection == DirectionHelpers.Direction.UP)
        {
            landSound.Play();
        }
    }

    /// <summary>
    /// Checks if the trigger being entered is a coin and collects it if so.
    /// </summary>
    /// <param name="collider2d"></param>
    private void CollectCoins(Collider2D collider2d)
    {
        // The collider will be nested under the parent
        CoinPickup coinScript = collider2d.GetComponentInParent<CoinPickup>();
        if (coinScript)
        {
            coinScript.ClaimCoinValue();
        }
    }

    /// <summary>
    /// Checks if the trigger being entered is a spike and ends the game if so.
    /// </summary>
    /// <param name="collider2d"></param>
    private void HandleSpikeHits(Collider2D collider2d)
    {
        if (collider2d.gameObject.CompareTag("BlockSpike"))
        {
            m_gameManager.TriggerGameOver();
        }
    }

    /// <summary>
    /// Handles destroying the block once a sufficient number of hits have been sustained.
    /// </summary>
    /// <param name="collision">Collision information.</param>
    private void HandleBlockHits(Collision2D collision)
    {
        // The collider will be nested under the parent
        BlockPlatform blockScript = collision.gameObject.GetComponentInParent<BlockPlatform>();
        if (blockScript)
        {
            blockScript.HandleBlockDamage(gameObject, collision);
        }
    }
}
