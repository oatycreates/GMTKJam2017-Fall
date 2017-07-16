using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    // Cached component references
    private Animator m_animator = null;
    private Rigidbody2D m_rigidbody = null;
    private PlayerInput m_playerInput = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        m_animator = gameObject.GetComponentInChildren<Animator>();
        if (!m_animator)
        {
            Debug.LogError("Could not find component Animator");
        }
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
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {
            UpdatePlayerAnimState();
        }
    }

    /// <summary>
    /// Plays the bounce animation.
    /// </summary>
    public void TriggerBounceAnim()
    {
        m_animator.SetTrigger("JustBounced");
    }

    /// <summary>
    /// Updates any relevant animation state values for the player.
    /// </summary>
    private void UpdatePlayerAnimState()
    {
        m_animator.SetFloat("PlayerSpeed", m_rigidbody.velocity.magnitude);
        m_animator.SetBool("IsPreparingSling", m_playerInput.isSlingshotDown);
    }
}
