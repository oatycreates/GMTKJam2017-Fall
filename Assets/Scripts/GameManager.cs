using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioSource gameOverSound = null;

    /// <summary>
    /// Percentage of the screen's height to place the kill zone at.
    /// </summary>
    public float screenKillHeightPerc = -0.05f;

    /// <summary>
    /// Current player's score total for this run.
    /// </summary>
    private float m_playerScore = 0.0f;

    /// <summary>
    /// Reference to the player's Transform component.
    /// </summary>
    private Transform m_playerTransform = null;

    /// <summary>
    /// Reference to the player's Rigidbody2D component.
    /// </summary>
    private Rigidbody2D m_playerRigidbody = null;

    /// <summary>
    /// Used to freeze the player in the game over screen.
    /// </summary>
    private RigidbodyConstraints2D m_playerRigidybodyConstraints = RigidbodyConstraints2D.FreezeRotation;

    /// <summary>
    /// Reference to the game over UI holder element. Will be tagged 'GameOver'.
    /// </summary>
    private GameObject m_gameOverHolder = null;

    /// <summary>
    /// Shows the player's score on the game over screen.
    /// </summary>
    private Text m_gameOverScoreText = null;

    /// <summary>
    /// Reference to the game score UI holder element. Will be named 'GameScoreHolder'.
    /// </summary>
    private GameObject m_gameScoreHolder = null;

    /// <summary>
    /// Shows the player's score in the main game screen.
    /// </summary>
    private Text m_gameScoreText = null;

    private bool m_isGameOver = false;
    /// <summary>
    /// Whether the game is presently over.
    /// </summary>
    public bool isGameOver
    {
        get { return m_isGameOver; }
    }

    /// <summary>
    /// Amount of time to show the game over screen for before allowing the
    /// player to close it to prevent instant closing.
    /// </summary>
    private float m_gameOverCooldownTimer = -1.0f;

    // Cached component references
    private Camera m_camera = null;

    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        m_playerTransform = playerObject.GetComponent<Transform>();
        if (!m_playerTransform)
        {
            Debug.LogError("Could not find component Player Transform");
        }
        m_playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
        if (!m_playerRigidbody)
        {
            Debug.LogError("Could not find component Player Rigidbody2D");
        }
        m_playerRigidybodyConstraints = m_playerRigidbody.constraints;
        m_gameOverHolder = GameObject.FindGameObjectWithTag("GameOver");
        if (!m_gameOverHolder)
        {
            Debug.LogError("Could not find component game over holder GameObject");
        }
        m_gameOverScoreText = GameObject.Find("GameOverScoreText").GetComponent<Text>();
        if (!m_gameOverScoreText)
        {
            Debug.LogError("Could not find component game over GameOverScoreText Text");
        }
        m_gameScoreHolder = GameObject.Find("GameScoreHolder");
        if (!m_gameScoreHolder)
        {
            Debug.LogError("Could not find component game score holder GameObject");
        }
        m_gameScoreText = GameObject.Find("GameScoreText").GetComponent<Text>();
        if (!m_gameScoreText)
        {
            Debug.LogError("Could not find component game over GameScoreText Text");
        }
        m_camera = Camera.main;
        if (!m_camera)
        {
            Debug.LogError("Could not find component Camera");
        }

        StartLevel();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {
            CheckPlayerGameOver();
        }
    }

    /// <summary>
    /// Adds to the player's score.
    /// </summary>
    /// <param name="score">Score to add.</param>
    public void AddScore(float score)
    {
        m_playerScore += score;
  
        // Update score total in main game score display
        m_gameScoreText.text = GetScoreText();
    }

    /// <summary>
    /// Ends the game and shows the game over screen.
    /// </summary>
    public void TriggerGameOver()
    {
        // Show game over screen
        m_isGameOver = true;
        m_gameOverHolder.SetActive(true);
        m_gameScoreHolder.SetActive(false);

        // Stop the game over screen from instantly closing
        m_gameOverCooldownTimer = 0.3f;

        // Update score total in game over display
        m_gameOverScoreText.text = GetScoreText();

        gameOverSound.Play();
    }

    /// <summary>
    /// Returns the formatted player score text.
    /// Will have commas for each thousand.
    /// </summary>
    /// <returns>Formatted player score text.</returns>
    private string GetScoreText()
    {
        return string.Format("{0:n0}", m_playerScore);
    }

    /// <summary>
    /// Initialises the level objects.
    /// </summary>
    private void StartLevel()
    {
        // Ensure there is only one game manager
        if (GameObject.FindObjectsOfType<GameManager>().Length != 1)
        {
            Debug.LogError("Multiple GameManager instances in the scene, only one should be present.");
        }
        
        // Hide the game over screen
        m_isGameOver = false;
        m_gameOverHolder.SetActive(false);
        m_gameScoreHolder.SetActive(true);
 
        // Update score total in main game score display
        m_gameScoreText.text = GetScoreText();
    }

    /// <summary>
    /// Triggers the game over scene if the player falls off the screen.
    /// </summary>
    private void CheckPlayerGameOver()
    {
        // Decrease game over cooldown timer
        m_gameOverCooldownTimer -= Time.deltaTime;

        if (!m_isGameOver)
        {
            // Work out where the bottom of the screen sits in world space
            Vector3 thresholdViewportPos = Vector3.up * screenKillHeightPerc;
            float heightThreshold = m_camera.ViewportToWorldPoint(thresholdViewportPos).y;

            if (m_playerTransform.position.y < heightThreshold)
            {
                Debug.Log("Player beneath kill Y, Game Over!");

                // Player beneath kill Y, show game over screen
                TriggerGameOver();
            }
        }
        else if (m_isGameOver)
        {
            // Check if the player interacts with the screen, if so, reset the level
            if (m_gameOverCooldownTimer <= 0.0f && Input.GetButtonUp("Fire1"))
            {
                // Restore the player's rigidbody constrians
                m_playerRigidbody.constraints = m_playerRigidybodyConstraints;

                // Reset the level
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                // Stop the player from moving so that they don't fall infinitely
                m_playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }
}
