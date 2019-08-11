using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main manager class used for handling main game loop.
/// </summary>
public class GameManager : Manager<GameManager>
{
    #region Editor parameters
    [Header("Starting Units")]
    [SerializeField] private StartingUnitLocation[] startingPlayerUnits;
    [SerializeField] private StartingUnitLocation[] startingAIUnits;

    [Header("Audio")]
    [SerializeField] private AudioClip[] playerTurnSignal;
    [SerializeField] private AudioClip[] victorySounds;
    [SerializeField] private AudioClip[] defeatSounds;

    [Header("Prefabs")]
    [SerializeField] private GameObject m_projectilePrefab;

    [Header("References")]
    [SerializeField] private GameOverPanel gameOverPanel;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private PhasePanel phasePanel;
    [SerializeField] private TurnPanel turnPanel;
    #endregion

    #region Public properties
    /// <summary>
    /// Prefab of the projectile Game Object.
    /// </summary>
    public GameObject ProjectilePrefab { get { return m_projectilePrefab; } }
    /// <summary>
    /// List of all units on the board. It is a queue.
    /// </summary>
    public List<Unit> UnitsOnBoard { get; private set; }
    /// <summary>
    /// Unit that is currently on turn.
    /// </summary>
    public Unit UnitOnTurn { get; private set; }
    /// <summary>
    /// Current game phase.
    /// </summary>
    public Phase Phase { get; private set; }
    #endregion

    #region Private fields
    private AI AI;
    #endregion

    #region Unity methods
    private void Start()
    {
        instructionsPanel.SetActive(true);
        turnPanel.Display(true);
        phasePanel.Display(false);

        // Inject dependencies.
        WorldManager.GetManager().Initialize();
        AudioManager.GetManager().Initialize();
        InputManager.GetManager().Initialize();
        AI = AI.GetManager();
        AI.Initialize();

        ChangePhase(Phase.Animation);
    }

    private void Update()
    {
        phasePanel.Display(false);

        switch (Phase)
        {
            case Phase.NextTurn:
                NextUnitTurn();
                break;
            case Phase.Action:
                phasePanel.Display(true);
                phasePanel.SetContent("ACTION");
                break;
            case Phase.Select:
                phasePanel.Display(true);
                phasePanel.SetContent("SELECTION");
                break;
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Change phase.
    /// </summary>
    /// <param name="newPhase"></param>
    public void ChangePhase(Phase newPhase)
    {
        Phase = newPhase;
    }

    /// <summary>
    /// Process next turn.
    /// </summary>
    public void NextUnitTurn()
    {
        if (UnitOnTurn != null)
            UnitOnTurn.EndTurn();

        // Remove first unit in queue and add it to the end.
        UnitOnTurn = UnitsOnBoard[0];
        UnitsOnBoard.RemoveAt(0);
        UnitsOnBoard.Add(UnitOnTurn);

        // Start its turn.
        UnitOnTurn.StartTurn();

        switch (UnitOnTurn.Allegiance)
        {
            case Allegiance.Enemy:
                turnPanel.SetContent("ENEMY");
                AI.Decide(UnitOnTurn);
                break;

            case Allegiance.Player:
                AudioManager.GetManager().PlayCombatSound(playerTurnSignal);
                turnPanel.SetContent("PLAYER");
                ChangePhase(Phase.Action);
                break;
        }
    }
    #endregion

    #region Button events
    public void Button_InstructionsClose()
    {
        StartGame();
        instructionsPanel.SetActive(false);
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Start Match.
    /// </summary>
    private void StartGame()
    {
        // Generate world.
        WorldManager.GetManager().GenerateWorld();

        // Spawn units.
        SpawnStartingUnits();

        // Hide Game Over Panel
        // TODO: move to more appropriate place.
        gameOverPanel.gameObject.SetActive(false);

        // Set start phase.
        ChangePhase(Phase.NextTurn);
    }

    /// <summary>
    /// Spawn starting units and assign them to the world.
    /// </summary>
    private void SpawnStartingUnits()
    {
        var world = WorldManager.GetManager().World;

        UnitsOnBoard = new List<Unit>();

        foreach (var unitLocation in startingPlayerUnits)
        {
            var unit = world.SpawnUnitOnTile(unitLocation.unit, world.Tiles[unitLocation.position.x, unitLocation.position.y], Allegiance.Player);
            UnitsOnBoard.Add(unit);
            unit.RegisterDeathCallback(OnUnitDeath);
        }

        foreach (var unitLocation in startingAIUnits)
        {
            var unit = world.SpawnUnitOnTile(unitLocation.unit, world.Tiles[unitLocation.position.x, unitLocation.position.y], Allegiance.Enemy);
            UnitsOnBoard.Add(unit);
            unit.RegisterDeathCallback(OnUnitDeath);
        }

        // Shuffle turn order.
        Utilities.ShuffleList(UnitsOnBoard);
    }

    /// <summary>
    /// On Unit death callback.
    /// </summary>
    /// <param name="unit">Dead unit.</param>
    private void OnUnitDeath(Unit unit)
    {
        // Remove unit from turn queue.
        UnitsOnBoard.Remove(unit);
        // Remove unit visually.
        unit.UnregisterDeathCallback(OnUnitDeath);
        unit.Tile.KillUnit();

        // Check for win/lose condition.
        CheckWinLoseCondition();
    }

    /// <summary>
    /// Check win lose condition.
    /// </summary>
    private void CheckWinLoseCondition()
    {
        bool win = true;
        bool lose = true;

        foreach (var unit in UnitsOnBoard)
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

        // Display game over screen if condition met.
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
    #endregion

    [System.Serializable]
    public struct StartingUnitLocation
    {
        public Unit unit;
        public Vector2Int position;
    }
}