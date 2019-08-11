using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
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

    /// <summary>
    /// Callback for hover on.
    /// </summary>
    private System.Action<Tile> OnHoverOnCallback;
    /// <summary>
    /// Callback for hover off.
    /// </summary>
    private System.Action<Tile> OnHoverOffCallback;

    /// <summary>
    /// Initialize tile.
    /// </summary>
    /// <param name="world">World the tile is being initialized into.</param>
    /// <param name="renderer">SpriteRenderer of the tile.</param>
    public void Initialize(World world, SpriteRenderer renderer)
    {
        this.World = world;
        this.Renderer = renderer;
        this.MovementPenalty = 1f;
        this.Unit = null;
        this.Pathfinding = new PathfindingData();
    }

    public Unit SpawnUnit(Unit unitPrefab, Allegiance allegiance)
    {
        var unitGO = GameObject.Instantiate(unitPrefab);
        unitGO.transform.SetParent(this.transform);
        unitGO.transform.localPosition = Vector3.zero;

        Unit unit = unitGO.GetComponent<Unit>();
        unit.Initialize(allegiance);
        unit.Tile = this;

        Unit = unit;

        return unit;
    }

    public bool SetUnit(Unit unit)
    {
        if (!Empty)
            return false;

        Unit = unit;
        Unit.Tile = this;

        return true;
    }

    public void KillUnit()
    {
        Destroy(Unit.gameObject);
        RemoveUnit();
    }

    public void RemoveUnit()
    {
        Unit = null;
    }

    public void Highlight(Color color)
    {
        Renderer.color = color;
    }

    public void Unhiglight()
    {
        Renderer.color = Color.white;
    }

    #region Callbacks
    public void RegisterOnHoverOnCallback(System.Action<Tile> cb)
    {
        OnHoverOnCallback += cb;
    }

    public void RegisterOnHoverOffCallback(System.Action<Tile> cb)
    {
        OnHoverOffCallback += cb;
    }

    public void UnregisterOnHoverOnCallback(System.Action<Tile> cb)
    {
        OnHoverOnCallback -= cb;
    }

    public void UnregisterOnHoverOffCallback(System.Action<Tile> cb)
    {
        OnHoverOffCallback -= cb;
    }
    #endregion

    private void OnMouseEnter()
    {
        OnHoverOnCallback?.Invoke(this);
    }

    private void OnMouseExit()
    {
        OnHoverOffCallback?.Invoke(this);
    }

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