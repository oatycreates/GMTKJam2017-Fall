using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles spawning new level pieces as the player moves further on.
/// </summary>
public class LevelManager : MonoBehaviour
{
    /// <summary>
    /// How far to place the walls away from the center line in either direction.
    /// </summary>
    public float wallXOffset = 2.25f;

    /// <summary>
    /// How tall the wall prefab pieces are in world space.
    /// </summary>
    public float wallHeightWorld = 2.0f;

    /// <summary>
    /// Unit size of the platform blocks in world space.
    /// Blocks are assumed to be square.
    /// </summary>
    public float platformBlockSizeWorld = 1.0f;

    /// <summary>
    /// Number of blocks from the bottom of each platform to the bottom of the next.
    /// </summary>
    public int platformVerticalSpacing = 3;

    /// <summary>
    /// Maximum number of blocks the platforms max shift by from the centre line when being spawned.
    /// </summary>
    public float maxNumBlocksHorizPlatformDrift = 1.5f;

    /// <summary>
    /// Unit size of the coin blocks in world space.
    /// Blocks are assumed to be square.
    /// </summary>
    public float coinBlockSizeWorld = 0.25f;

    /// <summary>
    /// Number of blocks from the bottom of each coin to the bottom of the next.
    /// </summary>
    public int coinVerticalSpacing = 3;

    /// <summary>
    /// Maximum number of blocks the coins max shift by from the centre line when being spawned.
    /// </summary>
    public int maxNumBlocksHorizCoinDrift = 2;

    /// <summary>
    /// Vertical size of the background tiles in world space.
    /// </summary>
    public float backgroundHeightWorld = 32.0f;

    /// <summary>
    /// Prefab to use for the left walls.
    /// </summary>
    public GameObject wallLeftPrefab = null;

    /// <summary>
    /// Prefab to use for the right walls.
    /// </summary>
    public GameObject wallRightPrefab = null;

    /// <summary>
    /// Prefab to use for the coins.
    /// </summary>
    public GameObject coinPrefab = null;

    /// <summary>
    /// Prefab to use for the background tiles.
    /// </summary>
    public GameObject backgroundTilePrefab = null;

    /// <summary>
    /// Prefabs to pick from when spawning a platform.
    /// </summary>
    public GameObject[] platformPrefabs = new GameObject[0];

    /// <summary>
    /// Created walls on the left side of the screen.
    /// </summary>
    private List<Transform> m_leftWalls = new List<Transform>();

    /// <summary>
    /// Created walls on the right side of the screen.
    /// </summary>
    private List<Transform> m_rightWalls = new List<Transform>();

    /// <summary>
    /// Created platforms in the scene.
    /// </summary>
    private List<Transform> m_platforms = new List<Transform>();

    /// <summary>
    /// Created coins in the scene.
    /// </summary>
    private List<Transform> m_coins = new List<Transform>();

    /// <summary>
    /// Created background tiles in the scene.
    /// </summary>
    private List<Transform> m_backgroundTiles = new List<Transform>();

    // Instance holders
    private Transform m_wallHolder = null;
    private Transform m_platformHolder = null;
    private Transform m_coinHolder = null;
    private Transform m_backgroundTileHolder = null;

    /// <summary>
    /// Number of walls required to cover the screen.
    /// Assumes that the left and right wall prefabs are the same vertical size.
    /// </summary>
    private int m_numWallsToCoverScreen = -1;

    /// <summary>
    /// Number of platforms including spacing required to cover the screen.
    /// </summary>
    private int m_numPlatformsToCoverScreen = -1;

    /// <summary>
    /// Number of coins including spacing required to cover the screen.
    /// </summary>
    private int m_numCoinsToCoverScreen = -1;

