using UnityEngine;

public class InfiniteScrollerADVANCED : MonoBehaviour
{
    public enum ScrollDirection { Up, Down, Left, Right }
    [SerializeField] private ScrollDirection direction = ScrollDirection.Left;
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private float scrollSpeed = 5f;

    private GameObject[] activeTiles;
    private float tileSize;
    private float screenBounds;
    private Vector2 movementVector;
    private bool isHorizontal;

    void Start()
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogError("No tile prefabs assigned!");
            return;
        }

        // Calculate screen bounds based on scroll direction (horizontal or vertical)
        Camera cam = Camera.main;
        isHorizontal = direction == ScrollDirection.Left || direction == ScrollDirection.Right;
        
        if (isHorizontal)
        {
            screenBounds = cam.orthographicSize * cam.aspect;
        }
        else
        {
            screenBounds = cam.orthographicSize;
        }
        
        SetMovementVector();
        CreateInitialTiles();
    }

    void Update()
    {
        // Move all tiles in the scroll direction
        Vector2 movement = movementVector * (scrollSpeed * Time.deltaTime);
        
        for (int i = 0; i < activeTiles.Length; i++)
        {
            if (activeTiles[i] != null)
            {
                activeTiles[i].transform.Translate(movement);
            }
        }

        // Check if any tiles have moved offscreen and need recycling
        CheckForRecycling();
    }

    void SetMovementVector()
    {
        // Convert scroll direction enum to a movement vector
        if (direction == ScrollDirection.Up)
            movementVector = Vector2.up;
        else if (direction == ScrollDirection.Down)
            movementVector = Vector2.down;
        else if (direction == ScrollDirection.Left)
            movementVector = Vector2.left;
        else
            movementVector = Vector2.right;
    }

    void CreateInitialTiles()
    {
        // Instantiate a test tile to measure its size, then destroy it
        GameObject testTile = Instantiate(tilePrefabs[0]);
        tileSize = GetTileSize(testTile);
        Destroy(testTile);

        // Create 3 tiles: one behind, one visible, one ahead
        activeTiles = new GameObject[3];
        
        for (int i = 0; i < 3; i++)
        {
            activeTiles[i] = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Length)], transform);
            activeTiles[i].transform.position = GetTileOffset(i);
        }
    }

    Vector2 GetTileOffset(int index)
    {
        // Calculate position offset for initial tile placement
        // Index 0 = behind, 1 = center, 2 = ahead (relative to scroll direction)
        Vector2 offset = Vector2.zero;
        int positionIndex = index - 1;
        
        if (direction == ScrollDirection.Left)
            offset.x = positionIndex * tileSize;
        else if (direction == ScrollDirection.Right)
            offset.x = -positionIndex * tileSize;
        else if (direction == ScrollDirection.Up)
            offset.y = positionIndex * tileSize;
        else
            offset.y = -positionIndex * tileSize;

        return offset;
    }

    void CheckForRecycling()
    {
        // Check each tile to see if it has scrolled offscreen
        for (int i = 0; i < activeTiles.Length; i++)
        {
            if (activeTiles[i] != null && IsTileOffscreen(activeTiles[i]))
            {
                RecycleTile(i);
            }
        }
    }

    bool IsTileOffscreen(GameObject tile)
    {
        // Check if tile has moved beyond screen bounds in the scroll direction
        Vector2 pos = tile.transform.position;
        float halfTile = tileSize * 0.5f;
        float threshold = screenBounds + halfTile;

        if (direction == ScrollDirection.Left)
            return pos.x < -threshold;
        else if (direction == ScrollDirection.Right)
            return pos.x > threshold;
        else if (direction == ScrollDirection.Up)
            return pos.y > threshold;
        else
            return pos.y < -threshold;
    }

    void RecycleTile(int tileIndex)
    {
        // Find the "lead" tile - the one farthest in the direction we're scrolling
        GameObject leadTile = activeTiles[0];
        float leadValue = GetRelevantPosition(leadTile);
        
        for (int i = 1; i < activeTiles.Length; i++)
        {
            float currentValue = GetRelevantPosition(activeTiles[i]);
            
            // For left/down: find highest position value (rightmost/topmost)
            // For right/up: find lowest position value (leftmost/bottommost)
            if (direction == ScrollDirection.Left || direction == ScrollDirection.Down)
            {
                if (currentValue > leadValue)
                {
                    leadValue = currentValue;
                    leadTile = activeTiles[i];
                }
            }
            else
            {
                if (currentValue < leadValue)
                {
                    leadValue = currentValue;
                    leadTile = activeTiles[i];
                }
            }
        }

        // Destroy the offscreen tile and create a new one
        Destroy(activeTiles[tileIndex]);
        activeTiles[tileIndex] = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Length)], transform);
        
        // Position new tile one tileSize ahead of the lead tile
        Vector2 newPos = leadTile.transform.position;
        
        if (direction == ScrollDirection.Left)
            newPos.x += tileSize;
        else if (direction == ScrollDirection.Right)
            newPos.x -= tileSize;
        else if (direction == ScrollDirection.Up)
            newPos.y -= tileSize;
        else
            newPos.y += tileSize;
        
        activeTiles[tileIndex].transform.position = newPos;
    }

    float GetRelevantPosition(GameObject tile)
    {
        // Return x position for horizontal scroll, y for vertical
        if (isHorizontal)
        {
            return tile.transform.position.x;
        }
        else
        {
            return tile.transform.position.y;
        }
    }

    float GetTileSize(GameObject tile)
    {
        // Get tile size from SpriteRenderer bounds, or fallback to scale
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (isHorizontal)
            {
                return renderer.bounds.size.x;
            }
            else
            {
                return renderer.bounds.size.y;
            }
        }
        
        if (isHorizontal)
        {
            return tile.transform.localScale.x;
        }
        else
        {
            return tile.transform.localScale.y;
        }
    }
}