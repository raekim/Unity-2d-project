using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class myTileMap : MonoBehaviour
{
    Tilemap myTilemap;
    [SerializeField] TileBase baseTile;
    [SerializeField] int mapWidth, mapHeight;
    [SerializeField] int fillAmount = 40;
    //[SerializeField] int mapSampleInterval;
    //[SerializeField] float PerlinCaveModifier = .5f;
    private int[,] map;

    // Start is called before the first frame update
    void Awake()
    {
        myTilemap = GetComponent<Tilemap>();
    }

    void Start()
    {
        
    }

    private void OnEnable()
    {
        //map = ProceduralTileMap.PerlinNoiseSmooth(map, Random.Range(0f, 1000f), mapSampleInterval);
        map = ProceduralTileMap.GenerateArray(mapWidth, mapHeight, true);
        //map = ProceduralTileMap.RandomWalkTopSmoothed(map, Random.Range(0f, 1000f), 5);
        //map = ProceduralTileMap.PerlinNoiseCave(map, PerlinCaveModifier, true, Random.Range(0f, 1000f));
        map = ProceduralTileMap.GenerateCellularAutomata(map, Random.Range(0f, 1000f), fillAmount, true);
        map = ProceduralTileMap.SmoothMooreCellularAutomata(map, true, 3);
        map = ProceduralTileMap.DeleteSmallCaves(map);
    }

    // Update is called once per frame
    void Update()
    {
        RenderMap(map, myTilemap, baseTile);
        UpdateMap(map, myTilemap);
    }

    private void RenderMap(int[,] map, Tilemap tilemap, TileBase tile)
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if (map[x, y] == 1)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    private void UpdateMap(int[,] map, Tilemap tilemap)
    {
        for (int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if (map[x, y] == 0)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
    }
}
