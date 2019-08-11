using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour that holds unit data and displays unit.
/// </summary>
public class Unit : MonoBehaviour
{
    #region Editor parameters
    [Header("Starting Stats")]
    [SerializeField] private string m_name;
    [SerializeField] [TextArea] private string m_description;
    [SerializeField] private int m_maxHealth;
    [SerializeField] private int m_attack;
    [SerializeField] private int m_movementRange;
    [SerializeField] private int m_attackRange;
    [SerializeField] private Sprite m_icon;
    [SerializeField] private string[] m_bonusAgainst;
    [SerializeField] private AudioClip[] m_weaponSounds;
    [SerializeField] private SpriteRenderer flagColorRenderer;
    #endregion

    #region Properties
    /// <summary>
    /// Array of fighting sounds.
    /// </summary>
    public AudioClip[] WeaponSounds { get { return m_weaponSounds; } }
    /// <summary>
    /// Array of unit names that this unit doubles attack against.
    /// </summary>
    public string[] BonusAgainst { get { return m_bonusAgainst; } }
    /// <summary>
    /// Unit's display icon.
    /// </summary>
    public Sprite Icon { get { return m_icon; } }
    /// <summary>
    /// Unit's name.
    /// </summary>
    public string Name { get { return m_name; } }
    /// <summary>
    /// Unit's description.
    /// </summary>
    public string Description { get { return m_description; } }
    /// <summary>
    /// Unit's max health.
    /// </summary>
    public int MaxHealth { get { return m_maxHealth; } }
    /// <summary>
    /// Unit's attack damage.
    /// </summary>
    public int Attack { get { return m_attack; } }
    /// <summary>
    /// Unit's movement range.
    /// </summary>
    public int MovementRange { get { return m_movementRange; } }
    /// <summary>
    /// Unit's attack range.
    /// </summary>
    public int AttackRange { get { return m_attackRange; } }
    /// <summary>
    /// Set of tiles the unit can traverse on.
    /// </summary>
    public HashSet<Tile> MovementTilesInRange { get; private set; }
    /// <summary>
    /// Set of tiles the unit can attack.
    /// </summary>
    public HashSet<Tile> AttackTilesInRange { get; private set; }
    /// <summary>
    /// Remaining movement for this turn.
    /// </summary>
    public int MovementRemaining { get; set; }
    /// <summary>
    /// Remaining health.
    /// </summary>
    public int CurrentHealth { get; private set; }
    /// <summary>
    /// Tile the unit is currently on.
    /// </summary>
    public Tile Tile { get; set; }
    /// <summary>
    /// Unit Allegiance.
    /// </summary>
    public Allegiance Allegiance { get; private set; }
    #endregion

    #region Public methods
    /// <summary>
    /// Initialize unit using selected alllegiance.
    /// </summary>
    /// <param name="allegiance">Allegiance.</param>
    public void Initialize (Allegiance allegiance)
    {
        Allegiance = allegiance;
        flagColorRenderer.color = allegiance == Allegiance.Player ? Color.green : Color.red;
        CurrentHealth = MaxHealth;
        MovementTilesInRange = new HashSet<Tile>();
    }

    /// <summary>
    /// Perform start turn calculations.
    /// </summary>
    public void StartTurn()
    {
        MovementRemaining = MovementRange;
        CalculateRanges();
    }

    /// <summary>
    /// Perform end turn calculations.
    /// </summary>
    public void EndTurn()
    {
        MovementRemaining = 0;
        CalculateRanges();
    }

    /// <summary>
    /// Calculate tiles in range (using obstacles).
    /// </summary>
    public void CalculateRanges()
    {
        MovementTilesInRange = Pathfinding.GetObstacledRange(Tile, MovementRemaining, GetFoes());
        AttackTilesInRange = Pathfinding.GetRange(Tile, AttackRange);
    }

    /// <summary>
    /// Receive damage and kill unit if health drops below 1.
    /// </summary>
    /// <param name="attacker">Attacker.</param>
    /// <param name="damage">Damage amount.</param>
    public void TakeDamage(Unit attacker, int damage)
    {
        // Double attack if attacker has bonuses.
        foreach (var bonus in attacker.BonusAgainst)
        {
            if (Name == bonus)
            {
                damage *= 2;
                break;
            }
        }

        NotificationManager.GetManager().PublishNotification(attacker.Name + " <#ff0000>attacked</color> " + Name + " dealing " + damage + " damage.");

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            NotificationManager.GetManager().PublishNotification(attacker.Name + " <#ff0000>killed</color> " + Name + ".");
            OnDeath?.Invoke(this);
        }
    }

    /// <summary>
    /// Retrieves list of all living allies.
    /// </summary>
    /// <returns></returns>
    public HashSet<Unit> GetAllies()
    {
        HashSet<Unit> allies = new HashSet<Unit>();

        foreach (var u in GameManager.GetManager().unitsOnBoard)
        {
            if (u.Allegiance == Allegiance)
            {
                allies.Add(u);
            }
        }

        return allies;
    }

    /// <summary>
    /// Retrieves list of all living foes.
    /// </summary>
    /// <returns></returns>
    public HashSet<Unit> GetFoes()
    {
        HashSet<Unit> foes = new HashSet<Unit>();

        foreach (var u in GameManager.GetManager().unitsOnBoard)
        {
            if (u.Allegiance != Allegiance)
            {
                foes.Add(u);
            }
        }

        return foes;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// Callback when unit dies.
    /// </summary>
    private event System.Action<Unit> OnDeath;

    /// <summary>
    /// Register OnDeath callback.
    /// </summary>
    /// <param name="cb"></param>
    public void RegisterDeathCallback(System.Action<Unit> cb)
    {
        OnDeath += cb;
    }

    /// <summary>
    /// Unregister OnDeath callback.
    /// </summary>
    /// <param name="cb"></param>
    public void UnregisterDeathCallback(System.Action<Unit> cb)
    {
        OnDeath -= cb;
    }
    #endregion
}