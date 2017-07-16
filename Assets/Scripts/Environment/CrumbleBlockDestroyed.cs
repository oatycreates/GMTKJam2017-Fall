using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumbleBlockDestroyed : MonoBehaviour
{
    /// <summary>
    /// Game object to destroy once the animation completes.
    /// </summary>
    public GameObject destroyOnAnimComplete = null;

    // Cached component references
    private Animator m_animator = null;

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
        if (!destroyOnAnimComplete)
        {
            Debug.LogError("destroyOnAnimComplete not set");
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {

        }
    }

    /// <summary>
    /// Called by the CrumbleBlock_Destroyed animation event.
    /// </summary>
    public void OnDestroyAnimComplete()
    {
        GameObject.Destroy(destroyOnAnimComplete);
    }
}
