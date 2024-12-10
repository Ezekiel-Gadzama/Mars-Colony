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
    float[,] heightMap;
    
    public string seed;
    public bool useRandomSeed;
    
    [Range(0, 100)] public int randomFillPercent;
    int[,] map;

    void Start()
    {
        heightMap = new float[width, height];
        
        // Perline noise for testing
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * 10.0f;
                float yCoord = (float)y / height * 10.0f;

                // Generate Perlin noise value
                heightMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        //
        
        
        
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

        List<Coord> wallsAroundRooms = GetCoordinatesWithZeroNeighbor();
        CreateVisualMap(wallsAroundRooms);
    }
    
    List<Coord> GetCoordinatesWithZeroNeighbor()
    {
        List<Coord> result = new List<Coord>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Check if the current tile is zero
                if (map[x, y] == 0)
                {
                    // Check the neighbors of the current tile
                    bool hasNonZeroNeighbor = false;

                    // Iterate over the 8 possible neighbors (left, right, up, down, and diagonals)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            int neighborX = x + dx;
                            int neighborY = y + dy;

                            // Ensure the neighbor is within map bounds
                            if (IsInMapRange(neighborX, neighborY) && map[neighborX, neighborY] != 0)
                            {
                                hasNonZeroNeighbor = true;
                                break;
                            }
                        }

                        if (hasNonZeroNeighbor)
                            break;
                    }

                    // If the tile has at least one neighbor with a value of zero, add it to the result list
                    if (hasNonZeroNeighbor)
                    {
                        result.Add(new Coord(x, y));
                    }
                }
            }
        }

        return result;
    }

    
    
    ////////////////////////////////////////////////  Connect rooms code below ////////////////////////////////////////////////
    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(0);
        int wallThresholdSize = 50;
        
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    // Check and update the map with the neighboring tile value.
                    map[tile.tileX, tile.tileY] = GetNeighborTileValue(tile);
                }
            }
        }
        
        List<List<Coord>> roomRegions = GetRegions(1);
        roomRegions.AddRange(GetRegions(2));
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    // Check and update the map with the neighboring tile value.
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        Debug.Log("surviving rooms " + survivingRooms.Count);
        ConnectClosestRooms(survivingRooms);
    }

    int GetNeighborTileValue(Coord tile)
    {
        // Check if the right neighbor is within range
        if (IsInMapRange(tile.tileX + 1, tile.tileY)) 
        {
            return map[tile.tileX + 1, tile.tileY];
        }
        // If not, check the left neighbor
        else if (IsInMapRange(tile.tileX - 1, tile.tileY)) 
        {
            return map[tile.tileX - 1, tile.tileY];
        }
        // If not, check the bottom neighbor
        else if (IsInMapRange(tile.tileX, tile.tileY + 1)) 
        {
            return map[tile.tileX, tile.tileY + 1];
        }
        // If not, check the top neighbor
        else if (IsInMapRange(tile.tileX, tile.tileY - 1)) 
        {
            return map[tile.tileX, tile.tileY - 1];
        }
        
        // Return default color if no valid neighbor is found
        return map[tile.tileX, tile.tileY];
    }

    void ConnectClosestRooms(List<Room> allRooms,bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomsListA = new List<Room>();
        List<Room> roomListB = new List<Room>();
        List<Coord> path = null;

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomsListA.Add(room);
                }
            }
        }
        else
        {
            roomsListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = int.MaxValue;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = null;
        Room bestRoomB = null;
        
        foreach (Room roomA in roomsListA)
        {
            if (!forceAccessibilityFromMainRoom && roomA.connectedRooms.Count > 0)
                continue;
            path = null;
            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                    continue;
                
                int numberToDivide = 1; // Delete
                
                foreach (Coord tileA in roomA.edgeTiles.Where((_, index) => index % (roomA.edgeTiles.Count / numberToDivide) == 0))
                {
                    bool shouldBreak = false;  // Delete
                    
                    
                    foreach (Coord tileB in roomB.edgeTiles.Where((_, index) => index % (roomB.edgeTiles.Count / numberToDivide) == 0))
                    {
                        Debug.Log("Found me me me me me");
                        path = AStarPathfinding(tileA, tileB);
                        if (path == null)
                        {
                            print("Path is null.");
                        }
                        else
                        {
                            print("Path is not null. Path length: " + path.Count);
                        }
                        
                        if (path != null && path.Count < bestDistance)
                        {
                            Debug.Log("Found path from (" + tileA.tileX + ", " + tileA.tileY +") to path (" + tileB.tileX + ", " + tileB.tileY + ") with path length: " + path.Count);
                            bestDistance = path.Count;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                            
                            //////
                            shouldBreak = true; // Delete
                            if (shouldBreak)
                            {
                                break;
                            }
                            
                            ////////
                        }
                    }
                    
                    //////
                    if (shouldBreak)
                    {
                        break;
                    }
                            
                    ////////
                }
            }

            if (path != null && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB,path);
            }
        }

        if (path != null && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB,path);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }
    
    
    
    List<Coord> AStarPathfinding(Coord start, Coord goal)
    {
        // Define the open and closed lists
        List<Node> openList = new List<Node>();
        HashSet<Coord> closedList = new HashSet<Coord>();

        // Initialize start node
        Node startNode = new Node(start, null, 0, GetDistance(start, goal));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Get the node with the lowest fCost (gCost + hCost)
            Node currentNode = openList[0];
            foreach (Node node in openList)
            {
                if (node.fCost < currentNode.fCost || 
                   (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost))
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.coord);

            // If goal is reached
            if (currentNode.coord.Equals(goal))
            {
                return RetracePath(currentNode);
            }

            // Explore neighbors
            foreach (Coord neighbor in GetNeighbors(currentNode.coord))
            {
                if (closedList.Contains(neighbor) || IsObstacle(neighbor))
                    continue;

                int tentativeGCost = currentNode.gCost + GetDistance(currentNode.coord, neighbor);

                Node neighborNode = openList.Find(node => node.coord.Equals(neighbor));
                if (neighborNode == null)
                {
                    // Add neighbor to the open list if it's not already there
                    neighborNode = new Node(neighbor, currentNode, tentativeGCost, GetDistance(neighbor, goal));
                    openList.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    // Update gCost and parent if a better path is found
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

        // No path found
        return null;
    }

    List<Coord> RetracePath(Node endNode)
    {
        List<Coord> path = new List<Coord>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.coord);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
    
    int GetDistance(Coord a, Coord b)
    {
        int dx = Mathf.Abs(a.tileX - b.tileX);
        int dy = Mathf.Abs(a.tileY - b.tileY);
        return dx + dy + (Mathf.Min(dx, dy) * (14 - 2 * 10));
    }

    bool IsObstacle(Coord coord)
    {
        if (coord.tileX < 0 || coord.tileY < 0 || coord.tileX >= width || coord.tileY >= height)
            return true;

        float heightValue = heightMap[coord.tileX, coord.tileY];

        // Check the current tile
        if (heightValue <= 0.2f || heightValue >= 0.7f)
            return true;

        // Check the neighbors
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Skip the center point
                if (x == 0 && y == 0)
                    continue;

                int neighborX = coord.tileX + x;
                int neighborY = coord.tileY + y;

                // Check if the neighbor is within bounds
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    float neighborHeightValue = heightMap[neighborX, neighborY];
                    if (neighborHeightValue <= 0.2f || neighborHeightValue >= 0.7f)
                        return true;
                }
            }
        }

        // Check the map value condition
        Debug.Log("print map value: " + map[coord.tileX, coord.tileY] + " and (" + coord.tileX + " " + coord.tileY + ") is obstacle:" );

        return false;
    }


    List<Coord> GetNeighbors(Coord coord)
    {
        List<Coord> neighbors = new List<Coord>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = coord.tileX + dx;
                int ny = coord.tileY + dy;
                neighbors.Add(new Coord(nx, ny));
            }
        }
        return neighbors;
    }



    class Node
    {
        public Coord coord;
        public Node parent;
        public int gCost; // Cost from start to this node
        public int hCost; // Estimated cost from this node to the goal
        public int fCost => gCost + hCost;

        public Node(Coord coord, Node parent, int gCost, int hCost)
        {
            this.coord = coord;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }
    }

    
    void CreatePassage(Room roomA, Room roomB,List<Coord> path)
    {
        Room.ConnectRooms(roomA, roomB);
        Debug.Log("path size: " + path.Count);
        foreach (Coord c in path)
        {
            // map[c.tileX, c.tileY] = -1;
            DrawCircle(c,1);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        bool shouldUpdate = true;

        // First pass: Check if any point in the circle is already -1
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int checkX = c.tileX + x;
                    int checkY = c.tileY + y;
                    if (IsInMapRange(checkX, checkY) && map[checkX, checkY] == -1)
                    {
                        shouldUpdate = false;
                        break;
                    }
                }
            }
            if (!shouldUpdate) break;
        }

        // Second pass: Update the circle if no neighbor was -1
        if (shouldUpdate)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int drawX = c.tileX + x;
                        int drawY = c.tileY + y;
                        if (IsInMapRange(drawX, drawY))
                        {
                            if (map[drawX, drawY] == 0)
                            {
                                map[drawX, drawY] = -1;
                            }
                        }
                    }
                }
            }
        }
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX; x <= tile.tileX; x++)
                {
                    for (int y = tile.tileY; y <= tile.tileY; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            Debug.Log("X is: "+ x + " y is: "+ y);
                            if (map[x, y] != 0)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }


        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
    
    ////////////////////////////////////////////////  Connect rooms code above ////////////////////////////////////////////////


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
                    if (heightMap[x, y] > 0.2 && heightMap[x, y] < 0.7)
                    {
                        // Assign zones randomly with weights
                        int randomValue = pseudoRandom.Next(0, 100);
                        if (randomValue < randomFillPercent)
                        {
                            map[x, y] = 2; // Industrial
                        }
                        else if (randomValue < (randomFillPercent * 3) - (int)(randomFillPercent / 1.3)) // 15% for industrial
                        {
                            map[x, y] = 1; // Residential
                        }
                        else
                        {
                            map[x, y] = 0; // white spaces
                        }
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
                        if (heightMap[x, y] > 0.2 && heightMap[x, y] < 0.7)
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
        
        public override bool Equals(object obj)
        {
            if (obj is Coord other)
            {
                return tileX == other.tileX && tileY == other.tileY;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return tileX.GetHashCode() ^ tileY.GetHashCode();
        }
    }
    
    bool HasOppositePurpleNeighborsOrNoWhiteNeighbors(int x, int y)
    {
        // Define the opposite neighbor pairs (top-bottom and left-right)
        var directions = new (int dx, int dy, int oppDx, int oppDy)[]
        {
            (0, -1, 0, 1),  // Top (0, -1) and Bottom (0, 1)
            (-1, 0, 1, 0)   // Left (-1, 0) and Right (1, 0)
        };

        // Check for opposite purple neighbors
        foreach (var (dx, dy, oppDx, oppDy) in directions)
        {
            int neighborX1 = x + dx;
            int neighborY1 = y + dy;
            int neighborX2 = x + oppDx;
            int neighborY2 = y + oppDy;

            // Check if both neighbors are within bounds
            bool isNeighbor1InBounds = neighborX1 >= 0 && neighborX1 < map.GetLength(0) && neighborY1 >= 0 && neighborY1 < map.GetLength(1);
            bool isNeighbor2InBounds = neighborX2 >= 0 && neighborX2 < map.GetLength(0) && neighborY2 >= 0 && neighborY2 < map.GetLength(1);

            // Check if both neighbors are purple (-1)
            if (isNeighbor1InBounds && isNeighbor2InBounds &&
                map[neighborX1, neighborY1] == -1 &&
                map[neighborX2, neighborY2] == -1)
            {
                return true;
            }
        }

        // Check if none of the neighbors are white (0)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                // Skip the center point
                if (dx == 0 && dy == 0)
                    continue;

                int neighborX = x + dx;
                int neighborY = y + dy;

                // Check if the neighbor is within bounds
                if (neighborX >= 0 && neighborX < map.GetLength(0) && neighborY >= 0 && neighborY < map.GetLength(1))
                {
                    // If any neighbor is white (0), return false
                    if (map[neighborX, neighborY] == 0)
                        return false;
                }
            }
        }

        // Return true if no neighbors are white (0)
        return true;
    }


    
    void CreateVisualMap(List<Coord> wallsAroundRooms)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
                GameObject obj = null;
                if (HasOppositePurpleNeighborsOrNoWhiteNeighbors(x, y) &&  map[x, y] == 0)
                {
                    map[x, y] = -1;
                }
                
                // Check if the current coordinate is in wallsAroundRooms
                if (wallsAroundRooms.Contains(new Coord(x, y)) &&  map[x, y] != -1)
                {
                    obj = CreateBuilding(pos, Color.black, PrimitiveType.Cube, new Vector3(1, 7, 1), "Wall");
                }
                else
                {
                    if (map[x, y] == 1) // Residential
                    {
                        obj = CreateBuilding(pos, Color.red, PrimitiveType.Cube, new Vector3(1, UnityEngine.Random.Range(1, 3), 1), "Residential");
                    }
                    else if (map[x, y] == 2) // Industrial
                    {
                        obj = CreateBuilding(pos, Color.gray, PrimitiveType.Cube, new Vector3(1, UnityEngine.Random.Range(1, 3), 1), "Industrial");
                    }
                    else if (map[x, y] == -1) // Connection
                    {
                        obj = CreateBuilding(pos, Color.magenta, PrimitiveType.Cube, new Vector3(1, UnityEngine.Random.Range(1, 3), 1), "Connection");
                    }
                    else if (map[x, y] == 0) // Empty
                    {
                        obj = CreateBuilding(pos, Color.white, PrimitiveType.Plane, new Vector3(0.5f, 0.2f, 0.5f), "Empty");
                    }
                }

                if (obj != null)
                {
                    obj.transform.parent = this.transform; // Keep hierarchy clean
                }
            }
        }
    }

    GameObject CreateBuilding(Vector3 position, Color color, PrimitiveType primitiveType, Vector3 localScale, string tag)
    {
        GameObject building = GameObject.CreatePrimitive(primitiveType);
        building.transform.position = position;
        building.transform.localScale = localScale; // Set scale
        building.GetComponent<Renderer>().material.color = color;
        building.tag = tag; // Assign tag for differentiation

        return building;
    }


}
