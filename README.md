# 🚀 Mars Colony - Procedural Content Generation

![Mars Colony Banner](https://via.placeholder.com/1200x400/0D1117/FFFFFF?text=Mars+Colony+Procedural+Generation)  
*Replace with your actual banner image*

## 📖 Overview

Mars Colony is a Unity-based procedural content generation system that creates realistic Martian colony layouts with distinct zones connected by optimal pathways, while respecting terrain constraints.

## ✨ Features

- **Procedural Map Generation** with Perlin noise terrain
- **Zone Allocation** (Residential/Industrial/Empty)
- **A* Pathfinding** for optimal connections
- **Terrain-Aware Placement** using elevation data
- **Customizable Parameters** for varied results
- **Room Processing** with size thresholds

## 🛠️ Installation

### Requirements
- Unity 2021.3+
- Basic C# knowledge for customization

### Setup
1. Clone this repository
2. Open in Unity
3. Attach `MapGenerator` script to a GameObject

## 🎮 Usage

```csharp
// Basic configuration
public int width = 100;          // Map width
public int height = 100;         // Map height
public int randomFillPercent = 45; // Fill percentage
public bool useRandomSeed = true; 
public string seed = "mars123";   // Fixed seed


⚙️ Configuration
Map Parameters
Parameter	Description	Recommended Range
width	Map width in tiles	50-500
height	Map height in tiles	50-500
randomFillPercent	Initial fill percentage	30-70
useRandomSeed	Randomize seed	true/false
seed	Manual seed value	Any string

🧠 Technical Details
Core Algorithms
1. Perlin Noise Generation
csharp
Copy
heightMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
2. Cellular Automata Smoothing
csharp
Copy
void SmoothMap() { ... }
3. A* Pathfinding
csharp
Copy
List<Coord> AStarPathfinding(Coord start, Coord goal) { ... }
4. Room Processing
csharp
Copy
void ProcessMap() { ... }
Data Structures
Coord
csharp
Copy
public struct Coord {
    public int tileX;
    public int tileY;
}
Room


csharp
public class Room : IComparable<Room> {
    public List<Coord> tiles;
    public List<Coord> edgeTiles;
    // ... other members
}

🖼️ Example Output
Generated Colony Layout

Zone Legend:

🔵 Blue: Residential areas

🔴 Red: Industrial zones

⚪ White: Empty space

🟣 Purple: Connecting pathways

🛠 Customization
Modifying Zone Distribution
Edit in RandomFillMap():

csharp
if (randomValue < randomFillPercent) {
    map[x, y] = 2; // Industrial
}
else if (randomValue < threshold) {
    map[x, y] = 1; // Residential
}
Adjusting Pathfinding
Modify IsObstacle():

csharp
Copy
bool IsObstacle(Coord coord) {
    return heightMap[coord.tileX, coord.tileY] <= 0.2f 
        || heightMap[coord.tileX, coord.tileY] >= 0.7f;
}


🚨 Troubleshooting
Common Issues
Issue	Solution
Paths not generating	Adjust IsObstacle() criteria
Performance issues	Reduce smoothing iterations
Disconnected rooms	Increase roomThresholdSize

📜 License
MIT License - Free for personal and commercial use

🤝 Contributing
Fork the repository

Create your feature branch (git checkout -b feature/feature-name)

Commit your changes (git commit -m 'Add some feature')

Push to the branch (git push origin feature/feature-name)

Open a Pull Request


