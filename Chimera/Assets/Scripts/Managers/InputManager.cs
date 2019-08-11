using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InputManager : Manager<InputManager>
{
    #region Editor properties
    [Header("References")]
    [SerializeField] private CameraRig cameraRig;
    [SerializeField] private Vector2 cameraMinMaxX;
    [SerializeField] private Vector2 cameraMinMaxY;
    [SerializeField] private UnitInfoPanel selectedInfoPanel;
    [SerializeField] private UnitInfoPanel targetInfoPanel;
    #endregion

    #region Private fields
    private Vector3 mousePos;
    private Vector3 lastMousePos;
    private bool ignoreCameraPan;
    private Tile hoveredTile;
    private Tile selectedTile;
    #endregion

    #region Private properties
    private EventSystem EventSystem { get; set; }
    private GameManager GameManager { get; set; }
    private WorldManager WorldManager { get; set; }
    private World World { get; set; }
    private CameraMode CameraMode { get; set; }
    #endregion

    #region Unity methods
    private void OnApplicationFocus(bool focus)
    {
        // Prevent sudden spaz if we middle click to focus
        if (focus)
        {
            ignoreCameraPan = true;
        }
    }

    private void Update()
    {
        if (!IsOverUIX() || CameraMode != CameraMode.Free)
        {
            HandleCameraMovement();
        }

        UnhighlightAllTiles();

        switch (GameManager.Phase)
        {
            case Phase.Select:

                // Left Click
                if (Input.GetMouseButtonDown(0))
                {
                    // We select a tile.
                    // If the hovered tile exists.
                    // If the hovered tile contains our unit.
                    if (hoveredTile != null && hoveredTile.Unit != null && hoveredTile.Unit == GameManager.UnitOnTurn)
                    {
                        Unit unit = GameManager.UnitOnTurn;
                        selectedTile = unit.Tile;
                        unit.CalculateRanges();
                        GameManager.ChangePhase(Phase.Action);
                    }
                    // The tile is invalid selecition.
                    else
                    {
                        selectedTile = null;
                        GameManager.ChangePhase(Phase.Select);
                    }
                }

                targetInfoPanel.Display(false);

                // In select mode we can view units.
                if (hoveredTile == null)
                {
                    selectedInfoPanel.Display(false);
                }
                else
                {
                    // Display info about the unit.
                    selectedInfoPanel.Display(true);
                    selectedInfoPanel.SetInfo(hoveredTile);

                    if (hoveredTile.Unit != null)
                    {
                        Unit unit = hoveredTile.Unit;

                        // Display tiles in max movement range.
                        foreach (var t in Pathfinding.GetObstacledRange(hoveredTile, unit.MovementRange, unit.GetFoes()))
                        {
                            t.Highlight(Color.cyan);
                        }

                        // If unit has remaining movement, show it in yellow.
                        foreach (var t in unit.MovementTilesInRange)
                        {
                            t.Highlight(Color.yellow);
                        }

                        // If we hold shift, show attack range instead.
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            foreach (var tile in Pathfinding.GetRange(hoveredTile, unit.AttackRange))
                            {
                                tile.Highlight(Color.magenta);
                            }
                        }
                    }

                    hoveredTile.Highlight(Color.cyan);
                }

                GameManager.UnitOnTurn.Tile.Highlight(Color.green);

                break;

            case Phase.Action:

                Unit selectedUnit = GameManager.UnitOnTurn;

                selectedInfoPanel.Display(true);
                selectedInfoPanel.SetInfo(selectedUnit.Tile);
                targetInfoPanel.Display(false);

                // Left Click
                if (Input.GetMouseButtonDown(0))
                {
                    if (hoveredTile != null)
                    {
                        // We ordered an attack.
                        if (hoveredTile.Unit != null && selectedUnit.GetFoes().Contains(hoveredTile.Unit))
                        {
                            Unit targetUnit = hoveredTile.Unit;

                            // If we are in attack range.
                            if (selectedUnit.AttackTilesInRange.Contains(targetUnit.Tile))
                            {
                                // Attack unit.
                                WorldManager.AttackUnit(selectedUnit, targetUnit, () =>
                                {
                                    // End turn.
                                    GameManager.ChangePhase(Phase.NextTurn);
                                });
                            }
                            // Otherwise, if we are in movement range.
                            else if (selectedUnit.MovementTilesInRange.Contains(targetUnit.Tile))
                            {
                                var nearestTile = Pathfinding.GetInAttackRange(selectedUnit.Tile, selectedUnit.MovementTilesInRange, targetUnit.Tile, selectedUnit.AttackRange);
                                var path = Pathfinding.FindPath(selectedUnit.Tile, nearestTile, out int cost2);

                                // Move unit.
                                WorldManager.MoveUnit(selectedUnit, path, () =>
                                {
                                    // After that, attack enemy.
                                    WorldManager.AttackUnit(selectedUnit, targetUnit, () => 
                                    {
                                        // End turn.
                                        GameManager.ChangePhase(Phase.NextTurn);
                                    });
                                });
                            }
                        }
                        // We want to move a unit.
                        // Check if hovered tile exists, and that the tile is in range.
                        else if (selectedUnit.MovementTilesInRange.Contains(hoveredTile))
                        {
                            var path = Pathfinding.FindPath(selectedUnit.Tile, hoveredTile, out int cost);

                            if (path.Length > 0)
                            {
                                var modifiedPathfinding = path[path.Length - 1].Pathfinding;
                                modifiedPathfinding.costToThisTile = cost;
                                path[path.Length - 1].Pathfinding = modifiedPathfinding;

                                // Move unit.
                                WorldManager.MoveUnit(selectedUnit, path, () =>
                                {
                                    // Return control to player.
                                    GameManager.ChangePhase(Phase.Action);
                                });
                            }
                        }
                    }
                }

                // Right Click
                if (Input.GetMouseButtonDown(1))
                {
                    // Change to select mode.
                    GameManager.ChangePhase(Phase.Select);
                }

                // Display potential traversable tiles.
                foreach (var t in selectedUnit.MovementTilesInRange)
                {
                    if (t.Empty)
                    {
                        t.Highlight(Color.yellow);
                    }
                }

                // Check if player wants to target a foe.
                if (hoveredTile != null && hoveredTile.Unit != null && selectedUnit.GetFoes().Contains(hoveredTile.Unit))
                {
                    Unit targetUnit = hoveredTile.Unit;

                    targetInfoPanel.Display(true);
                    targetInfoPanel.SetInfo(hoveredTile);

                    foreach (var tile in Pathfinding.GetRange(selectedUnit.Tile, selectedUnit.AttackRange))
                    {
                        tile.Highlight(Color.magenta);
                    }

                    // Check if in attack range.
                    if (selectedUnit.AttackTilesInRange.Contains(targetUnit.Tile))
                    {
                        targetUnit.Tile.Highlight(Color.red);
                    }
                    // If not, check if in movement range.
                    else if (selectedUnit.MovementTilesInRange.Contains(targetUnit.Tile))
                    {
                        // Find nearest tile from which we can attack.
                        Tile nearestTile = Pathfinding.GetInAttackRange(selectedUnit.Tile, selectedUnit.MovementTilesInRange, targetUnit.Tile, selectedUnit.AttackRange);
                        var path = Pathfinding.FindPath(selectedUnit.Tile, nearestTile, out int cost);

                        foreach (var p in path)
                        {
                            p.Highlight(Color.green);
                        }

                        targetUnit.Tile.Highlight(Color.red);
                    }
                }
                // Otherwise target a tile.
                else if (hoveredTile != null && hoveredTile.Unit == null)
                {
                    // Calculate path to tile.
                    var path = Pathfinding.FindPath(selectedUnit.Tile, hoveredTile, out int cost, selectedUnit.MovementRemaining);

                    if (path.Length > 0)
                    {
                        // If path exists.
                        // If player holds shift, display attack range after move.
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            var attackRangeAfter = Pathfinding.GetRange(path[path.Length - 1], selectedUnit.AttackRange);
                            foreach (var ar in attackRangeAfter)
                            {
                                ar.Highlight(Color.magenta);
                            }
                        }

                        // Highlight path to target tile.
                        var modifiedPathfinding = path[path.Length - 1].Pathfinding;
                        modifiedPathfinding.costToThisTile = cost;
                        path[path.Length - 1].Pathfinding = modifiedPathfinding;
                        foreach (var p in path)
                        {
                            p.Highlight(Color.green);
                        }
                    }
                }

                selectedUnit.Tile.Highlight(Color.cyan);

                break;

            default:

                selectedInfoPanel.Display(false);
                targetInfoPanel.Display(false);

                break;
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Initialize manager.
    /// </summary>
    public void Initialize()
    {
        EventSystem = EventSystem.current;
        GameManager = GameManager.GetManager();
        WorldManager = WorldManager.GetManager();
        World = WorldManager.World;
        CameraMode = CameraMode.Free;
    }

    /// <summary>
    /// Register hover callbacks.
    /// </summary>
    /// <param name="tile">Tile.</param>
    public void RegisterTile(Tile tile)
    {
        tile.RegisterOnHoverOnCallback(TileOnHoverOn);
        tile.RegisterOnHoverOffCallback(TileOnHoverOff);
    }
    #endregion

    #region Button events
    public void Button_EndTurn()
    {
        if (GameManager.Phase == Phase.Action || GameManager.Phase == Phase.Select)
        {
            GameManager.NextUnitTurn();
        }
    }

    public void Button_Restart()
    {
        SceneManager.LoadScene(0);
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Handle Camera Movement.
    /// </summary>
    private void HandleCameraMovement()
    {
        mousePos = cameraRig.PointFromRaycast();

        if (!ignoreCameraPan)
        {
            if (Input.GetMouseButtonDown(2))
            {
                CameraMode = CameraMode.Pan;
            }
            else if (Input.GetMouseButton(2))
            {
                // If delta exists, move camera.
                if (mousePos != lastMousePos && CameraMode == CameraMode.Pan)
                {
                    Vector3 mouseMoveDelta = lastMousePos - mousePos;
                    Vector3 cameraPos = cameraRig.transform.position;

                    cameraPos += mouseMoveDelta;

                    // Clamp camera pos.
                    float xPos = Mathf.Clamp(cameraPos.x, cameraMinMaxX.x, cameraMinMaxX.y);
                    float yPos = Mathf.Clamp(cameraPos.y, cameraMinMaxY.x, cameraMinMaxY.y);

                    cameraPos = new Vector3(xPos, yPos, -10f);

                    cameraRig.transform.position = cameraPos;
                }
            }
            else if (Input.GetMouseButtonUp(2))
            {
                CameraMode = CameraMode.Free;
            }
        }
        else
        {
            ignoreCameraPan = false;
        }

        lastMousePos = cameraRig.PointFromRaycast();
    }

    /// <summary>
    /// Is pointer over any UI element.
    /// </summary>
    /// <returns></returns>
    private bool IsOverUIX()
    {
        return EventSystem.IsPointerOverGameObject();
    }

    /// <summary>
    /// On Tile hover on Callback.
    /// </summary>
    /// <param name="tile"></param>
    private void TileOnHoverOn(Tile tile)
    {
        switch (GameManager.Phase)
        {
            case Phase.Select:
                hoveredTile = tile;
                break;

            case Phase.Action:
                if (GameManager.UnitOnTurn != null)
                {
                    hoveredTile = tile;
                }
                break;

            case Phase.Animation:
                hoveredTile = tile;
                break;
        }
    }

    /// <summary>
    /// On Tile hover off Callback.
    /// </summary>
    /// <param name="tile"></param>
    private void TileOnHoverOff(Tile tile)
    {
        switch (GameManager.Phase)
        {
            case Phase.Select:
                hoveredTile = null;
                break;
        }
    }

    /// <summary>
    /// Unhighlight all tiles.
    /// </summary>
    private void UnhighlightAllTiles()
    {
        if (WorldManager.World == null || WorldManager.World.Tiles == null)
            return;

        foreach (var tile in WorldManager.World.Tiles)
        {
            tile.Unhiglight();
        }
    }
    #endregion
}

public enum CameraMode
{
    Free,
    Pan
}