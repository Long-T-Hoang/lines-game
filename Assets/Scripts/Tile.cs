using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        EMPTY,
        RED,
        GREEN,
        BLUE
    }

    [SerializeField]
    private Sprite[] tileSprites;
    [SerializeField]
    private TileType currentTile;

    private GameObject[] balls;
    private Vector3[] ballOriginalPos;
    private GameObject currentBall;
    private Vector3 currentBallPos;

    private SpriteRenderer sr;
    private bool dragged; 
    
    public GameObject[] Balls
    {
        set
        {
            for(int i = 0; i < value.Length; i++)
            {
                balls[i] = value[i];
            }
        }
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!dragged)
        {
            currentBall = balls[(int)currentTile - 1];
            currentBallPos = currentBall.transform.position;
            UpdateVisual();
        }
    }

    void UpdateVisual()
    {
        sr.sprite = tileSprites[(int)currentTile];
    }



    // Mouse drag functions for dragging balls
    private void OnMouseDown()
    {
        dragged = true;
        sr.sprite = tileSprites[0];
    }

    private void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        curPosition.z = -5;
        currentBall.transform.position = curPosition;
    }

    private void OnMouseUp()
    {
        currentBall.transform.position = currentBallPos;
        dragged = false;
    }
}
