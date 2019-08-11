using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class for solving pathfinding.
/// </summary>
public class Pathfinding
{
    #region Properties
    private static WorldManager m_worldManager;
    private static WorldManager WorldManager
    {
        get
        {
            if (m_worldManager == null)
                m_worldManager = WorldManager.GetManager();

            return m_worldManager;
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Retrieve set of tiles that are traversable for the unit on the starting tile. This
    /// calculation removes any untraversable tiles from the result, but includes tiles
    /// which units in [attackableUnits] set sit on.
    /// </summary>
    /// <param name="startTile">Starting tile.</param>
    /// <param name="movement">Movement range to be calculated.</param>
    /// <param name="attackableUnits">Set of units that are included for the calculation of the target tile.</param>
    /// <returns></returns>
    public static HashSet<Tile> GetObstacledRange(Tile startTile, int movement, HashSet<Unit> attackableUnits)
    {
        HashSet<Tile> visited = new HashSet<Tile> { startTile };
        List<List<Tile>> fringes = new List<List<Tile>> { new List<Tile>() { startTile } };

        for (int i = 1; i <= movement; i++)
        {
            fringes.Add(new List<Tile>());
            foreach (var tile in fringes[i - 1])
            {
                foreach (var n in GetNeighbors(tile))
                {
                    if (!visited.Contains(n) && n.Empty)
                    {
                        visited.Add(n);
                        fringes[i].Add(n);
                    }
                    else if (!visited.Contains(n) && !n.Empty && attackableUnits.Contains(n.Unit))
                    {
                        visited.Add(n);
                    }
                }
            }
        }

        return visited;
    }

    /// <summary>
    /// Retrieve tile which is furthest from target tile but still in attacker's attack range.
    /// </summary>
    /// <param name="attackerTile">Tile from which attacker starts.</param>
    /// <param name="attackerMoveTiles">Set of tiles in attacker's attack range.</param>
    /// <param name="targetTile">Target tile.</param>
    /// <param name="attackRange">Attacker's attack range.</param>
    /// <returns></returns>
    public static Tile GetInAttackRange(Tile attackerTile, HashSet<Tile> attackerMoveTiles, Tile targetTile, int attackRange)
    {
        HashSet<Tile> finalSet = new HashSet<Tile>();

        // Retrieve ring that is as far from destination as the attacker's attack range.
        var ring = GetSingleRing(targetTile, attackRange);

        // Add them to the final set to be considered.
        foreach (var r in ring)
        {
            if (attackerMoveTiles.Contains(r))
            {
                finalSet.Add(r);
            }
        }

        int lowestCost = int.MaxValue;
        Tile lowestCostTile = null;

        // Of all those tiles, find the one with lowest cost.
        foreach (var tile in finalSet)
        {
            FindPath(attackerTile, tile, out int cost);

            if (cost < lowestCost)
            {
                lowestCostTile = tile;
                lowestCost = cost;
            }
        }

        return lowestCostTile;
    }

    /// <summary>
    /// Retrieve a ring with radius indicated. The ring does not include inner tiles.
    /// </summary>
    /// <param name="tile">Center of ring.</param>
    /// <param name="radius">Radius of the ring.</param>
    /// <returns></returns>
    public static HashSet<Tile> GetSingleRing(Tile tile, int radius)
    {
        // Calculation doesn't make sense for radii less than 1.
        if (radius < 1)
            throw new System.ArgumentException("Radius can't be less than 1.");
        // If radius is 1, then we return neighbours of the center tile.
        else if (radius == 1)
            return GetRange(tile, 1);

        // Grab the inner and outer rings and do a boolean subtraction.
        var outerRing = GetRange(tile, radius);
        var innerRing = GetRange(tile, radius - 1);

        foreach (var t in innerRing)
        {
            if (outerRing.Contains(t))
            {
                outerRing.Remove(t);
            }
        }

        return outerRing;
    }

    /// <summary>
    /// Retrieve the fastest path between two tiles. Optional parameter is max range that
    /// limits the number of tiles in path. Uses A-star algorithm.
    /// </summary>
    /// <param name="startTile">Starting tile.</param>
    /// <param name="targetTile">Target tile.</param>
    /// <param name="cost">Out param that returns cost of the final path.</param>
    /// <param name="maxRange">Maximum of tiles in path. If less than 0, no maximum is set.</param>
    /// <returns></returns>
    public static Tile[] FindPath(Tile startTile, Tile targetTile, out int cost, int maxRange = -1)
    {
        Hex hex = WorldManager.Hex;

        if (startTile == null || targetTile == null)
            throw new System.ArgumentNullException("Start or End tiles are null.");

        // Grab all tiles and assign them a base weight of 1.
        foreach (Tile t in WorldManager.GetManager().World.Tiles)
        {
            t.Pathfinding = new Tile.PathfindingData() { baseWeight = 1 };
        }

        cost = 0;
        List<Tile> openSet = new List<Tile> { startTile };
        HashSet<Tile> closedSet = new HashSet<Tile>();

        if (maxRange < 0)
            maxRange = int.MaxValue;

        while (openSet.Count > 0)
        {
            Tile currentTile = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].Pathfinding.fCost <= currentTile.Pathfinding.fCost
                    && openSet[i].Pathfinding.hCost < currentTile.Pathfinding.hCost)
                {
                    currentTile = openSet[i];
                }
            }

            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (currentTile == targetTile)
            {
                return RetracePath(startTile, targetTile, out cost, maxRange);
            }

            foreach (var n in GetNeighbors(currentTile))
            {
                if (!n.Empty && n != targetTile)
                {
                    continue;
                }

                if (closedSet.Contains(n) == true)
                {
                    continue;
                }

                int weight = n.Pathfinding.baseWeight;

                int newMovementCostToNeighbor = currentTile.Pathfinding.gCost + Hex.Distance(hex, currentTile, n) * weight;

                if (newMovementCostToNeighbor < n.Pathfinding.gCost || !openSet.Contains(n))
                {
                    var pf = n.Pathfinding;

                    pf.gCost = newMovementCostToNeighbor;
                    pf.hCost = Hex.Distance(hex, n, targetTile);
                    pf.parent = currentTile;

                    n.Pathfinding = pf;

                    if (!openSet.Contains(n))
                        openSet.Add(n);
                }
            }
        }