    /// <summary>
    /// Number of background tiles including spacing required to cover the screen.
    /// </summary>
    private int m_numBackgroundTilesToCoverScreen = -1;

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
        m_camera = Camera.main;
        if (!m_camera)
        {
            Debug.LogError("Could not find component Camera");
        }
        if (!wallLeftPrefab)
        {
            Debug.LogError("wallLeftPrefab not set");
        }
        if (!wallRightPrefab)
        {
            Debug.LogError("wallRightPrefab not set");
        }
        if (!coinPrefab)
        {
            Debug.LogError("coinPrefab not set");
        }
        if (!backgroundTilePrefab)
        {
            Debug.LogError("backgroundTilePrefab not set");
        }
        if (platformPrefabs.Length == 0)
        {
            Debug.LogError("platformPrefabs not set");
        }

        InitialiseLevelManager();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (isActiveAndEnabled)
        {
            DestroyLevelPiecesOffScreen();
            SpawnLevelPieces();
        }
    }

    private void InitialiseLevelManager()
    {
        // Ensure there is only one level manager
        if (GameObject.FindObjectsOfType<LevelManager>().Length != 1)
        {
            Debug.LogError("Multiple LevelManager instances in the scene, only one should be present.");
        }

        // Precalculate relevant values
        m_numWallsToCoverScreen = NumPrefabsToCoverScreen(wallHeightWorld);
        m_numPlatformsToCoverScreen = NumPrefabsToCoverScreen(platformBlockSizeWorld * platformVerticalSpacing);
        m_numCoinsToCoverScreen = NumPrefabsToCoverScreen(coinBlockSizeWorld * coinVerticalSpacing);
        m_numBackgroundTilesToCoverScreen = NumPrefabsToCoverScreen(backgroundHeightWorld);

        SpawnLevelPieces();
    }

    /// <summary>
    /// Spawns any outstanding walls, obstacles, and pickups.
    /// </summary>
    private void SpawnLevelPieces()
    {
        // TODO: Use object pools to recycle wall pieces instead of creating/destroying them

        // Walls
        if (m_leftWalls.Count < m_numWallsToCoverScreen + 1)
        {
            // Spawn some left walls
            int numWallsToSpawn = Mathf.CeilToInt(m_numWallsToCoverScreen - (m_leftWalls.Count)) + 1;
            for (int i = 0; i < numWallsToSpawn; ++i)
            {
                SpawnNextWall(ref m_leftWalls, wallLeftPrefab, -wallXOffset);
            }
        }
        if (m_rightWalls.Count < m_numWallsToCoverScreen + 1)
        {
            // Spawn some right walls
            int numWallsToSpawn = Mathf.CeilToInt(m_numWallsToCoverScreen - (m_rightWalls.Count)) + 1;
            for (int i = 0; i < numWallsToSpawn; ++i)
            {
                SpawnNextWall(ref m_rightWalls, wallRightPrefab, wallXOffset);
            }
        }

        // Obstacles
        if (m_platforms.Count < m_numPlatformsToCoverScreen + 1)
        {
            // Spawn some left walls
            int numPrefabsToSpawn = Mathf.CeilToInt(m_numPlatformsToCoverScreen - (m_platforms.Count)) + 1;
            for (int i = 0; i < numPrefabsToSpawn; ++i)
            {
                // Pick a random prefab to spawn, Random.Range upper bound is exclusive
                GameObject prefabToSpawn = platformPrefabs[UnityEngine.Random.Range(0, platformPrefabs.Length)];
                SpawnNextPlatform(ref m_platforms, prefabToSpawn);
            }
        }

        // Pickups
        if (m_coins.Count < m_numCoinsToCoverScreen + 1)
        {
            // Spawn some left walls
            int numPrefabsToSpawn = Mathf.CeilToInt(m_numCoinsToCoverScreen - (m_coins.Count)) + 1;
            for (int i = 0; i < numPrefabsToSpawn; ++i)
            {
                SpawnNextCoin(ref m_coins);
            }
        }

        // Backgrounds
        if (m_backgroundTiles.Count < m_numBackgroundTilesToCoverScreen + 1)
        {
            // Spawn some left walls
            int numPrefabsToSpawn = Mathf.CeilToInt(m_numBackgroundTilesToCoverScreen - (m_backgroundTiles.Count)) + 1;
            for (int i = 0; i < numPrefabsToSpawn; ++i)
            {
                SpawnNextBackgroundTile(ref m_backgroundTiles);
            }
        }
    }

    /// <summary>
    /// Destroys any level pieces that have fallen off the screen.
    /// </summary>
    private void DestroyLevelPiecesOffScreen()
    {
        // Walls
        for (int i = 0; i < m_leftWalls.Count;)
        {
            if (IsObjectOffScreen(m_leftWalls[i], wallHeightWorld))
            {
                GameObject.Destroy(m_leftWalls[i].gameObject);
                // Remove the wall from the list, the loop will automatically jump to the next piece due to the old index being removed
                m_leftWalls.RemoveAt(i);
            }
            else
            {
                // Test the next wall
                ++i;
            }
        }
        for (int i = 0; i < m_rightWalls.Count;)
        {
            if (IsObjectOffScreen(m_rightWalls[i], wallHeightWorld))
            {
                GameObject.Destroy(m_rightWalls[i].gameObject);
                // Remove the wall from the list, the loop will automatically jump to the next piece due to the old index being removed
                m_rightWalls.RemoveAt(i);
            }
            else
            {
                // Test the next wall
                ++i;
            }
        }

        // Platforms
        for (int i = 0; i < m_platforms.Count;)
        {
            if (IsObjectOffScreen(m_platforms[i], platformBlockSizeWorld * platformVerticalSpacing))
            {
                GameObject.Destroy(m_platforms[i].gameObject);
                // Remove the platform from the list, the loop will automatically jump to the next piece due to the old index being removed
                m_platforms.RemoveAt(i);
            }
            else
            {
                // Test the next platform
                ++i;
            }
        }

        // Pickups
        for (int i = 0; i < m_coins.Count;)
        {
            if (IsObjectOffScreen(m_coins[i], coinBlockSizeWorld * coinVerticalSpacing))
            {
                GameObject.Destroy(m_coins[i].gameObject);
                // Remove the coin from the list, the loop will automatically jump to the next piece due to the old index being removed
                m_coins.RemoveAt(i);
            }
            else
            {
                // Test the next coin
                ++i;
            }
        }

        // Background tiles
        for (int i = 0; i < m_backgroundTiles.Count;)
        {
            if (IsObjectOffScreen(m_backgroundTiles[i], backgroundHeightWorld))
            {
                GameObject.Destroy(m_backgroundTiles[i].gameObject);
                // Remove the background tile from the list, the loop will automatically jump to the next piece due to the old index being removed
                m_backgroundTiles.RemoveAt(i);
            }
            else
            {
                // Test the next backgroundTile
                ++i;
            }
        }
    }

    /// <summary>
    /// Spawns a new wall for the input wall list on top of the last piece or flush with the bottom of the screen.
    /// </summary>
    /// <param name="wallList">Wall list to append the new instance to.</param>
    /// <param name="wallPrefab">Which wall to spawn.</param>
    /// <param name="wallSpawnXOffset">X offset in world space for spawning the wall piece.</param>
    private void SpawnNextWall(ref List<Transform> wallList, GameObject wallPrefab, float wallSpawnXOffset)
    {
        // Place the new wall above the last one or start at the bottom of the screen
        Vector3 lastWallPos = Vector3.zero;
        if (wallList.Count > 0)
        {
            lastWallPos = wallList[wallList.Count - 1].position;
        }
        else
        {
            // Pretend that there is a wall piece below the screen for positioning the next one flush
            Vector3 bottomOfScreenWorld = m_camera.ViewportToWorldPoint(Vector3.zero);
            lastWallPos = new Vector3(wallSpawnXOffset, bottomOfScreenWorld.y - (wallHeightWorld * 0.5f), 0.0f);
        }

        // Spawn the new wall piece
        Vector3 newWallPos = lastWallPos + (Vector3.up * wallHeightWorld);
        GameObject newWall = GameObject.Instantiate(wallPrefab, newWallPos, Quaternion.identity, GetWallHolder());
        wallList.Add(newWall.transform);
    }

    /// <summary>
    /// Spawns a new platform for the input platform list on top of the last piece or flush with the middle of the screen.
    /// </summary>
    /// <param name="platformsList">Platform list to append the new instance to.</param>
    /// <param name="platformPrefab">Which platform to spawn.</param>
    private void SpawnNextPlatform(ref List<Transform> platformsList, GameObject platformPrefab)
    {
        float platformHeightWithSpacing = platformVerticalSpacing * platformBlockSizeWorld;

        // Place the new platform above the last one or start at the bottom of the screen
        Vector3 lastPlatformPos = Vector3.zero;
        if (platformsList.Count > 0)
        {
            lastPlatformPos = platformsList[platformsList.Count - 1].position;
        }
        else
        {
            // Pretend that there is a platform piece below the middle of the screen for positioning the next one flush
            Vector3 middleOfScreenWorld = m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
            lastPlatformPos = new Vector3(0.0f, middleOfScreenWorld.y - (platformHeightWithSpacing * 0.5f), 0.0f);
        }

        // Number of blocks to offset by in the X axis
        float numBlocksXOffset = UnityEngine.Random.Range(-maxNumBlocksHorizPlatformDrift, maxNumBlocksHorizPlatformDrift);

        // Spawn the new platform piece
        Vector3 newPlatformPos = new Vector3(numBlocksXOffset * platformBlockSizeWorld, lastPlatformPos.y + platformHeightWithSpacing, 0.0f);
        GameObject newPlatform = GameObject.Instantiate(platformPrefab, newPlatformPos, Quaternion.identity, GetPlatformHolder());
        platformsList.Add(newPlatform.transform);
    }

    /// <summary>
    /// Spawns a new coin for the input coin list on top of the last piece or flush with the middle of the screen.
    /// </summary>
    /// <param name="coinsList">Coin list to append the new instance to.</param>
    private void SpawnNextCoin(ref List<Transform> coinsList)
    {
        float coinHeightWithSpacing = coinVerticalSpacing * coinBlockSizeWorld;

        // Place the new coin above the last one or start at the bottom of the screen
        Vector3 lastCoinPos = Vector3.zero;
        if (coinsList.Count > 0)
        {
            lastCoinPos = coinsList[coinsList.Count - 1].position;
        }
        else
        {
            // Pretend that there is a coin piece below the middle of the screen for positioning the next one flush
            Vector3 middleOfScreenWorld = m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
            lastCoinPos = new Vector3(0.0f, middleOfScreenWorld.y - (coinHeightWithSpacing * 0.5f), 0.0f);
        }

        // Number of blocks to offset by in the X axis
        int numBlocksXOffset = UnityEngine.Random.Range(-maxNumBlocksHorizCoinDrift, maxNumBlocksHorizCoinDrift + 1);

        // Spawn the new coin piece
        Vector3 newCoinPos = new Vector3(numBlocksXOffset * coinBlockSizeWorld, lastCoinPos.y + coinHeightWithSpacing, 0.0f);
        GameObject newCoin = GameObject.Instantiate(coinPrefab, newCoinPos, Quaternion.identity, GetCoinHolder());
        coinsList.Add(newCoin.transform);
    }

    /// <summary>
    /// Spawns a new backgroundTile for the input backgroundTile list on top of the last piece or flush with the middle of the screen.
    /// </summary>
    /// <param name="backgroundTilesList">BackgroundTile list to append the new instance to.</param>
    private void SpawnNextBackgroundTile(ref List<Transform> backgroundTilesList)
    {
        // Place the new background tile above the last one or start at the bottom of the screen
        Vector3 lastBackgroundTilePos = Vector3.zero;
        if (backgroundTilesList.Count > 0)
        {
            lastBackgroundTilePos = backgroundTilesList[backgroundTilesList.Count - 1].position;
        }
        else
        {
            // Pretend that there is a backgroundTile piece bottom of the screen for positioning the next one flush
            Vector3 middleOfScreenWorld = m_camera.ViewportToWorldPoint(Vector3.zero);
            lastBackgroundTilePos = new Vector3(0.0f, middleOfScreenWorld.y - (backgroundHeightWorld * 0.5f), 0.0f);
        }

        // Spawn the new backgroundTile piece
        Vector3 newBackgroundTilePos = Vector3.up * (lastBackgroundTilePos.y + backgroundHeightWorld);
        GameObject newBackgroundTile = GameObject.Instantiate(backgroundTilePrefab, newBackgroundTilePos, Quaternion.identity, GetBackgroundTileHolder());
        backgroundTilesList.Add(newBackgroundTile.transform);
    }

    /// <summary>
    /// Returns an empty object for holding the spawned walls.
    /// </summary>
    /// <returns></returns>
    private Transform GetWallHolder()
    {
        if (!m_wallHolder)
        {
            GameObject wallHolderGameObject = new GameObject("WallHolder");
            m_wallHolder = wallHolderGameObject.transform;
            m_wallHolder.parent = m_transform;
        }
        return m_wallHolder;
    }

    /// <summary>
    /// Returns an empty object for holding the spawned platforms.
    /// </summary>
    /// <returns></returns>
    private Transform GetPlatformHolder()
    {
        if (!m_platformHolder)
        {
            GameObject platformHolderGameObject = new GameObject("PlatformHolder");
            m_platformHolder = platformHolderGameObject.transform;
            m_platformHolder.parent = m_transform;
        }
        return m_platformHolder;
    }

    /// <summary>
    /// Returns an empty object for holding the spawned coins.
    /// </summary>
    /// <returns></returns>
    private Transform GetCoinHolder()
    {
        if (!m_coinHolder)
        {
            GameObject coinHolderGameObject = new GameObject("CoinHolder");
            m_coinHolder = coinHolderGameObject.transform;
            m_coinHolder.parent = m_transform;
        }
        return m_coinHolder;
    }

    /// <summary>
    /// Returns an empty object for holding the spawned background tiles.
    /// </summary>
    /// <returns></returns>
    private Transform GetBackgroundTileHolder()
    {
        if (!m_backgroundTileHolder)
        {
            GameObject backgroundTileHolderGameObject = new GameObject("BackgroundTileHolder");
            m_backgroundTileHolder = backgroundTileHolderGameObject.transform;
            m_backgroundTileHolder.parent = m_transform;
        }
        return m_backgroundTileHolder;
    }

    /// <summary>
    /// Determines how many prefabs with the input height will be required to cover the screen.
    /// Assumes that the left and right wall prefabs are the same vertical size.
    /// This value likely wont change and should be cached per prefab on startup.
    /// </summary>
    /// <param name="prefabHeightWorld">Height of the prefab to fill the screen with.</param>
    /// <returns>Number of prefabs with the input height required to cover the screen.</returns>
    private int NumPrefabsToCoverScreen(float prefabHeightWorld)
    {
        Vector3 screenPrefabTop = m_camera.WorldToScreenPoint(Vector3.up * prefabHeightWorld);
        Vector3 screenPrefabBottom = m_camera.WorldToScreenPoint(Vector3.zero);
        float prefabScreenSizeY = (screenPrefabTop - screenPrefabBottom).y;

        return Mathf.CeilToInt(m_camera.pixelHeight / prefabScreenSizeY);
    }

    /// <summary>
    /// Tests if the input object instance is at least a full height beneath the screen.
    /// </summary>
    /// <param name="objectTransform">Object transform to test for whether it is beneath the screen.</param>
    /// <param name="objectHeightWorld">Height of the object in world space.</param>
    private bool IsObjectOffScreen(Transform objectTransform, float objectHeightWorld)
    {
        // If the object is a half object height beneath the viewport, it can safely be considered as below.
        Vector3 objectPos = objectTransform.position;
        Vector3 objectPosViewport = m_camera.WorldToViewportPoint(objectPos + (Vector3.up * objectHeightWorld * 0.5f));
        return objectPosViewport.y < 0.0f;
    }
}
