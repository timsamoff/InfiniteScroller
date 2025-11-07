using UnityEngine;

public class InfiniteScrollerSIMPLE : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float scrollSpeed = 5f;

    private GameObject[] activeTiles;
    private float tileWidth;
    private float screenBounds;

    void Start()
    {
        // Calculate screen boundaries
        Camera cam = Camera.main;
        screenBounds = cam.orthographicSize * cam.aspect;
        
        // Create first tile and get its size after instantiation
        activeTiles = new GameObject[3];
        activeTiles[0] = Instantiate(tilePrefab, new Vector2(0, 0), Quaternion.identity);
        
        // Get the width from the instantiated tile
        tileWidth = activeTiles[0].GetComponent<SpriteRenderer>().bounds.size.x;
        
        // Create remaining tiles immediately to the right
        activeTiles[1] = Instantiate(tilePrefab, new Vector2(tileWidth, 0), Quaternion.identity);
        activeTiles[2] = Instantiate(tilePrefab, new Vector2(tileWidth * 2, 0), Quaternion.identity);
    }

    void Update()
    {
        // Move all tiles left
        for (int i = 0; i < activeTiles.Length; i++)
        {
            activeTiles[i].transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);
        }
        
        // Check each tile to see if it needs recycling
        for (int i = 0; i < activeTiles.Length; i++)
        {
            // If any tile is completely off-screen to the left
            if (activeTiles[i].transform.position.x < -screenBounds - tileWidth)
            {
                // Find which tile is farthest to the right
                GameObject rightmostTile = activeTiles[0];
                for (int j = 1; j < activeTiles.Length; j++)
                {
                    if (activeTiles[j].transform.position.x > rightmostTile.transform.position.x)
                    {
                        rightmostTile = activeTiles[j];
                    }
                }
                
                // Move the off-screen tile to the right of the rightmost tile
                float newX = rightmostTile.transform.position.x + tileWidth;
                activeTiles[i].transform.position = new Vector2(newX, 0);
            }
        }
    }
}