using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager class for handling world.
/// </summary>
public class WorldManager : Manager<WorldManager>
{
    #region Editor parameters
    // All info regarding the world, can be changed from the inspector.
    [Header("Hex values")]
    [SerializeField] private int worldWidth;
    [SerializeField] private int worldHeight;
    [SerializeField] private float tileOffset;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float hexagonSide;
    [SerializeField] private Hex.Oddity hexagonOddity;

    [Header("Other world values")]
    [SerializeField] private float unitMoveSpeed = 4f;

    [Header("References")]
    [SerializeField] private GameObject worldGO;
    #endregion

    #region Public properties
    /// <summary>
    /// Current world.
    /// </summary>
    public World World { get; private set; }
    /// <summary>
    /// Current definition of hex.
    /// </summary>
    public static Hex Hex { get; private set; }
    #endregion

    #region Private properties
    private GameManager GameManager;
    #endregion

    #region Public methods
    /// <summary>
    /// Initialize world.
    /// </summary>
    public void Initialize()
    {
        GameManager = GameManager.GetManager();
    }

    /// <summary>
    /// Generate world using editor-defined parameters.
    /// </summary>
    public void GenerateWorld()
    {
        Hex = new Hex (hexagonSide, hexagonOddity);
        World = new World
        {
            TilePrefab = tilePrefab,
            WorldData = new WorldData(new Vector2Int(worldWidth, worldHeight), tileOffset, Hex)
        };

        World.GenerateWorld(worldGO.transform);
    }

    /// <summary>
    /// Initiate attack.
    /// </summary>
    /// <param name="attacker">Attacker.</param>
    /// <param name="defender">Defender.</param>
    /// <param name="attFinishedCb">Callback to be called after finishing.</param>
    public void AttackUnit(Unit attacker, Unit defender, System.Action attFinishedCb)
    {
        // Ensure no new commands can be issued.
        GameManager.ChangePhase(Phase.Moving);

        StartCoroutine(AttackUnitCoroutine(attacker, defender, attFinishedCb));
    }

    /// <summary>
    /// Initiate move of the unit using provided path.
    /// </summary>
    /// <param name="unit">Unit.</param>
    /// <param name="path">Path.</param>
    /// <param name="moveFinishedCb">Callback to be called after finishing.</param>
    public void MoveUnit(Unit unit, Tile[] path, System.Action moveFinishedCb)
    {
        // Ensure no new commands can be issued.
        GameManager.ChangePhase(Phase.Moving);

        StartCoroutine(MoveUnitCoroutine(unit, path, moveFinishedCb));
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Initiate attack, stucking Game Manager while unit attacks.
    /// </summary>
    /// <param name="attacker">Attacker.</param>
    /// <param name="defender">Defender.</param>
    /// <param name="attFinishedCb">Callback to be called after finishing.</param>
    /// <returns></returns>
    private IEnumerator AttackUnitCoroutine(Unit attacker, Unit defender, System.Action attFinishedCb)
    {
        // Start the attack sequence.
        // Spawn a projectile.
        var projectileGO = Instantiate(GameManager.projectilePrefab);
        projectileGO.transform.position = attacker.transform.position;
        Vector3 dir = (defender.transform.position - attacker.transform.position).normalized;
        projectileGO.transform.eulerAngles = new Vector3(0, 0, Vector3.SignedAngle(Vector3.up, dir, Vector3.forward));

        AudioManager.GetManager().PlayCombatSound(attacker.WeaponSounds);

        // Projectile is flying.
        while (Vector3.Distance(projectileGO.transform.position, defender.transform.position) > 0.001f)
        {
            float dst = Vector3.Distance(projectileGO.transform.position, defender.transform.position);
            float magn = unitMoveSpeed * Time.deltaTime;
            if (dst < magn)
            {
                magn = dst;
            }

            projectileGO.transform.position += dir * magn;

            yield return null;
        }

        // Destroy projectile, take damage and return control to Game Manager.
        Destroy(projectileGO);
        defender.TakeDamage(attacker, attacker.Attack);

        attFinishedCb?.Invoke();
    }

    /// <summary>
    /// Initiate move, stucking Game Manager while unit move.
    /// </summary>
    /// <param name="unit">Unit.</param>
    /// <param name="path">Path.</param>
    /// <param name="moveFinishedCb">Callback to be called after finishing.</param>
    /// <returns></returns>
    private IEnumerator MoveUnitCoroutine(Unit unit, Tile[] path, System.Action moveFinishedCb)
    {
        // Store temporary cost.
        int costToTile = path[path.Length - 1].Pathfinding.costToThisTile;
        int pathIndex = 0;

        // Start the moving sequence.
        // Move to the each next tile on the path.
        while (pathIndex != path.Length)
        {
            yield return null;
            
            Tile nextTile = path[pathIndex];
            World.MoveUnit(unit, nextTile);
            unit.transform.SetParent(nextTile.transform);

            while (Vector3.Distance(Vector3.zero, unit.transform.localPosition) > 0.001f)
            {
                Vector3 dir = (Vector3.zero - unit.transform.localPosition).normalized;
                float dst = Vector3.Distance(Vector3.zero, unit.transform.localPosition);
                float magn = unitMoveSpeed * Time.deltaTime;
                if (dst < magn)
                {
                    magn = dst;
                }

                unit.transform.localPosition += dir * magn;

                yield return null;
            }

            pathIndex++;
        }

        unit.MovementRemaining -= costToTile;
        unit.CalculateRanges();
        unit.transform.localPosition = Vector3.zero;

        NotificationManager.GetManager().PublishNotification(unit.Name + " <#00ff00>moved</color> to tile " + path[path.Length - 1].Position.x + ", " + path[path.Length - 1].Position.y + ".");

        // Return control to Game Manager.
        moveFinishedCb?.Invoke();
    }
    #endregion
}