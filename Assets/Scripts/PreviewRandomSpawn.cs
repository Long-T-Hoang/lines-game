using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewRandomSpawn : MonoBehaviour
{
    [SerializeField]
    private Sprite[] tilePrefab;
    public TileType tileType;
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        image.sprite = tilePrefab[(int)tileType];
    }
}
