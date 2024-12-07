using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using System.Linq;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public int width;
    public int height;
    
    public string seed;
    public bool useRandomSeed;
    
    [Range(0, 100)] public int randomFillPercent;
    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        for (int i = 0; i < 1000; i++)
        {
            SmoothMap();
        }

        ProcessMap();
        
        /////////////////// Generating map border ///////////////////
        int borderSize = 1;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }
        
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap,1);
        
        // CreateVisualMap();
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        wallRegions.AddRange(GetRegions(2));
        int wallThresholdSize = 30;
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        
        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 5;
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = map[tile.tileX + 1, tile.tileY];
                }
            }
        }
        
        
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width,height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);
                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (x == tile.tileX || y == tile.tileY))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                        
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 0; // Borders are always walls or boundaries
                }
                else
                {
                    // Assign zones randomly with weights
                    int randomValue = pseudoRandom.Next(0, 100);
                    if (randomValue < randomFillPercent)
                    {
                        map[x, y] = 1; // Residential
                    }
                    else if (randomValue < randomFillPercent + 15) // 15% for industrial
                    {
                        map[x, y] = 2; // Industrial
                    }
                    else
                    {
                        map[x, y] = 0; // white spaces
                    }
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Count all neighbors
                int residentialCount = GetSurroundingTileCount(x, y, 1);
                int industrialCount = GetSurroundingTileCount(x, y, 2);
                int emptyCount = GetSurroundingTileCount(x, y, 0);
    
                // Total valid neighbors (excluding boundaries)
                int totalNeighbors = residentialCount + industrialCount + emptyCount;
    
                if (totalNeighbors > 0) // Avoid division by zero
                {
                    // Calculate proportions
                    float residentialRatio = (float)residentialCount / totalNeighbors;
                    float industrialRatio = (float)industrialCount / totalNeighbors;
                    float emptyRatio = (float)emptyCount / totalNeighbors;
                    if (residentialRatio + industrialRatio > emptyRatio)
                    {
                        // Assign type based on dominant proportion
                        if (residentialRatio > industrialRatio)
                        {
                            map[x, y] = 1; // Residential
                        }
                        else if (industrialRatio > residentialRatio)
                        {
                            map[x, y] = 2; // Industrial
                        }
                    }
                }
            }
        }
    }
    
    
    int GetSurroundingTileCount(int gridX, int gridY, int tileType)
    {
        int count = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        if (map[neighbourX, neighbourY] == tileType)
                        {
                            count++;
                        }
                    }
                }
            }
        }
        return count;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }
    
    
    void CreateVisualMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
                GameObject obj = null;

                if (map[x, y] == 1) // Residential
                {
                    obj = CreateBuilding(pos, Color.red, "Residential");
                }
                else if (map[x, y] == 2) // Industrial
                {
                    obj = CreateBuilding(pos, Color.gray, "Industrial");
                }
                else if (map[x, y] == 0) // Empty
                {
                    obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    obj.transform.position = pos;
                    obj.GetComponent<Renderer>().material.color = Color.white;
                }

                if (obj != null)
                {
                    obj.transform.parent = this.transform; // Keep hierarchy clean
                }
            }
        }
    }

    GameObject CreateBuilding(Vector3 position, Color color, string tag)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.transform.position = position;
        building.transform.localScale = new Vector3(1, UnityEngine.Random.Range(1, 3), 1); // Random height
        building.GetComponent<Renderer>().material.color = color;
        building.tag = tag; // Assign tag for differentiation
        return building;
    }

}
