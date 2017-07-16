using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lib;

public class PlayerInput : MonoBehaviour
{
    public AudioSource slingshotPullSound = null;
    public AudioSource slingshotReleaseSound = null;

    /// <summary>
    /// Whether the player is attempting to set up a slingshot of themselves.
    /// </summary>
    private bool m_isSlingshotDown = false;
    public bool isSlingshotDown
    {
        get { return m_isSlingshotDown; }
    }

    /// <summary>
    /// Start point for where the player clicked/touched/etc. to offset the movement by.
    /// </summary>
    private Vector2 m_slingshotStartPos = Vector2.zero;

    /// <summary>
    /// Whether a controller is being used for the current slingshot.
    /// </summary>
    private bool m_usingControllerForSlingshot = false;

    // Cached component references
    private PlayerPhysics m_playerPhysics = null;
    private PlayerRotation m_playerRotation = null;
    private Camera m_camera = null;
    private SlingshotDirectionIndicator m_slingshotDirectionIndicator = null;
    private GameManager m_gameManager = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        m_playerPhysics = gameObject.GetComponent<PlayerPhysics>();
        if (!m_playerPhysics)
        {
            Debug.LogError("Could not find component PlayerPhysics");
        }
        m_playerRotation = gameObject.GetComponent<PlayerRotation>();
        if (!m_playerRotation)
        {
            Debug.LogError("Could not find component PlayerRotation");
        }
        m_camera = Camera.main;
        if (!m_camera)
        {
            Debug.LogError("Could not find component Camera");
        }
        m_slingshotDirectionIndicator = GameObject.FindObjectOfType<SlingshotDirectionIndicator>();
        if (!m_slingshotDirectionIndicator)
        {
            Debug.LogError("Could not find component SlingshotDirectionIndicator");
        }
        m_gameManager = GameObject.FindObjectOfType<GameManager>();
        if (!m_gameManager)
        {
            Debug.LogError("Could not find component GameManager");
        }

        // Hide the slingshot input until needed
        m_slingshotDirectionIndicator.StopSlingshotInput();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled && !m_gameManager.isGameOver)
        {
            HandleSlingshotInput();
        }
    }

    /// <summary>
    /// Calculates and returns the normalised direction that the player is trying to slingshot themselves in.
    /// </summary>
    /// <returns>Normalised press direction vector.</returns>
    public Vector2 GetSlingshotDirection()
    {
        if (m_usingControllerForSlingshot)
        {
            // Simply read the controller axis as controllers don't touch/drag unlike cursors
            return -(GetControllerAxis()).normalized;
        }
        else
        {
            // Mouse/touch inputs will press and drag from a start position
            return (m_slingshotStartPos - GetPressPos()).normalized;
        }
    }

    /// <summary>
    /// Handles starting and stopping (triggering) the slingshot input.
    /// </summary>
    private void HandleSlingshotInput()
    {
        // Press to start slingshotting, release to stop
        if (!m_isSlingshotDown && Input.GetButtonDown("Fire1"))
        {
            m_isSlingshotDown = true;
            m_usingControllerForSlingshot = false; // Will be set to true in GetPressPos if using controller
            m_slingshotStartPos = GetPressPos();

            slingshotPullSound.Play();
            slingshotReleaseSound.Stop();

            // Only show the directional input element on touch/mouse setups as
            // they make it harder to tell where the input will take the player.
            if (!m_usingControllerForSlingshot)
            {
                m_slingshotDirectionIndicator.StartSlingshotInput(m_slingshotStartPos);
            }
        }
        else if (m_isSlingshotDown && Input.GetButtonUp("Fire1"))
        {
            m_isSlingshotDown = false;
            Vector2 launchDir = GetSlingshotDirection();
            m_playerPhysics.LaunchPlayer(launchDir);
            m_playerRotation.UpdatePlayerRotation(DirectionHelpers.RotationAngleForVector(launchDir) * Mathf.Rad2Deg, false);

            slingshotReleaseSound.Play();
            slingshotPullSound.Stop();

            if (!m_usingControllerForSlingshot)
            {
                m_slingshotDirectionIndicator.StopSlingshotInput();
            }
        }
    }

    /// <summary>
    /// Returns a vector 2 representation of the controller input axis state.
    /// </summary>
    /// <returns>Vector 2 representing controller input axis state.</returns>
    private Vector2 GetControllerAxis()
    {
        float controllerX = Input.GetAxis("Horizontal");
        float controllerY = Input.GetAxis("Vertical");
        return new Vector2(controllerX, controllerY);
    }

    /// <summary>
    /// Returns the position the user is pressing on the screen.
    /// </summary>
    /// <returns>Position the user is pressing in screen space.</returns>
    private Vector2 GetPressPos()
    {
        Vector2 currentInputPos = Vector2.zero;

        // Controller input
        Vector2 controllerAxis = GetControllerAxis();
        if (!Mathf.Approximately(controllerAxis.x, 0.0f) || !Mathf.Approximately(controllerAxis.y, 0.0f))
        {
            currentInputPos = controllerAxis;
            m_usingControllerForSlingshot = true;
        }
        else
        {
            // Use touch/mouse if controller fails as mouse will generally always return a position value

            // Touch/Mouse input
            Vector3 cursorPosScreen = Input.mousePosition;
            if (!Mathf.Approximately(cursorPosScreen.x, 0.0f) || !Mathf.Approximately(cursorPosScreen.y, 0.0f))
            {
                currentInputPos = new Vector2(cursorPosScreen.x, cursorPosScreen.y);
            }
        }

        return currentInputPos;
    }
}
