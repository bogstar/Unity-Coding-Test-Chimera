using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InputManager : Manager<InputManager>
{
    [Header("References")]
    [SerializeField]
    private CameraRig cameraRig;
    [SerializeField]
    private Vector2 cameraMinMaxX;
    [SerializeField]
    private Vector2 cameraMinMaxY;
    [SerializeField]
    private UnitInfoPanel selectedInfoPanel;
    [SerializeField]
    private UnitInfoPanel targetInfoPanel;

    private Mode mode;

    private Vector3 mousePos;
    private Vector3 lastMousePos;
    private bool ignoreCameraPan;

    private Tile hoveredTile;
    private Tile selectedTile;

    private EventSystem EventSystem;

    private GameManager GameManager;
    private WorldManager WorldManager;
    private World World;


    public void Initialize()
    {
        EventSystem = EventSystem.current;
        GameManager = GameManager.GetManager();
        WorldManager = WorldManager.GetManager();
        World = WorldManager.World;
        mode = Mode.Free;
    }

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
        if (IsOverUIX() && mode == Mode.Free)
        {
            // nothing
        }
        else
        {
            HandleCameraMovement();
        }

        UnhighlightAllTiles();

        switch (GameManager.phase)
        {
            case Phase.Select:

                // Left Click
                if (Input.GetMouseButtonDown(0))
                {
                    // We select a tile.
                    // If the hovered tile exists.
                    // If the hovered tile contains a unit.
                    if (hoveredTile != null && hoveredTile.Unit != null && hoveredTile.Unit == GameManager.unitOnTurn)
                    {
                        Unit unit = GameManager.unitOnTurn;
                        selectedTile = unit.Tile;
                        unit.CalculateRanges();
                        GameManager.ChangePhase(Phase.Move);
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

                GameManager.unitOnTurn.Tile.Highlight(Color.green);

                break;

            case Phase.Move:

                Unit selectedUnit = GameManager.unitOnTurn;

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
                                WorldManager.AttackUnit(selectedUnit, targetUnit, () => { GameManager.ChangePhase(Phase.None); });
                            }
                            // Otherwise, if we are in movement range.
                            else if (selectedUnit.MovementTilesInRange.Contains(targetUnit.Tile))
                            {
                                var nearestTile = Pathfinding.GetInAttackRange(selectedUnit.Tile, selectedUnit.MovementTilesInRange, targetUnit.Tile, selectedUnit.AttackRange);
                                var path = Pathfinding.FindPath(selectedUnit.Tile, nearestTile, out int cost2);

                                WorldManager.MoveUnit(selectedUnit, path, () => { WorldManager.AttackUnit(selectedUnit, targetUnit, () => { GameManager.ChangePhase(Phase.None); }); });
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

                                WorldManager.MoveUnit(selectedUnit, path, () => { GameManager.ChangePhase(Phase.Move); });
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
                else if (hoveredTile != null && hoveredTile.Unit == null)
                {
                    var path = Pathfinding.FindPath(selectedUnit.Tile, hoveredTile, out int cost, selectedUnit.MovementRemaining);

                    if (path.Length > 0)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            var attackRangeAfter = Pathfinding.GetRange(path[path.Length - 1], selectedUnit.AttackRange);
                            foreach (var ar in attackRangeAfter)
                            {
                                ar.Highlight(Color.magenta);
                            }
                        }

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

    private void HandleCameraMovement()
    {
        mousePos = cameraRig.PointFromRaycast();

        if (!ignoreCameraPan)
        {
            if (Input.GetMouseButtonDown(2))
            {
                mode = Mode.Pan;
            }
            else if (Input.GetMouseButton(2))
            {
                // If delta exists, move camera
                if (mousePos != lastMousePos && mode == Mode.Pan)
                {
                    Vector3 mouseMoveDelta = lastMousePos - mousePos;
                    Vector3 cameraPos = cameraRig.transform.position;

                    cameraPos += mouseMoveDelta;

                    // Clamp camera pos
                    float xPos = Mathf.Clamp(cameraPos.x, cameraMinMaxX.x, cameraMinMaxX.y);
                    float yPos = Mathf.Clamp(cameraPos.y, cameraMinMaxY.x, cameraMinMaxY.y);

                    cameraPos = new Vector3(xPos, yPos, -10f);

                    cameraRig.transform.position = cameraPos;
                }
            }
            else if (Input.GetMouseButtonUp(2))
            {
                mode = Mode.Free;
            }
        }
        else
        {
            ignoreCameraPan = false;
        }

        lastMousePos = cameraRig.PointFromRaycast();
    }

    private bool IsOverUIX()
    {
        return EventSystem.IsPointerOverGameObject();
    }

    public void RegisterTile(Tile tile)
    {
        tile.RegisterOnHoverOnCallback(TileOnHoverOn);
        tile.RegisterOnHoverOffCallback(TileOnHoverOff);
    }

    public void Button_EndTurn()
    {
        if (GameManager.phase == Phase.Move || GameManager.phase == Phase.Select)
        {
            GameManager.NextUnitTurn();
        }
    }

    public void Button_Restart()
    {
        SceneManager.LoadScene(0);
    }

    private void TileOnHoverOn(Tile tile)
    {
        switch (GameManager.phase)
        {
            case Phase.Select:
                hoveredTile = tile;
                break;

            case Phase.Move:
                if (GameManager.unitOnTurn != null)
                {
                    hoveredTile = tile;
                }
                break;
        }
    }

    private void TileOnHoverOff(Tile tile)
    {
        switch (GameManager.phase)
        {
            case Phase.Select:
                hoveredTile = null;
                break;
        }
    }

    private void UnhighlightAllTiles()
    {
        foreach (var tile in WorldManager.GetManager().World.Tiles)
        {
            tile.Unhiglight();
        }
    }

    public enum Mode
    {
        Free,
        Pan
    }
}