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
        Generator();
    }

    void Generator()
    {
        map = new int[width, height];
        RandomFillMap();
        for (int i = 0; i < 1000; i++)
        {
            SmoothMap();
        }
        
        CreateVisualMap();
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
                    obj = CreateBuilding(pos, Color.green, "Residential");
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
