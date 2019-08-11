using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : Manager<AI>
{
    private GameManager GameManager;
    private WorldManager WorldManager;
    private World World;

    public void Initialize()
    {
        GameManager = GameManager.GetManager();
        WorldManager = WorldManager.GetManager();
        World = WorldManager.World;
    }

    public void Decide(Unit unit)
    {
        var foes = unit.GetFoes();

        HashSet<Unit> consideredFoes = new HashSet<Unit>();

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

            WorldManager.AttackUnit(unit, lowestHealthUnit, () =>
            {
                GameManager.ChangePhase(Phase.None);
            });

            //StartCoroutine(AttackUnit(unit, lowestHealthUnit));

            return;
        }

        // Afterwards we will consider enemies in movement range.
        foreach (var foe in foes)
        {
            if (unit.MovementTilesInRange.Contains(foe.Tile))
            {
                consideredFoes.Add(foe);
            }
        }

        if (consideredFoes.Count > 0)
        {
            // Find closest foe.
            int fastestPath = int.MaxValue;
            Unit fastestPathUnit = null;

            foreach (var foe in consideredFoes)
            {
                var path = Pathfinding.FindPath(unit.Tile, foe.Tile, out int cost);

                if (path.Length > 0 && cost < fastestPath)
                {
                    fastestPath = cost;
                    fastestPathUnit = foe;
                }
            }
            
            // Find nearest tile from which we can attack.
            Tile nearestTile = Pathfinding.GetInAttackRange(unit.Tile, unit.MovementTilesInRange, fastestPathUnit.Tile, unit.AttackRange);
            var path2 = Pathfinding.FindPath(unit.Tile, nearestTile, out int cost2);

            WorldManager.MoveUnit(unit, path2, () => { WorldManager.AttackUnit(unit, fastestPathUnit, () => { GameManager.ChangePhase(Phase.None); }); });

            return;
        }

        // Alternatively, we will chase enemies.
        int lowestCost = int.MaxValue;
        Tile lowestCostTile = null;

        foreach (var foe in foes)
        {
            var path3 = Pathfinding.FindPath(unit.Tile, foe.Tile, out int c);

            if (path3.Length < 1)
            {
                continue;
            }

            if (c < lowestCost)
            {
                lowestCost = c;
                lowestCostTile = foe.Tile;
            }
        }

        if (lowestCostTile == null)
        {
            // Path cannot be found.
            GameManager.ChangePhase(Phase.None);

            return;
        }

        var path4 = Pathfinding.FindPath(unit.Tile, lowestCostTile, out int cost3, unit.MovementRange);

        WorldManager.MoveUnit(unit, path4, () => { GameManager.ChangePhase(Phase.None); });
    }
}