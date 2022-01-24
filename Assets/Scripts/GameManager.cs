using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MOVE,
        MATCH,
        SPAWN,
        NONE
    }

    #region Fields
    private const int play_area_size = 9;
    [SerializeField]
    private GameObject tileContainer;

    [SerializeField]
    private GameObject[] balls;

    [SerializeField]
    private GameObject tilePrefab;

    private GameObject[,] tileArray;

    private GameState lastState;
    private GameState currentState;
    public bool canMove;

    private TileType[] randomSpawnQueue;
    private Vector2[] randomSpawnPos;

    private int score;
    private int currentTime;
    private bool gameFinish;

    // GUI elements
    [Header("GUI elements")]
    [SerializeField]
    private GameObject scoreText;
    private TextMeshProUGUI scoreTextMesh;
    [SerializeField]
    private GameObject timeText;
    private TextMeshProUGUI timeTextMesh;
    [SerializeField]
    private GameObject pauseResumeButton;
    private TextMeshProUGUI pauseResumeTextMesh;
    [SerializeField]
    private GameObject startScene;

    // Preview for random spawn
    [SerializeField]
    private GameObject[] randomSpawnPreviews;

    // Save/load functions
    string saveFile;
    #endregion

    #region Functions
    private void Awake()
    {
        saveFile = Application.persistentDataPath + "/gamedata.json";
    }

    // Start is called before the first frame update
    void Start()
    {
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

        score = 0;
        currentTime = 0;

        Time.timeScale = 0.0f;
        currentState = GameState.NONE;
        gameFinish = false;

        randomSpawnQueue = new TileType[3];
        randomSpawnPos = new Vector2[3];
        RandomSpawnColor();

        scoreTextMesh = scoreText.GetComponent<TextMeshProUGUI>();
        scoreTextMesh.SetText(score.ToString());
        timeTextMesh = timeText.GetComponent<TextMeshProUGUI>();
        pauseResumeTextMesh = pauseResumeButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
        StartCoroutine(GameUpdate());
    }

    // Format and update time GUI element
    private void UpdateTime()
    {
        int time = (int)Time.time + currentTime;
        int minute = time / 60;
        int second = time % 60;
        string timeString = string.Format("{0:00}:{1:00}", minute, second);
        timeTextMesh.SetText(timeString);
    }

    // Game logic
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

                        tileArray[x, y].GetComponent<Tile>().TileType = randomSpawnQueue[i];
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
                    for (int y = 0; y < play_area_size; y++)
                    {
                        for (int x = 0; x < play_area_size; x++)
                        {
                            Tile currentTile = tileArray[x, y].GetComponent<Tile>();
                            if (!currentTile.IsCleared && currentTile.TileType != TileType.EMPTY)
                            {
                                currentTile.MatchTile(1);
                            }
                        }
                    }

                    for (int y = 0; y < play_area_size; y++)
                    {
                        for (int x = 0; x < play_area_size; x++)
                        {
                            Tile currentTile = tileArray[x, y].GetComponent<Tile>();

                            // Reset check flag
                            currentTile.IsChecked = false;
                            
                            // Destroy balls and increment score
                            if (currentTile.IsCleared)
                            {
                                score++;
                                currentTile.TileType = TileType.EMPTY;
                                currentTile.IsCleared = false;
                            }
                        }
                    }

                    scoreTextMesh.SetText(score.ToString());

                    currentState = GameState.SPAWN;
                    //gameFinish = !gameFinish;
                    break;
                #endregion

                default:
                    yield return null;
                    break;
            }

            // End game when all tiles are filled
            int filled_tile_count = 0;

            for (int y = 0; y < play_area_size; y++)
            {
                for (int x = 0; x < play_area_size; x++)
                {
                    Tile currentTile = tileArray[x, y].GetComponent<Tile>();
                    if (currentTile.TileType != TileType.EMPTY)
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

            randomSpawnPreviews[i].GetComponent<PreviewRandomSpawn>().tileType = (TileType)rand;
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
                int randomX = Random.Range(0, play_area_size);
                int randomY = Random.Range(0, play_area_size);

                if (tileArray[randomX, randomY].GetComponent<Tile>().TileType == TileType.EMPTY)
                {
                    randomSpawnPos[i] = new Vector2(randomX, randomY);
                }
            }
        }
    }

    // Save current game data
    public void SaveState()
    {
        GameData data = new GameData();
        data.time = (int)Time.time;
        data.score = score;
        data.state = currentState.ToString();
        data.tileArray = new List<string>();

        for(int y = 0; y < play_area_size; y++)
        {
            for(int x = 0; x < play_area_size; x++)
            {
                data.tileArray.Add(tileArray[x,y].GetComponent<Tile>().TileType.ToString());
            }
        }

        string jsonString = JsonUtility.ToJson(data);
        File.WriteAllText(saveFile, jsonString);
    }

    public void LoadState()
    {
        if (File.Exists(saveFile))
        {
            // Read the entire file and save its contents.
            string fileContents = File.ReadAllText(saveFile);

            // Deserialize the JSON data 
            //  into a pattern matching the GameData class.
            GameData data = JsonUtility.FromJson<GameData>(fileContents);

            score = data.score;
            currentTime = data.time;
            currentState = (GameState)System.Enum.Parse(typeof(GameState), data.state);

            for(int i = 0; i < data.tileArray.Count; i++)
            {
                int y = (int)(i / play_area_size);
                int x = (int)(i % play_area_size);

                tileArray[x, y].GetComponent<Tile>().TileType = (TileType)System.Enum.Parse(typeof(TileType), data.tileArray[i]);
            }
        }

        Time.timeScale = 1.0f;
        startScene.SetActive(false);
    }

    #region Button handlers
    public void GameStateButtonHandler()
    {
        if (currentState == GameState.NONE)
        {
            pauseResumeTextMesh.SetText("Pause");
            ContinueGame();
        }
        else
        {
            pauseResumeTextMesh.SetText("Resume");
            PauseGame();
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1.0f;
        currentState = GameState.SPAWN;
        startScene.SetActive(false);
    }

    private void PauseGame()
    {
        Time.timeScale = 0.0f;
        lastState = currentState;
        currentState = GameState.NONE;
    }

    private void ContinueGame()
    {
        Time.timeScale = 1.0f;
        currentState = lastState;
    }
    #endregion

    #endregion

}
