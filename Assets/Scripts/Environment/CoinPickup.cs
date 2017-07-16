using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    /// <summary>
    /// How many points the coin is worth at a base value before any multipliers.
    /// </summary>
    public float baseCoinValue = 1.0f;

    // Cached component references
    private GameManager m_gameManager = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
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
		
	}

    /// <summary>
    /// Call this to claim the coin's value and remove it.
    /// </summary>
    public void ClaimCoinValue()
    {
        // TODO: Use object pools for coin claiming instead of destroying

        m_gameManager.AddScore(baseCoinValue);
        gameObject.SetActive(false);
    }
}
