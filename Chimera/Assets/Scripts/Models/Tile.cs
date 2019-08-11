using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour that holds tile data and displays tile.
/// </summary>
public class Tile : MonoBehaviour
{
    #region Public properties
    /// <summary>
    /// World the tile belongs to.
    /// </summary>
    public World World { get; private set; }
    /// <summary>
    /// Graphics for the tile.
    /// </summary>
    public SpriteRenderer Renderer { get; private set; }
    /// <summary>
    /// Movement penalty imposed on entities trying to pass through this tile. Number represents ratio.
    /// i.e. 0.2 - entity takes 5 times more movement to pass through this tile.
    ///      2   - entity takes half movement to pass through this tile.
    /// </summary>
    public float MovementPenalty { get; private set; }
    /// <summary>
    /// Unit currently on the tile.
    /// </summary>
    public Unit Unit { get; private set; }
    /// <summary>
    /// Tile coordinates.
    /// </summary>
    public Vector2Int Position { get; set; }
    /// <summary>
    /// Pathfinding data.
    /// </summary>
    public PathfindingData Pathfinding { get; set; }
    /// <summary>
    /// Tile completely empty and available to pathfinding.
    /// </summary>
    public bool Empty
    {
        get
        {
            return Unit == null;
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Initialize tile.
    /// </summary>
    /// <param name="world">World the tile is being initialized into.</param>
    /// <param name="renderer">SpriteRenderer of the tile.</param>
    public void Initialize(World world, SpriteRenderer renderer)
    {
        World = world;
        Renderer = renderer;
        MovementPenalty = 1f;
        Unit = null;
        Pathfinding = new PathfindingData();
    }

    /// <summary>
    /// Spawns unit prefab on the tile. Returns the spawned unit.
    /// </summary>
    /// <param name="unitPrefab"></param>
    /// <param name="allegiance"></param>
    /// <returns></returns>
    public Unit SpawnUnit(Unit unitPrefab, Allegiance allegiance)
    {
        var unitGO = GameObject.Instantiate(unitPrefab);
        unitGO.transform.SetParent(transform);
        unitGO.transform.localPosition = Vector3.zero;

        Unit unit = unitGO.GetComponent<Unit>();
        unit.Initialize(allegiance);
        unit.Tile = this;

        Unit = unit;

        return unit;
    }

    /// <summary>
    /// Assigns a unit to this tile. (Not visually)
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool SetUnit(Unit unit)
    {
        if (!Empty)
            return false;

        Unit = unit;
        Unit.Tile = this;

        return true;
    }

    /// <summary>
    /// Visually destroys unit from tile.
    /// </summary>
    public void KillUnit()
    {
        Destroy(Unit.gameObject);
        RemoveUnit();
    }

    /// <summary>
    /// Removes a unit from tile. (Not visually)
    /// </summary>
    public void RemoveUnit()
    {
        Unit = null;
    }

    /// <summary>
    /// Highlight a tile to a specific color.
    /// </summary>
    /// <param name="color"></param>
    public void Highlight(Color color)
    {
        Renderer.color = color;
    }

    /// <summary>
    /// Colors the tile back to white.
    /// </summary>
    public void Unhiglight()
    {
        Renderer.color = Color.white;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// Callback for hover on.
    /// </summary>
    private System.Action<Tile> OnHoverOnCallback;
    /// <summary>
    /// Callback for hover off.
    /// </summary>
    private System.Action<Tile> OnHoverOffCallback;

    /// <summary>
    /// Registers OnHoverOnCallback.
    /// </summary>
    /// <param name="cb"></param>
    public void RegisterOnHoverOnCallback(System.Action<Tile> cb)
    {
        OnHoverOnCallback += cb;
    }

    /// <summary>
    /// Registers OnHoverOffCallback.
    /// </summary>
    /// <param name="cb"></param>
    public void RegisterOnHoverOffCallback(System.Action<Tile> cb)
    {
        OnHoverOffCallback += cb;
    }

    /// <summary>
    /// Unregisters OnHoverOnCallback callback.
    /// </summary>
    /// <param name="cb"></param>
    public void UnregisterOnHoverOnCallback(System.Action<Tile> cb)
    {
        OnHoverOnCallback -= cb;
    }

    /// <summary>
    /// Unregisters OnHoverOffCallback callback.
    /// </summary>
    /// <param name="cb"></param>
    public void UnregisterOnHoverOffCallback(System.Action<Tile> cb)
    {
        OnHoverOffCallback -= cb;
    }
    #endregion

    #region Unity methods
    private void OnMouseEnter()
    {
        OnHoverOnCallback?.Invoke(this);
    }

    private void OnMouseExit()
    {
        OnHoverOffCallback?.Invoke(this);
    }
    #endregion

    /// <summary>
    /// Detailed info about pathfinding.
    /// </summary>
    public struct PathfindingData
    {
        public Tile parent;

        public int baseWeight;

        public int gCost;
        public int hCost;
        public int fCost
        {
            get {
                return gCost + hCost;
            }
        }

        /// <summary>
        /// Used only when this tile is the destination tile of pathfinding.
        /// </summary>
        public int costToThisTile;
    }
}