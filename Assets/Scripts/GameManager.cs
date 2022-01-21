using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject tileContainer;

    [SerializeField]
    private GameObject[] balls;

    [SerializeField]
    private GameObject tilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        float distance_between_tile = 1.25f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
