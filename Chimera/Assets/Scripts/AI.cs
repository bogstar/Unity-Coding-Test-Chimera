using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager class used for handling AI.
/// </summary>
public class AI : Manager<AI>
{
    #region Private fields
    private GameManager GameManager;
    private WorldManager WorldManager;
    private World World;
    #endregion

    #region Public methods
    /// <summary>
    /// Initialize manager.
    /// </summary>
    public void Initialize()
    {
        GameManager = GameManager.GetManager();
        WorldManager = WorldManager.GetManager();
        World = WorldManager.World;
    }

    /// <summary>
    /// Bread and butter of the AI system. Decide an action and directly call it.
    /// </summary>
    /// <param name="unit">Unit for the AI calculation.</param>
    public void Decide(Unit unit)
    {
        HashSet<Unit> consideredFoes = new HashSet<Unit>();
        Tile closestTile = null;

        var foes = unit.GetFoes();

        // First we consider enemies in attack range.
        foreach (var foe in foes)
        {
            if (unit.AttackTilesInRange.Contains(foe.Tile))
            {
                consideredFoes.Add(foe);
            }
        }

        if (consideredFoes.Count > 0)
        {
            // If there are foes in attack range, choose the one with lowest health.
            int lowestHealth = int.MaxValue;
            Unit lowestHealthUnit = null;

            foreach (var foe in consideredFoes)
            {
                if (foe.CurrentHealth < lowestHealth)
                {
                    lowestHealthUnit = foe;
                    lowestHealth = foe.CurrentHealth;
                }
            }

            // Attack the poor man.
            WorldManager.AttackUnit(unit, lowestHealthUnit, () =>
            {
                // Return control to Game Manager.
                GameManager.ChangePhase(Phase.NextTurn);
            });

            // AI is done.
            return;
        }

        // If no foes in attack range, consider ones in movement range.
        foreach (var foe in foes)
        {
            if (unit.MovementTilesInRange.Contains(foe.Tile))
            {
                consideredFoes.Add(foe);
            }
        }

        if (consideredFoes.Count > 0)
        {
            // If there are foes in movement range, choose closest one.
            closestTile = GetClosestEnemyTile(unit, foes);
            
            if (closestTile != null)
            {
                // Find nearest tile from which we can attack.
                Tile nearestTile = Pathfinding.GetInAttackRange(unit.Tile, unit.MovementTilesInRange, closestTile, unit.AttackRange);
                var path2 = Pathfinding.FindPath(unit.Tile, nearestTile, out int cost2);

                // Move to that location.
                WorldManager.MoveUnit(unit, path2, () =>
                {
                    // Attack the target.
                    WorldManager.AttackUnit(unit, closestTile.Unit, () =>
                    {
                        // Return control to Game Manager.
                        GameManager.ChangePhase(Phase.NextTurn);
                    });
                });

                // AI is done.
                return;
            }
        }

        // Alternatively, we will chase enemies.
        // Find the closest enemy.
        closestTile = GetClosestEnemyTile(unit, foes);

        if (closestTile == null)
        {
            // Path cannot be found.
            // Return control to Game Manager.
            GameManager.ChangePhase(Phase.NextTurn);

            // AI is done.
            return;
        }

        // Create path to nearest enemy.
        var path4 = Pathfinding.FindPath(unit.Tile, closestTile, out int cost4, unit.MovementRange);

        // Move to the location.
        WorldManager.MoveUnit(unit, path4, () =>
        {
            // Return control to Game Manager.
            GameManager.ChangePhase(Phase.NextTurn);
        });

        // Alternatively, AI could still attack here if there is an enemy in attack range now.
        // todo?

        // AI is done.
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Retrieve closest enemy's tile.
    /// </summary>
    /// <param name="unit">Unit to calculate proximity from.</param>
    /// <param name="foes">List of the unit's foes.</param>
    /// <returns></returns>
    private Tile GetClosestEnemyTile(Unit unit, HashSet<Unit> foes)
    {
        int lowestCost = int.MaxValue;
        Tile lowestCostTile = null;

        foreach (var foe in foes)
        {
            var path3 = Pathfinding.FindPath(unit.Tile, foe.Tile, out int cost3);

            if (path3.Length > 0 && cost3 < lowestCost)
            {
                lowestCost = cost3;
                lowestCostTile = foe.Tile;
            }
        }

        return lowestCostTile;
    }
    #endregion
}