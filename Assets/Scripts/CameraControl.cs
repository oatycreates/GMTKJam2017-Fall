using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    /// <summary>
    /// How much of the overall height of the screen the player needs to move
    /// up before the scroll starts matching the player's upwards movement.
    /// </summary>
    public float screenHeightDeadzonePercent = 0.5f;

    /// <summary>
    /// Lerp rate per second to chase the player once they move high enough.
    /// </summary>
    public float screenChaseLerpFactor = 2.5f;

    /// <summary>
    /// Reference to the player's rigidbody component.
    /// </summary>
    private Rigidbody2D m_playerRigidbody = null;

    // Cached component references
    private Transform m_transform = null;
    private Camera m_camera = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        m_transform = GetComponent<Transform>();
        if (!m_transform)
        {
            Debug.LogError("Could not find component Transform");
        }
        m_camera = GetComponent<Camera>();
        if (!m_camera)
        {
            Debug.LogError("Could not find component Camera");
        }
        m_playerRigidbody = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        if (!m_playerRigidbody)
        {
            Debug.LogError("Could not find component Player Rigidbody2D");
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {
            UpdatePlayerFollow();
        }
    }

    /// <summary>
    /// Moves the camera up if the player moves high enough.
    /// </summary>
    private void UpdatePlayerFollow()
    {
        // Work out where the screen threshold percentage sits in world space
        Vector3 thresholdViewportPos = Vector3.up * screenHeightDeadzonePercent;
        float heightThresholdWorld = m_camera.ViewportToWorldPoint(thresholdViewportPos).y;

        // If the player has moved above the screen threshold, slide the screen up.
        Vector2 playerPos = m_playerRigidbody.position;
        if (playerPos.y > heightThresholdWorld)
        {
            // Find the mid point of the screen so that we can work out where to move the middle of the screen to keep the player visually on the deadzone
            float screenMiddleYWorld = m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f)).y;
            float cameraYOffsetWorld = screenMiddleYWorld - heightThresholdWorld;

            // Lerp to match the player's position
            Vector3 newCamPos = new Vector3(m_transform.position.x, playerPos.y + cameraYOffsetWorld, m_transform.position.z);
            m_transform.position = Vector3.Lerp(m_transform.position, newCamPos, screenChaseLerpFactor * Time.deltaTime);
        }
    }
}
