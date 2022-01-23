using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MOVE,
        MATCH,
        SPAWN
    }

    [SerializeField]
    private GameObject tileContainer;

    [SerializeField]
    private GameObject[] balls;

    [SerializeField]
    private GameObject tilePrefab;

    private GameObject[,] tileArray;

    private GameState currentState;
    public bool canMove;

    private TileType[] randomSpawnQueue;
    private Vector2[] randomSpawnPos;

    private int score;
    private bool gameFinish;

    // Start is called before the first frame update
    void Start()
    {
        int play_area_size = 9;
        float distance_between_tile = 1f;
        tileArray = new GameObject[play_area_size, play_area_size];

        // Instantiate tiles 
        for(int y = 0; y < play_area_size; y++)
        {
            for(int x = 0; x < play_area_size; x++)
            {
                float offset = distance_between_tile * (float)play_area_size / 2;
                Vector3 pos = new Vector3(x * distance_between_tile - 4f, -y * distance_between_tile + 4f);
                GameObject newTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, tileContainer.transform);

                newTile.transform.localPosition = pos;
                tileArray[x, y] = newTile;
                Tile newTileScript = newTile.GetComponent<Tile>();
                newTileScript.Balls = balls;
                newTileScript.Manager = GetComponent<GameManager>();
                newTileScript.coord = new Vector2(x, y);
            }
        }

        // Connecting tiles
        for (int y = 0; y < play_area_size; y++)
        {
            for (int x = 0; x < play_area_size; x++)
            {
                tileArray[x, y].GetComponent<Tile>().ConnectTiles(x, y, tileArray);
            }
        }

        currentState = GameState.MOVE;
        gameFinish = false;

        randomSpawnQueue = new TileType[3];
        randomSpawnPos = new Vector2[3];
        RandomSpawnColor();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(GameUpdate());
    }

    IEnumerator GameUpdate()
    {
        while (!gameFinish)
        {
            switch (currentState)
            {
                case GameState.SPAWN:
                    #region Spawn random balls
                    RandomSpawnPosition();

                    for (int i = 0; i < randomSpawnQueue.Length; i++)
                    {
                        int x = (int)randomSpawnPos[i].x;
                        int y = (int)randomSpawnPos[i].y;

                        tileArray[x, y].GetComponent<Tile>().CurrentTile = randomSpawnQueue[i];
                    }

                    RandomSpawnColor();

                    currentState = GameState.MOVE;
                    canMove = true;
                    break;
                    #endregion

                case GameState.MOVE:
                    #region Let the player move the balls between tiles
                    while(canMove)
                    {
                        yield return null;
                    }
                    currentState = GameState.MATCH;
                    break;
                    #endregion

                case GameState.MATCH:
                    #region Search for matching tiles and increment point based on result
                    for (int y = 0; y < 9; y++)
                    {
                        for (int x = 0; x < 9; x++)
                        {
                            Tile currentTile = tileArray[x, y].GetComponent<Tile>();
                            if (!currentTile.IsCleared && currentTile.CurrentTile != TileType.EMPTY)
                            {
                                currentTile.MatchTile(1);
                            }
                        }
                    }

                    for (int y = 0; y < 9; y++)
                    {
                        for (int x = 0; x < 9; x++)
                        {
                            Tile currentTile = tileArray[x, y].GetComponent<Tile>();

                            // Reset check flag
                            currentTile.IsChecked = false;
                            
                            // Destroy balls and increment score
                            if (currentTile.IsCleared)
                            {
                                score++;
                                currentTile.CurrentTile = TileType.EMPTY;
                                currentTile.IsCleared = false;
                            }
                        }
                    }

                    currentState = GameState.SPAWN;
                    //gameFinish = !gameFinish;
                    break;
                    #endregion
            }

            // End game when all tiles are filled
            int filled_tile_count = 0;

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Tile currentTile = tileArray[x, y].GetComponent<Tile>();
                    if (currentTile.CurrentTile != TileType.EMPTY)
                    {
                        filled_tile_count++;
                    }
                }
            }

            if (filled_tile_count >= 81)
                gameFinish = true;
        }
    }    

    // Randomize colors for next 3 balls
    private void RandomSpawnColor()
    {
        for (int i = 0; i < randomSpawnQueue.Length; i++)
        {
            int rand = Random.Range(1, 4);

            randomSpawnQueue[i] = (TileType)rand;
        }
    }

    // Randomize positions for next 3 balls
    private void RandomSpawnPosition()
    {
        for(int i = 0; i < randomSpawnQueue.Length; i++)
        {
            randomSpawnPos[i] = Vector2.one * -1;

            // Getting random position for random spawn
            while (randomSpawnPos[i].x < 0 || randomSpawnPos[i].y < 0)
            {
                int randomX = Random.Range(0, 8);
                int randomY = Random.Range(0, 8);

                if (tileArray[randomX, randomY].GetComponent<Tile>().CurrentTile == TileType.EMPTY)
                {
                    randomSpawnPos[i] = new Vector2(randomX, randomY);
                }
            }
        }
    }
}
