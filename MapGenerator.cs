using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        for (int i = 0; i < 3; i++)
        {
            RemoveUnnecessaryPoints(); // Remove isolated points
        }

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
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
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
    
    void RemoveUnnecessaryPoints()
    {
        int n = 3;
        int[,] newMap = (int[,])map.Clone(); // Create a copy to store the updated map

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int currentColor = map[x, y];

                // Skip if the current point is already white
                if (currentColor == 0)
                    continue;

                // Check for N consecutive neighbors in each direction
                bool hasConsecutiveNeighbors = HasNConsecutiveNeighbors(x, y, currentColor,n);

                // If no N consecutive neighbors, set the point to white in the new map
                if (!hasConsecutiveNeighbors)
                {
                    newMap[x, y] = 0;
                }
            }
        }

        map = newMap; // Update the map with the new map
    }

    bool HasNConsecutiveNeighbors(int x, int y, int color, int n)
    {
        // Check right (x+1 to x+N)
        if (x + n < width)
        {
            bool allMatch = true;
            for (int i = 1; i <= n; i++)
            {
                if (map[x + i, y] != color)
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch) return true;
        }

        // Check left (x-1 to x-N)
        if (x - n >= 0)
        {
            bool allMatch = true;
            for (int i = 1; i <= n; i++)
            {
                if (map[x - i, y] != color)
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch) return true;
        }

        // Check up (y+1 to y+N)
        if (y + n < height)
        {
            bool allMatch = true;
            for (int i = 1; i <= n; i++)
            {
                if (map[x, y + i] != color)
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch) return true;
        }

        // Check down (y-1 to y-N)
        if (y - n >= 0)
        {
            bool allMatch = true;
            for (int i = 1; i <= n; i++)
            {
                if (map[x, y - i] != color)
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch) return true;
        }

        // No N consecutive neighbors found
        return false;
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
