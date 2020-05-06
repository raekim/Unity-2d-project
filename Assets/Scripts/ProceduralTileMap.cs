using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralTileMap : MonoBehaviour
{   
    public static int[,] GenerateArray(int width, int height, bool empty)
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

    // Top Layer Generation Algorithms
    public static int[,] PerlinNoise(int[,] map, float seed)
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

    public static int[,] PerlinNoiseSmooth(int[,] map, float seed, int interval)
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

    public static int[,] RandomWalkTop(int[,] map, float seed)
    {
        System.Random rand = new System.Random(seed.GetHashCode());

        int lastHeight = Random.Range(0, map.GetUpperBound(1));

        for(int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            int nextMove = rand.Next(2);

            if(nextMove == 0 && lastHeight > 2)
            {
                lastHeight--;
            }
            else if(nextMove == 1 && lastHeight < map.GetUpperBound(1))
            {
                lastHeight++;
            }

            for(int y = lastHeight; y >= 0; --y)
            {
                map[x, y] = 1;
            }
        }

        return map;
    }

    public static int[,] RandomWalkTopSmoothed(int[,] map, float seed, int minSectionWidth)
    {
        System.Random rand = new System.Random(seed.GetHashCode());

        int lastHeight = Random.Range(0, map.GetUpperBound(1));

        int nextMove;
        int sectionWidth = 0;

        for (int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            if (sectionWidth > minSectionWidth)
            {
                nextMove = rand.Next(2);
                if (nextMove == 0 && lastHeight > 2)
                {
                    lastHeight--;
                }
                else if (nextMove == 1 && lastHeight < map.GetUpperBound(1))
                {
                    lastHeight++;
                }
                sectionWidth = 0;
            }

            sectionWidth++;

            for (int y = lastHeight; y >= 0; --y)
            {
                map[x, y] = 1;
            }
        }

        return map;
    }

    // Cave Generation ALgorithms
    public static int[,] PerlinNoiseCave(int[,] map, float modifier, bool edgedAreWalls, float offSet)
    {
        // modifier는 Perlin noise 평면의 줌인/줌아웃 효과를 준다. (커질 수록 줌 아웃)
        // offset은 Perlin noise 평면상의 좌표의 offset으로 랜덤 효과를 준다.
        int newPoint;
        for(int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if(edgedAreWalls && x == 0 || y == 0 || x == map.GetUpperBound(0) || y == map.GetUpperBound(1))
                {
                    map[x, y] = 1;  // Keep the deges as walls
                }
                else
                {
                    newPoint = Mathf.RoundToInt(Mathf.PerlinNoise(x * modifier + offSet, y * modifier + offSet));
                    map[x, y] = Mathf.Max(0, newPoint); // Perlin noise could return value slightly less than 0
                }
            }
        }

        return map;
    }

    public static int[,] GenerateCellularAutomata(int[,] map, float seed, int fillPercent, bool edgesAreWalls)
    {
        System.Random rand = new System.Random(seed.GetHashCode());

        for (int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if (edgesAreWalls && (x == 0 || y == 0 || x == map.GetUpperBound(0) || y == map.GetUpperBound(1)))
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (rand.Next(0, 100) < fillPercent) ? 1 : 0;
                }
            }       
        }
        return map;
    }

    private static int GetMooreSurroundingTiles(int[,] map, int x, int y, bool edgesAreWalls)
    {
        int tileCount = 0;

        for (int neighborX = x - 1; neighborX <= x + 1; ++neighborX)
        {
            for (int neighborY = y - 1; neighborY <= y + 1; ++neighborY)
            {
                if(neighborX >= 0 && neighborX <= map.GetUpperBound(0) &&
                    neighborY >= 0 && neighborY <= map.GetUpperBound(1))
                {
                    tileCount += map[neighborX, neighborY];
                }
            }
        }

        // DO NOT COUNT THE (x, y) TILE
        tileCount -= map[x, y];
        return tileCount;
    }

    public static int[,] SmoothMooreCellularAutomata(int[,] map, bool edgesAreWalls, int smoothCount)
    {
        for(int i=0; i < smoothCount; ++i)
        {
            for (int x = 0; x <= map.GetUpperBound(0); ++x)
            {
                for (int y = 0; y <= map.GetUpperBound(1); ++y)
                {
                    if (edgesAreWalls && (x == 0 || x == map.GetUpperBound(0) || y == 0 || y == map.GetUpperBound(1)))
                    {
                        // EDGE
                        map[x, y] = 1;
                    }
                    else
                    {
                        int surroundingTiles = GetMooreSurroundingTiles(map, x, y, edgesAreWalls);
                        if(surroundingTiles > 4)
                        {
                            map[x, y] = 1;
                        }
                        else if (surroundingTiles < 4)
                        {
                            map[x, y] = 0;
                        }
                    }
                }
            }
        }

        return map;
    }

    // x,y 좌표에서 시작해 동굴을 num으로 마크하고, 넓이를 반환
    private static int MarkCave(int[,]map, int x, int y, int num)
    {
        int caveArea = 0;

        Stack<KeyValuePair<int, int>> st = new Stack<KeyValuePair<int, int>>();
        st.Push(new KeyValuePair<int, int>(x, y));

        while(st.Count > 0)
        {
            caveArea++;

            // 스택 top에서 pop하여 마크
            var pos = st.Pop();
            map[pos.Key, pos.Value] = num;

            // 주변 4방향 타일 스택에 넣기
            int[] dx = new int[4] { -1, 0, 1, 0 };
            int[] dy = new int[4] { 0, 1, 0, -1 };
            for (int i = 0; i < 4; ++i)
            {
                var newPos = new KeyValuePair<int, int>(pos.Key + dx[i], pos.Value + dy[i]);

                if (newPos.Key >= 0 && newPos.Key <= map.GetUpperBound(0) &&
                    newPos.Value >= 0 && newPos.Value <= map.GetUpperBound(1) &&
                    map[newPos.Key, newPos.Value] == 0)
                {
                    st.Push(newPos);
                }
            }
        }

        return caveArea;
    }

    public static int[,] DeleteSmallCaves(int[,] map)
    {
        List<KeyValuePair<int, int>> caves = new List<KeyValuePair<int, int>>();

        // 키값 2로 시작하기 위한 더미 데이터
        caves.Add(new KeyValuePair<int, int>(0, 0));
        caves.Add(new KeyValuePair<int, int>(0, 0)); 

        // caves에 <키값, 넓이> 페어로 동굴들 저장
        for (int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if(map[x,y] == 0)
                {
                    int key = caves.Count;
                    int area = MarkCave(map, x, y, key);
                    caves.Add(new KeyValuePair<int, int>(key, area));
                }
            }
        }
        
        int largestCaveKey = 0;

        for(int i=1; i<caves.Count; ++i)
        {
            // 제일 큰 동굴의 key를 구한다
            if(caves[largestCaveKey].Value < caves[i].Value)
            {
                largestCaveKey = i;
            }
        }

        // 제일 큰 동굴을 제외한 나머지 동굴 메꾸기
        for (int x = 0; x <= map.GetUpperBound(0); ++x)
        {
            for (int y = 0; y <= map.GetUpperBound(1); ++y)
            {
                if (map[x, y] != 1)
                {
                    map[x, y] = (map[x, y] == largestCaveKey ? 0 : 1);
                }
            }
        }

        return map;
    }
}