        return new Tile[0];
    }

    /// <summary>
    /// Retrieve set of tiles in range of the specified tile using radius as a parameter.
    /// </summary>
    /// <param name="tile">Target tile.</param>
    /// <param name="radius">Radius of the range.</param>
    /// <returns></returns>
    public static HashSet<Tile> GetRange(Tile tile, int radius)
    {
        HashSet<Tile> setA = new HashSet<Tile>();
        HashSet<Tile> setB = new HashSet<Tile>();

        Hex hex = WorldManager.Hex;

        foreach (Tile t in WorldManager.World.Tiles)
        {
            if (Hex.AxisDistance(hex, tile, t, Hex.Axis.X) >= -radius && Hex.AxisDistance(hex, tile, t, Hex.Axis.X) <= radius)
            {
                setA.Add(t);
            }
        }

        foreach (Tile t in setA)
        {
            if (Hex.AxisDistance(hex, tile, t, Hex.Axis.Y) >= -radius && Hex.AxisDistance(hex, tile, t, Hex.Axis.Y) <= radius)
            {
                setB.Add(t);
            }
        }

        setA.Clear();

        foreach (Tile t in setB)
        {
            if (Hex.AxisDistance(hex, tile, t, Hex.Axis.Z) >= -radius && Hex.AxisDistance(hex, tile, t, Hex.Axis.Z) <= radius)
            {
                setA.Add(t);
            }
        }

        setA.Remove(tile);
        return setA;
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Retrace path from end to start tile using Tile's internal Pathfinding data.
    /// </summary>
    /// <param name="startTile">Starting tile.</param>
    /// <param name="endTile">Target tile.</param>
    /// <param name="cost">Out param that returns cost of the final path.</param>
    /// <param name="maxRange">Maximum of tiles in path. If less than 0, no maximum is set.</param>
    /// <returns></returns>
    private static Tile[] RetracePath(Tile startTile, Tile endTile, out int cost, int maxRange)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;

        cost = 0;

        while (currentTile != startTile)
        {
            cost += currentTile.Pathfinding.baseWeight;
            path.Add(currentTile);

            currentTile = currentTile.Pathfinding.parent;
        }

        path.Reverse();

        if (path.Count > maxRange)
        {
            path.RemoveRange(maxRange, path.Count - maxRange);
        }

        return path.ToArray();
    }

    /// <summary>
    /// Retrieve set of all neighbors to the target tile.
    /// </summary>
    /// <param name="tile">Target tile.</param>
    /// <returns></returns>
    private static HashSet<Tile> GetNeighbors(Tile tile)
    {
        HashSet<Tile> neighbors = new HashSet<Tile>();

        AddNeighbor(ref neighbors, GetDirectionCube(tile, Hex.CubeDirections.Left));
        AddNeighbor(ref neighbors, GetDirectionCube(tile, Hex.CubeDirections.TopLeft));
        AddNeighbor(ref neighbors, GetDirectionCube(tile, Hex.CubeDirections.TopRight));
        AddNeighbor(ref neighbors, GetDirectionCube(tile, Hex.CubeDirections.Right));
        AddNeighbor(ref neighbors, GetDirectionCube(tile, Hex.CubeDirections.BottomRight));
        AddNeighbor(ref neighbors, GetDirectionCube(tile, Hex.CubeDirections.BottomLeft));

        return neighbors;
    }

    /// <summary>
    /// Add neighbor to the set.
    /// </summary>
    /// <param name="neighbors">Existing set of neighbors.</param>
    /// <param name="tile">New neighbor.</param>
    private static void AddNeighbor(ref HashSet<Tile> neighbors, Tile tile)
    {
        if (neighbors.Contains(tile) == false && tile != null)
            neighbors.Add(tile);
    }

    /// <summary>
    /// Retrieve tile located in the direction specified from the selected tile.
    /// </summary>
    /// <param name="tile">Target tile.</param>
    /// <param name="dir">Cube direction.</param>
    /// <returns></returns>
    private static Tile GetDirectionCube(Tile tile, Hex.CubeDirections dir)
    {
        Hex.OffsetCoordinates offsetPairs = Hex.GetNeighborFromDir(WorldManager.Hex, dir, new Hex.OffsetCoordinates(tile.Position.x, tile.Position.y));

        if ((offsetPairs.row == tile.Position.x && offsetPairs.col == tile.Position.y) || offsetPairs.col == -1 || offsetPairs.row == -1)
            return null;

        offsetPairs.row = Mathf.Clamp(offsetPairs.row, 0, WorldManager.World.Tiles.GetLength(0) - 1);
        offsetPairs.col = Mathf.Clamp(offsetPairs.col, 0, WorldManager.World.Tiles.GetLength(1) - 1);

        return WorldManager.World.Tiles[offsetPairs.row, offsetPairs.col];
    }
    #endregion
}