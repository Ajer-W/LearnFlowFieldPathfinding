using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = System.Random;

public class Grid
{
    public int distance;
    public Vector2Int position { get; private set; }
    public int x => position.x;
    public int y => position.y;
    public Vector3 realPosition;

    public Grid(Vector2Int pos)
    {
        this.position = pos;
    }
}

public class GridManager
{
    //How big the grid is in pixels
    int gridWidthPx = 800, gridHeightPx = 448;
    int gridPx = 32;

    //Grid size in actual units
    int gridWidth;
    int gridHeight;
    public Grid pathStart { get; private set; }
    public Grid pathEnd { get; private set; }

    Vector2 destination; //middle right

    //Storage for the current towers and enemies
    private List<Vector2Int> obstacles = new List<Vector2Int>();

    public Grid[][] dijkstraGrid { get; private set; }
    public Vector3[][] flowField;

    public GridManager()
    {
        gridWidth = gridWidthPx / gridPx;
        gridHeight = gridHeightPx / gridPx;
        destination = new Vector2(gridWidth - 2, gridHeight / 2);
    }

    public void Init()
    {
        startGame();
    }

    //Called to start the game
    void startGame()
    {
        obstacles.Clear();
        Random random = new Random();
        //Create some initial towers
        for (var i = 0; i < 30; i++)
        {
            int row = 1 + (int)(random.NextDouble() * (gridWidth - 2));
            int column = (int)(random.NextDouble() * (gridHeight - 1));
            obstacles.Add(new Vector2Int(row, column));
        }

        generateDijkstraGrid();
        generateFlowField();
        // generatePathFromDijkstraGrid();
    }

    //called periodically to update the game
    //dt is the change of time since the last update (in seconds)

    void generateDijkstraGrid()
    {
        //Generate an empty grid, set all places as weight null, which will stand for unvisited
        float _x = -(Screen.width * 0.5f + 32 * 0.5f);
        float _y = Screen.height * 0.5f + 32 * 0.5f;
        dijkstraGrid = new Grid[gridWidth][];
        for (var x = 0; x < gridWidth; x++)
        {
            var arr = new Grid[gridHeight];
            for (var y = 0; y < gridHeight; y++)
            {
                arr[y] = new Grid(new Vector2Int(x, y));
                arr[y].realPosition = new Vector3(_x + x * 32, _y - y * 32);
                arr[y].distance = -1;
            }

            dijkstraGrid[x] = arr;
        }

        int tempX = (int)Math.Round(destination.x);
        int tempY = (int)Math.Round(destination.y);

        Debug.Log($"{tempX}  {tempY}");
        pathEnd = dijkstraGrid[tempX][tempY];
        // Random random = new Random();
        // pathStart = dijkstraGrid[(int)(random.NextDouble() * gridHeight)][0];
        // pathEnd = dijkstraGrid[(int)(random.NextDouble() * gridHeight)][gridWidth - 1];

        //Set all places where towers are as being weight MAXINT, which will stand for not able to go here
        for (var i = 0; i < obstacles.Count; i++)
        {
            var t = obstacles[i];
            dijkstraGrid[t.x][t.y].distance = int.MaxValue;
        }

        //flood fill out from the end point
        // return new Vector2(Math.round(this.x), Math.round(this.y));
        pathEnd.distance = 0;
        List<Grid> toVisit = new List<Grid>() { pathEnd };
        //for each node we need to visit, starting with the pathEnd
        for (int i = 0; i < toVisit.Count; i++)
        {
            var neighbours = neighboursOf(toVisit[i]);

            //for each neighbour of this node (only straight line neighbours, not diagonals)
            for (var j = 0; j < neighbours.Count; j++)
            {
                var n = neighbours[j];

                //We will only ever visit every node once as we are always visiting nodes in the most efficient order
                if (dijkstraGrid[n.x][n.y].distance == -1)
                {
                    n.distance = toVisit[i].distance + 1;
                    dijkstraGrid[n.x][n.y].distance = n.distance;
                    toVisit.Add(n);
                }
            }
        }

        for (int i = 0; i < dijkstraGrid.Length; i++)
        {
            for (int j = 0; j < dijkstraGrid[i].Length; j++)
            {
                Debug.Log($"[{i}][{j}]=" + dijkstraGrid[i][j].distance);
            }
        }
    }

