# ByCubed7s Pathfinding Library
A C# (Unity aimed) package that deals with Pathfinding using a network of nodes.

See: [A*_search_algorithm](https://en.wikipedia.org/wiki/A*_search_algorithm)

## Install

#### Unity
Unity's Package Manager can load a package from a Git repository, See [docs.unity3d](https://docs.unity3d.com/Manual/upm-ui-giturl.html) for more info~

Git: ```https://github.com/ByCubed7/Pathfinding.git```

![](https://docs.unity3d.com/uploads/Main/PackageManagerUI-GitURLPackageButton.png)
![](https://docs.unity3d.com/uploads/Main/PackageManagerUI-GitURLPackageButton-Add.png)


## Usage

```cs
// Importing the libary with:
using ByCubed7.NodeNetwork;
```

```cs
// Creating a new NodeNetwork
NodeNetwork nodeNetwork = new NodeNetwork();
```

```cs
// - Pathfinding usage example

(int, int) startNode = (startLocation.x, startLocation.y);
(int, int) endNode = (endLocation.x, endLocation.y);

List<(int, int)> nodePath = PathfindAStar(startNode, endNode);
```

```cs
// Clearing the NodeNetwork
nodeNetwork.Clear();
```

#### Tilemap convertion
How to convert a Tilemap to a Nodemap
```cs
for (int x = 0; x < board.size.x; x++) {
    for (int y = 0; y < board.size.y; y++) {
        Piece piece = board.TryGetPieceAtLocation(new Vector2Int(x, y));
        if (piece == null) CreateNode(x, y);
    }
}

// Link neighbours
for (int x = 0; x < board.size.x; x++) {//nodes.GetLength(0)
    for (int y = 0; y < board.size.y; y++) {
        if (!IsValidNode(x, y)) continue;

        if (x + 1 < board.size.x) AddNeighbour((x, y), (x + 1, y));
        if (y + 1 < board.size.y) AddNeighbour((x, y), (x, y + 1));
        if (x > 0) AddNeighbour((x, y), (x - 1, y));
        if (y > 0) AddNeighbour((x, y), (x, y - 1));
    }
}
```
