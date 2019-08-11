using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plain class for holding and generating world. Handles unit spawning and dying.
/// </summary>
public class World
{
    #region Public properties.
    /// <summary>
    /// All tiles.
    /// </summary>
    public Tile[,] Tiles { get; private set; }
    /// <summary>
    /// Prefab to spawn.
    /// </summary>
    public GameObject TilePrefab { get; set; }
    /// <summary>
    /// World spawning data.
    /// </summary>
    public WorldData WorldData { get; set; }
    #endregion

    #region Public methods
    /// <summary>
    /// Generates world and visually displays it as children of the specified world holder.
    /// </summary>
    /// <param name="worldHolder">Specified world holder.</param>
    public void GenerateWorld(Transform worldHolder)
    {
        Hex hex = WorldData.hex;

        Tiles = new Tile[WorldData.worldSize.x, WorldData.worldSize.y];

        for (int y = 0; y < WorldData.worldSize.y; y++)
        {
            for (int x = 0; x < WorldData.worldSize.x; x++)
            {
                GameObject tileGO = GameObject.Instantiate(TilePrefab, worldHolder) as GameObject;

                Tile tile = tileGO.GetComponent<Tile>();

                GameObject tileGraphic = tile.gameObject;
                tileGO.name = y + " " + x;

                float cx, cy, xOffset, xCenterOffset, yOffset, yCenterOffset;
                cx = cy = xOffset = xCenterOffset = yOffset = yCenterOffset = 0;

                xOffset = hex.width + WorldData.offset;
                xCenterOffset = WorldData.worldSize.x * xOffset / 2f - hex.width * .25f;
                cx = x * xOffset - xCenterOffset;

                yOffset = hex.height * .75f + WorldData.offset;
                yCenterOffset = WorldData.worldSize.y * yOffset / 2f - hex.a * .75f;
                cy = y * yOffset - yCenterOffset;

                if (y % 2 != 0)
                {
                    switch (hex.oddity)
                    {
                        case Hex.Oddity.Odd:
                            cx += hex.width * .5f + WorldData.offset;
                            break;
                        case Hex.Oddity.Even:
                            cx -= hex.width * .5f + WorldData.offset;
                            break;
                    }
                }

                tile.transform.localPosition = new Vector3(cx, cy);

                tile.Initialize(this, tileGraphic.GetComponent<SpriteRenderer>());
                InputManager.GetManager().RegisterTile(tile);

                Tiles[x, y] = tile;
                Tiles[x, y].Position = new Vector2Int(x, y);
            }
        }
    }

    /// <summary>
    /// Move unit to new tile. This does not solve for visual. Returns true if successful.
    /// </summary>
    /// <param name="unit">Unit to move.</param>
    /// <param name="targetTile">Target tile.</param>
    /// <returns></returns>
    public bool MoveUnit(Unit unit, Tile targetTile)
    {
        Tile fromTile = unit.Tile;

        if (targetTile.SetUnit(unit))
        {
            fromTile.RemoveUnit();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Initiate kill unit.
    /// </summary>
    /// <param name="unit">Unit to kill.</param>
    public void KillUnit(Unit unit)
    {
        unit.Tile.KillUnit();
    }
    
    /// <summary>
    /// Spawn unit on tile. Returns unit spawned.
    /// </summary>
    /// <param name="unitPrefab">Prefab to spawn.</param>
    /// <param name="tile">Target tile.</param>
    /// <param name="allegiance">Allegiance of the unit.</param>
    /// <returns></returns>
    public Unit SpawnUnitOnTile(Unit unitPrefab, Tile tile, Allegiance allegiance)
    {
        if (tile == null)
        {
            throw new System.ArgumentNullException("Tile is null.");
        }

        if (unitPrefab == null)
        {
            throw new System.ArgumentNullException("Unit Prefab is null.");
        }

        if (tile.Unit != null)
        {
            throw new System.Exception("Already a unit on this tile.");
        }

        return tile.SpawnUnit(unitPrefab, allegiance);
    }
    #endregion
}

/// <summary>
/// World spawning data.
/// </summary>
public struct WorldData
{
    /// <summary>
    /// World size.
    /// </summary>
    public Vector2Int worldSize;

    /// <summary>
    /// Distance from individual hexes.
    /// </summary>
    public float offset;

    /// <summary>
    /// Hex data.
    /// </summary>
    public Hex hex;

    public WorldData(Vector2Int worldSize, float offset, Hex hex)
    {
        this.worldSize = worldSize;
        this.offset = offset;
        this.hex = hex;
    }
}