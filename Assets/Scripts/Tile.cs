using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    EMPTY,
    RED,
    GREEN,
    BLUE
}

public class Tile : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Sprite[] tileSprites;
    [SerializeField]
    private TileType tileType;

    // Fields for dragging functions
    private GameObject[] balls;
    private Vector3[] ballOriginalPos;
    private GameObject currentBall;
    private Vector3 currentBallPos;
    private SpriteRenderer sr;
    private bool dragged;

    private GameObject[] surroundingTiles;
    private bool isChecked; // Tag for matching tiles
    private bool isCleared; // Tag for destroying balls and add score

    private GameManager manager;
    public Vector2 coord;
    #endregion

    public GameObject[] Balls
    {
        set
        {
            if(balls == null)
                balls = new GameObject[value.Length];

            for(int i = 0; i < value.Length; i++)
            {
                balls[i] = value[i];
            }
        }
    }

    public TileType TileType
    {
        get { return tileType; }
        set { tileType = value; }
    }

    public bool IsChecked
    {
        get { return isChecked; }
        set { isChecked = value; }
    }

    public bool IsCleared
    {
        get { return isCleared; }
        set { isCleared = value; }
    }

    public GameManager Manager
    {
        set { manager = value; }
        get { return manager; }
    }

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        dragged = false;

        ballOriginalPos = new Vector3[balls.Length];

        for(int i = 0; i < balls.Length; i++)
        {
            ballOriginalPos[i] = balls[i].transform.position;
        }

        isChecked = false;
        isCleared = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dragged && tileType != TileType.EMPTY)
        {
            currentBall = balls[(int)tileType - 1];
            currentBallPos = currentBall.transform.position;
        }
        UpdateVisual();
    }

    void UpdateVisual()
    {
        sr.sprite = tileSprites[(int)tileType];
    }

    public void ConnectTiles(int x, int y, GameObject[,] tileArray)
    {
        int arrayMin = 0;
        int arrayMax = 8;

        surroundingTiles = new GameObject[8];

        for (int i = 0; i < surroundingTiles.Length; i++)
            surroundingTiles[i] = null;

        if (y > arrayMin)
            surroundingTiles[0] = tileArray[x, y - 1];

        if (y > arrayMin && x < arrayMax)
            surroundingTiles[1] = tileArray[x + 1, y - 1];

        if (x < arrayMax)
            surroundingTiles[2] = tileArray[x + 1, y];

        if (y < arrayMax && x < arrayMax)
            surroundingTiles[3] = tileArray[x + 1, y + 1];

        if (y < arrayMax)
            surroundingTiles[4] = tileArray[x, y + 1];

        if (y < arrayMax && x > arrayMin)
            surroundingTiles[5] = tileArray[x - 1, y + 1];

        if (x > arrayMin)
            surroundingTiles[6] = tileArray[x - 1, y];

        if (x > arrayMin && y > arrayMin)
            surroundingTiles[7] = tileArray[x - 1, y - 1];
    }

    public bool MatchTile(int count, int direction = -1)
    {
        bool isMatch = false;
        bool isComplete = false;

        // Continue from the main direction
        if(direction > 0 && surroundingTiles[direction] != null)
        {
            Tile nextTile = surroundingTiles[direction].GetComponent<Tile>();

            if (nextTile.TileType == tileType)
            {
                isComplete = nextTile.MatchTile(count + 1, direction);
            }
            else if(count >= 5)
            {
                isCleared = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        // Check surrounding tiles
        if (direction < 0)
        {
            for (int i = 0; i < surroundingTiles.Length; i++)
            {
                GameObject nextTile = null;
                Tile nextTileScript = null;

                if (surroundingTiles[i] != null && surroundingTiles[i].GetComponent<Tile>().TileType != TileType.EMPTY)
                {
                    nextTile = surroundingTiles[i];
                    nextTileScript = nextTile.GetComponent<Tile>();
                }
                else continue;

                if (nextTileScript.TileType == tileType)
                {
                    isMatch = true;
                    if (direction < 0)
                        isComplete |= nextTileScript.GetComponent<Tile>().MatchTile(count + 1, i);
                }
            }
        }

        if(!isMatch && count >= 5)
        {
            isCleared = true;
            return true;
        }

        if(!isCleared)
            isCleared = isComplete;

        return isComplete;
    }

    #region Mouse drag functions for dragging balls

    private void OnMouseDown()
    {
        if (manager.canMove && tileType != TileType.EMPTY)
        {
            dragged = true;
            sr.sprite = tileSprites[0];
        }
    }

    private void OnMouseDrag()
    {
        if (tileType != TileType.EMPTY && manager.canMove)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
            curPosition.z = -5;
            currentBall.transform.position = curPosition;
        }
    }

    private void OnMouseUp()
    {
        if (manager.canMove && tileType != TileType.EMPTY)
        {
            currentBall.transform.position = currentBallPos;
            dragged = false;

            RaycastHit2D hit;
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
            curPosition.z = 0;

            // CircleCast to get the tile 
            hit = Physics2D.CircleCast(curPosition, 0.1f, Vector2.zero);
            Tile newTileScript = hit.collider.gameObject.GetComponent<Tile>();

            // Check if the tile is in the 4 directions of the original tile and is empty
            if ((coord.x == newTileScript.coord.x || coord.y == newTileScript.coord.y) && newTileScript.TileType == TileType.EMPTY)
            {
                newTileScript.TileType = tileType;
                tileType = TileType.EMPTY;

                manager.canMove = false;
            }
        }
    }

    #endregion
}
