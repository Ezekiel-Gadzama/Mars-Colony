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
Parameter	Description	Recommended Range
width	Map width in tiles	50-500
height	Map height in tiles	50-500
randomFillPercent	Initial fill percentage	30-70
useRandomSeed	Randomize seed	true/false
seed	Manual seed value	Any string


🧠 Technical Details
Key Algorithms
Perlin Noise Generation

Cellular Automata Smoothing

A Pathfinding*

Room Processing & Connection



Data Structures
csharp
Copy
struct Coord { int tileX, tileY; }  // Tile coordinates
class Room { ... }                 // Connected regions
class Node { ... }                 // A* pathfinding nodes
🖼️ Example Output
Generated Layout
Legend: Blue=Residential, Red=Industrial, White=Empty, Purple=Paths

🛠 Customization
Edit RandomFillMap() for initial distribution

Adjust SmoothMap() for automata behavior

Modify IsObstacle() for terrain constraints

🚨 Troubleshooting
Issue	Solution
Paths not connecting	Adjust IsObstacle criteria
Too many small rooms	Increase roomThresholdSize
📜 License
MIT License - Free for personal and commercial use


