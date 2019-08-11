using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Manager<GameManager>
{
    public AudioClip[] playerTurnSignal;
    public AudioClip[] victorySounds;
    public AudioClip[] defeatSounds;

    public GameObject projectilePrefab;

    public StartingUnitLocation[] startingPlayerUnits;
    public StartingUnitLocation[] startingAIUnits;

    public GameOverPanel gameOverPanel;

    public List<Unit> unitsOnBoard;
    public Unit unitOnTurn;

    public Phase phase;

    private AI AI;

    private void Start()
    {
        WorldManager.GetManager().Initialize();
        WorldManager.GetManager().GenerateWorld();
        AudioManager.GetManager().Initialize();
        SpawnStartingUnits();

        InputManager.GetManager().Initialize();

        gameOverPanel.gameObject.SetActive(false);

        AI = AI.GetManager();
        AI.Initialize();
        StartCoroutine(ProcessGameTick());
    }

    public void ChangePhase(Phase newPhase)
    {
        phase = newPhase;
    }

    private IEnumerator ProcessGameTick()
    {
        ChangePhase(Phase.None);

        while (true)
        {
            switch (phase)
            {
                case Phase.None:
                    NextUnitTurn();
                    break;
            }

            yield return null;
        }
    }

    public void NextUnitTurn()
    {
        if (unitOnTurn != null)
            unitOnTurn.EndTurn();

        unitOnTurn = unitsOnBoard[0];
        unitsOnBoard.RemoveAt(0);
        unitsOnBoard.Add(unitOnTurn);

        unitOnTurn.StartTurn();
        if (unitOnTurn.Allegiance == Allegiance.Enemy)
        {
            AI.Decide(unitOnTurn);
        }
        else
        {
            AudioManager.GetManager().PlayCombatSound(playerTurnSignal);
            ChangePhase(Phase.Move);
        }
    }

    private void SpawnStartingUnits()
    {
        var world = WorldManager.GetManager().World;

        foreach (var unitLocation in startingPlayerUnits)
        {
            var unit = world.SpawnUnitOnTile(unitLocation.unit, world.Tiles[unitLocation.position.x, unitLocation.position.y], Allegiance.Player);
            unitsOnBoard.Add(unit);
            unit.RegisterDeathCallback(OnUnitDeath);
        }

        foreach (var unitLocation in startingAIUnits)
        {
            var unit = world.SpawnUnitOnTile(unitLocation.unit, world.Tiles[unitLocation.position.x, unitLocation.position.y], Allegiance.Enemy);
            unitsOnBoard.Add(unit);
            unit.RegisterDeathCallback(OnUnitDeath);
        }

        Utilities.ShuffleList(unitsOnBoard);
    }

    private void OnUnitDeath(Unit unit)
    {
        unitsOnBoard.Remove(unit);
        unit.UnregisterDeathCallback(OnUnitDeath);
        unit.Tile.KillUnit();
        CheckWinLoseCondition();
    }

    private void CheckWinLoseCondition()
    {
        bool win = true;
        bool lose = true;

        foreach (var unit in unitsOnBoard)
        {
            if (unit.Allegiance == Allegiance.Enemy)
            {
                win = false;
            }
            else if (unit.Allegiance == Allegiance.Player)
            {
                lose = false;
            }
        }

        if (win)
        {
            AudioManager.GetManager().PlaySfxSound(victorySounds);
            gameOverPanel.DisplayPanel("VICTORY!");
            ChangePhase(Phase.EndMatch);
        }
        else if (lose)
        {
            AudioManager.GetManager().PlaySfxSound(defeatSounds);
            gameOverPanel.DisplayPanel("DEFEAT!");
            ChangePhase(Phase.EndMatch);
        }
    }

    [System.Serializable]
    public struct StartingUnitLocation
    {
        public Unit unit;
        public Vector2Int position;
    }
}