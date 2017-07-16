using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpFollow : MonoBehaviour
{
    /// <summary>
    /// Object to follow, the follow point will be the offset between the target and this object on start.
    /// </summary>
    public Transform lerpTarget = null;

    /// <summary>
    /// Lerp percentage per secont to move towards the target.
    /// </summary>
    public float lerpFollowSpeed = 4.0f;

    /// <summary>
    /// Offset from the starting position of this object to the starting position of the target.
    /// </summary>
    private Vector3 startOffsetToTarget = Vector3.zero;

    // Cached component references
    private Transform m_transform = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        m_transform = gameObject.GetComponentInChildren<Transform>();
        if (!m_transform)
        {
            Debug.LogError("Could not find component Transform");
        }
        if (!lerpTarget)
        {
            Debug.LogError("lerpTarget not set");
        }

        startOffsetToTarget = lerpTarget.position - m_transform.position;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {
            m_transform.position = Vector3.Lerp(m_transform.position, lerpTarget.position - startOffsetToTarget, lerpFollowSpeed * Time.deltaTime);
        }
    }
}
