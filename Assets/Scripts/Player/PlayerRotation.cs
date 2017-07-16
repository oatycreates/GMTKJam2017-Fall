using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lib;

public class PlayerRotation : MonoBehaviour
{
    /// <summary>
    /// Percentage of the desired rotation to lerp towards per second.
    /// </summary>
    public float rotationLerpRate = 8.0f;

    /// <summary>
    /// Sound to play while moving.
    /// </summary>
    public AudioSource movementSound = null;

    // Cached component references
    private Rigidbody2D m_rigidbody = null;
    private PlayerInput m_playerInput = null;
    private GameManager m_gameManager = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        m_rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (!m_rigidbody)
        {
            Debug.LogError("Could not find component Rigidbody2D");
        }
        m_playerInput = gameObject.GetComponent<PlayerInput>();
        if (!m_playerInput)
        {
            Debug.LogError("Could not find component PlayerInput");
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
        if (isActiveAndEnabled && !m_gameManager.isGameOver)
        {
            PlayerRotateTick();
        }
    }

    /// <summary>
    /// Sets or lerps the player's rotation to the input angle.
    /// </summary>
    /// <param name="rotationAngleDeg"></param>
    /// <param name="lerpRotation"></param>
    public void UpdatePlayerRotation(float rotationAngleDeg, bool lerpRotation, float lerpRateMult = 1.0f)
    {
        if (lerpRotation)
        {
            // Lerp rotation
            m_rigidbody.rotation = Mathf.LerpAngle(m_rigidbody.rotation, rotationAngleDeg, rotationLerpRate * lerpRateMult * Time.deltaTime);
        }
        else
        {
            // Snap rotation
            m_rigidbody.rotation = rotationAngleDeg;
        }
    }

    private void PlayerRotateTick()
    {
        if (m_playerInput.isSlingshotDown)
        {
            // Lerp the rotate to keep it smooth but lerp faster so that the player's facing better reflects their input
            Vector2 launchDir = m_playerInput.GetSlingshotDirection();
            UpdatePlayerRotation(DirectionHelpers.RotationAngleForVector(launchDir) * Mathf.Rad2Deg, true, 4.0f);
        }
        else
        {
            // Lerp rotate to face velocity
            Vector2 playerVelocity = m_rigidbody.velocity;
            float rotationAngleDeg = 0.0f; // Lerp to face up if no velocity present
            if (playerVelocity.sqrMagnitude > 0.1f)
            {
                rotationAngleDeg = DirectionHelpers.RotationAngleForVector(playerVelocity.normalized) * Mathf.Rad2Deg;

                if (!movementSound.isPlaying)
                {
                    movementSound.Play();
                }
            }
            else
            {
                if (movementSound.isPlaying)
                {
                    movementSound.Stop();
                }
            }
            UpdatePlayerRotation(rotationAngleDeg, true);
        }
    }
}