    public List<Grid> generatePathFromDijkstraGrid()
    {
        var currentWeight = dijkstraGrid[pathStart.x][pathStart.y].distance;
        if (currentWeight == -1 || currentWeight == int.MaxValue)
        {
            return null;
        }

        List<Grid> _path = new List<Grid>() { pathStart };

        var at = pathStart;
        while (at.x != pathEnd.x || at.y != pathEnd.y)
        {
            currentWeight = dijkstraGrid[at.x][at.y].distance;

            var neighbours = neighboursOf(at);
            Grid next = null;
            var nextWeight = currentWeight;

            //We are going to take the first neighbour that has lower weight than our current position
            //Randomly chosing between them or rotating which one you choose may give me pleasing results
            for (var i = 0; i < neighbours.Count; i++)
            {
                var neighbour = neighbours[i];
                var neighbourWeight = dijkstraGrid[neighbour.x][neighbour.y].distance;
                if (neighbourWeight < nextWeight)
                {
                    next = neighbour;
                    nextWeight = neighbourWeight;
                }
            }

            _path.Add(next);
            at = next;
        }

        return _path;
    }

    List<Grid> neighboursOf(Grid v)
    {
        List<Grid> res = new List<Grid>();
        if (v.x > 0)
        {
            res.Add(dijkstraGrid[v.x - 1][v.y]);
        }

        if (v.y > 0)
        {
            res.Add(dijkstraGrid[v.x][v.y - 1]);
        }

        if (v.x < gridWidth - 1)
        {
            res.Add(dijkstraGrid[v.x + 1][v.y]);
        }

        if (v.y < gridHeight - 1)
        {
            res.Add(dijkstraGrid[v.x][v.y + 1]);
        }

        return res;
    }

    void generateFlowField()
    {
        int x, y;

        //Generate an empty grid, set all places as Vector2.zero, which will stand for no good direction
        flowField = new Vector3[gridWidth][];
        for (x = 0; x < gridWidth; x++)
        {
            var arr = new Vector3[gridHeight];
            for (y = 0; y < gridHeight; y++)
            {
                arr[y] = Vector3.zero;
            }

            flowField[x] = arr;
        }

        //for each grid square
        for (x = 0; x < gridWidth; x++)
        {
            for (y = 0; y < gridHeight; y++)
            {
                //Obstacles have no flow value
                if (dijkstraGrid[x][y].distance == int.MaxValue)
                {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                var neighbours = allNeighboursOf(pos);

                //Go through all neighbours and find the one with the lowest distance
                Grid min = null;
                var minDist = 0;
                for (var i = 0; i < neighbours.Count; i++)
                {
                    var n = neighbours[i];
                    var dist = dijkstraGrid[n.x][n.y].distance - dijkstraGrid[x][y].distance;

                    if (dist < minDist)
                    {
                        min = dijkstraGrid[n.x][n.y];
                        minDist = dist;
                    }
                }

                //If we found a valid neighbour, point in its direction
                if (min != null)
                {
                    flowField[x][y] = (min.realPosition - dijkstraGrid[x][y].realPosition).normalized;
                }
            }
        }
    }

    List<Vector2Int> allNeighboursOf(Vector2Int v)
    {
        List<Vector2Int> res = new List<Vector2Int>();

        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                var x = v.x + dx;
                var y = v.y + dy;

                //All neighbours on the grid that aren't ourself
                if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight && !(dx == 0 && dy == 0))
                {
                    res.Add(new Vector2Int(x, y));
                }
            }
        }

        return res;
    }
}