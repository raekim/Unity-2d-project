using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralTileMap : MonoBehaviour
{
    Tilemap myTilemap;
    [SerializeField] TileBase baseTile;
    [SerializeField] int mapWidth, mapHeight;
    [SerializeField] int mapSampleInterval;
    private int[,] map;

    // Start is called before the first frame update
    void Awake()
    {
        myTilemap = GetComponent<Tilemap>();
        map = GenerateArray(mapWidth, mapHeight, true);
    }

    private void Start()
    {
        //map = PerlinNoise(map, Random.Range(0f, 1000f));
        map = PerlinNoiseSmooth(map, Random.Range(0f, 1000f), mapSampleInterval);
    }

    // Update is called once per frame
    void Update()
    {
        RenderMap(map, myTilemap, baseTile);
    }

    private int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for(int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                map[x, y] = (empty ? 0 : 1);
            }
        }
        return map;
    }

    private void RenderMap(int[,] map, Tilemap tilemap, TileBase tile)
    {
        tilemap.ClearAllTiles();
        for(int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if(map[x,y] == 1)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    private void UpdateMap(int[,] map, Tilemap tilemap)
    {
        for(int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if(map[x,y] == 0)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
    }

    private int[,] PerlinNoise(int[,] map, float seed)
    {
        int newPoint;
        float reduction = .5f;
        for(int x=0; x<= map.GetUpperBound(0); ++x)
        {
            newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, seed) - reduction) * map.GetUpperBound(1));
            newPoint += (map.GetUpperBound(1) / 2);
            for(int y = newPoint; y >= 0; --y)
            {
                map[x, y] = 1;
            }
        }
        return map;
    }

    private int[,] PerlinNoiseSmooth(int[,] map, float seed, int interval)
    {
        if(interval > 1)
        {
            int newPoint, points;
            float reduction = .5f;

            List<int> noiseX = new List<int>();
            List<int> noiseY = new List<int>();

            // 인터벌마다 newPoint 얻어오기
            for (int x = 0; x <= map.GetUpperBound(0); x = Mathf.Min(x + interval, map.GetUpperBound(0)))
            {
                newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, seed) - reduction) * map.GetUpperBound(1));
                newPoint += (map.GetUpperBound(1) / 2) + 1;
                noiseX.Add(x);
                noiseY.Add(newPoint);

                if (x == map.GetUpperBound(0)) break;
            }

            points = noiseY.Count;
            //Debug.Log("points : " + points);

            for(int i = 1; i< points; ++i)
            {
                Vector2Int currentPos = new Vector2Int(noiseX[i], noiseY[i]);
                Vector2Int lastPos = new Vector2Int(noiseX[i - 1], noiseY[i - 1]);

                Vector2 diff = currentPos - lastPos;
                float heightChange = diff.y / interval;
                float currHeight = lastPos.y;

                // interval 양 끝의 x지점들의 타일 높이가 부드럽게 변화하도록 한다
                for(int x = lastPos.x; x <= currentPos.x; ++x)
                {
                    for(int y = Mathf.FloorToInt(currHeight); y >= 0; --y)
                    {
                        map[x, y] = 1;
                    }
                    currHeight += heightChange;
                }
            }
        }
        else
        {
            map = PerlinNoise(map, seed);
        }

        return map;
    }
}
