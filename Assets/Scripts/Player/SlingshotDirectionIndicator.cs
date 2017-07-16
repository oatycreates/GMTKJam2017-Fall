using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lib;

public class SlingshotDirectionIndicator : MonoBehaviour
{
    private bool initialisedRefs = false;

    // Cached component references
    private Transform m_transform = null;
    private PlayerInput m_playerInput = null;
    private Camera m_camera = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        InitialiseRefsIfUnset();
    }

    private void InitialiseRefsIfUnset()
    {
        if (!initialisedRefs)
        {
            m_transform = gameObject.GetComponent<Transform>();
            if (!m_transform)
            {
                Debug.LogError("Could not find component Transform");
            }
            m_playerInput = GameObject.FindObjectOfType<PlayerInput>();
            if (!m_playerInput)
            {
                Debug.LogError("Could not find component PlayerInput");
            }
            m_camera = Camera.main;
            if (!m_camera)
            {
                Debug.LogError("Could not find component Camera");
            }

            initialisedRefs = true;
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {
            RotateSlingshotDirectionIndicator();
        }
    }

    /// <summary>
    /// Called once the player begins the slingshot input.
    /// </summary>
    /// <param name="slingshotInputStartPos">Where the slingshot input started in screen space.</param>
    public void StartSlingshotInput(Vector2 slingshotInputStartPos)
    {
        gameObject.SetActive(true);

        // Initialise refs if they haven't been already
        InitialiseRefsIfUnset();

        Vector3 startWorldPoint = m_camera.ScreenToWorldPoint(new Vector3(slingshotInputStartPos.x, slingshotInputStartPos.y, 0.0f));
        m_transform.position = new Vector3(startWorldPoint.x, startWorldPoint.y, m_transform.position.z);
    }

    /// <summary>
    /// Called once the player releases the slingshot input.
    /// </summary>
    public void StopSlingshotInput()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Makes the slingshot direction indicator face along the input direction vector.
    /// </summary>
    private void RotateSlingshotDirectionIndicator()
    {
        if (m_playerInput.isSlingshotDown)
        {
            // Rotate to face input
            Vector2 slingshotVector = m_playerInput.GetSlingshotDirection();
            float rotationAngle = DirectionHelpers.RotationAngleForVector(slingshotVector);
            m_transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationAngle * Mathf.Rad2Deg);
        }
    }
}
