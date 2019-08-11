using UnityEngine;
using System.Collections;

/// <summary>
/// Mathematical representation of pure hexagon, and its helper methods.
/// </summary>
public class Hex
{
    /// <summary>
    /// Size of the base of the hex.
    /// </summary>
    public float a { get; private set; }
    public Oddity oddity { get; private set; }
    public float height { get; private set; }
    public float width { get; private set; }
    /// <summary>
    /// Horizontal spacing between hexes.
    /// </summary>
    public float horiz { get; private set; }
    /// <summary>
    /// Vertical spacing between hexes.
    /// </summary>
    public float vert { get; private set; }

    public Hex (float a, Oddity oddity)
    {
        this.a = a;
        this.oddity = oddity;

        height = a * 2f;
        vert = height * .75f;
        width = Mathf.Sqrt(3f) * a;
        horiz = height;
    }

    /// <summary>
    /// Return offset coordinates of the neigboring tile in the specified direction.
    /// </summary>
    /// <param name="hex">Hex base.</param>
    /// <param name="dir">Specified direction.</param>
    /// <param name="pairs">Coordinate pairs.</param>
    /// <returns></returns>
    public static OffsetCoordinates GetNeighborFromDir(Hex hex, CubeDirections dir, OffsetCoordinates pairs)
    {
        CubeCoordinates cubePairs = OffsetToCube(hex, pairs.col, pairs.row);

        switch (dir)
        {
            case CubeDirections.Left:
                return CubeToOffset(hex, cubePairs.x - 1, cubePairs.y + 1, cubePairs.z);
            case CubeDirections.Right:
                return CubeToOffset(hex, cubePairs.x + 1, cubePairs.y - 1, cubePairs.z);
            case CubeDirections.TopLeft:
                return CubeToOffset(hex, cubePairs.x, cubePairs.y + 1, cubePairs.z - 1);
            case CubeDirections.BottomRight:
                return CubeToOffset(hex, cubePairs.x, cubePairs.y - 1, cubePairs.z + 1);
            case CubeDirections.TopRight:
                return CubeToOffset(hex, cubePairs.x + 1, cubePairs.y, cubePairs.z - 1);
            case CubeDirections.BottomLeft:
                return CubeToOffset(hex, cubePairs.x - 1, cubePairs.y, cubePairs.z + 1);
            default:
                Debug.LogError("Some unknown CubeDirections enum received.");
                return new OffsetCoordinates(-1, -1);
        }
    }

    /// <summary>
    /// Convert from cube coord system to offset coord system.
    /// </summary>
    /// <param name="hex">Hex base.</param>
    /// <param name="coords">Offset coordinate set.</param>
    /// <returns></returns>
    public static OffsetCoordinates CubeToOffset(Hex hex, CubeCoordinates coords)
    {
        return CubeToOffset(hex, coords.x, coords.y, coords.z);
    }

    /// <summary>
    /// Convert from cube coord system to offset coord system.
    /// </summary>
    /// <param name="hex">Hex base.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="z">Z coordinate.</param>
    /// <returns></returns>
    public static OffsetCoordinates CubeToOffset(Hex hex, int x, int y, int z)
    {
        if (hex == null)
        {
            Debug.LogError("Hex is null.");
            return new OffsetCoordinates();
        }

        int row = 0;
        int col = 0;
        int offset = 1;

        if (hex.oddity == Oddity.Odd)
            offset = -1;

        col = x + (z + offset * (Mathf.Abs(z) % 2)) / 2;
        row = -z;

        return new OffsetCoordinates(row, col);
    }

    /// <summary>
    /// Convert from offset coord system to cube coord system.
    /// </summary>
    /// <param name="hex">Hex base.</param>
    /// <param name="coords">Offset coordinates.</param>
    /// <returns></returns>
    public static CubeCoordinates OffsetToCube(Hex hex, OffsetCoordinates coords)
    {
        return OffsetToCube(hex, coords.row, coords.col);
    }

    /// <summary>
    /// Convert from offset coord system to cube coord system.
    /// </summary>
    /// <param name="hex">Hex base.</param>
    /// <param name="col">C coordinate.</param>
    /// <param name="row">Q coordinate.</param>
    /// <returns></returns>
    public static CubeCoordinates OffsetToCube(Hex hex, int col, int row)
    {
        if (hex == null)
        {
            Debug.LogError("Hex is null.");
            return new CubeCoordinates();
        }

        int x = 0;
        int z = 0;
        int y = 0;
        int offset = -1;

        row = -row;

        if (hex.oddity == Oddity.Even)
            offset = 1;

        x = col - (row + offset * (Mathf.Abs(row) % 2)) / 2;
        z = row;
        y = -x - z;

        return new CubeCoordinates(x, y, z);
    }

    /// <summary>
    /// Calculate distance between two tiles.
    /// </summary>
    /// <param name="hex">Hex base.</param>
    /// <param name="a">Tile A.</param>
    /// <param name="b">Tile B.</param>
    /// <returns></returns>
    public static int Distance(Hex hex, Tile a, Tile b)
    {
        CubeCoordinates ca = OffsetToCube(hex, a.Position.x, a.Position.y);
        CubeCoordinates cb = OffsetToCube(hex, b.Position.x, b.Position.y);

        return Mathf.Max(Mathf.Abs(ca.x - cb.x), Mathf.Abs(ca.y - cb.y), Mathf.Abs(ca.z - cb.z));
    }

    /// <summary>
    /// Calculate the distance between two tiles on the specified axis.
    /// </summary>
    /// <param name="hex">Hex base.</param>
    /// <param name="a">Tile A.</param>
    /// <param name="b">Tile B.</param>
    /// <param name="axis">Axis.</param>
    /// <returns></returns>
    public static int AxisDistance(Hex hex, Tile a, Tile b, Axis axis)
    {
        CubeCoordinates ca = OffsetToCube(hex, a.Position.x, a.Position.y);
        CubeCoordinates cb = OffsetToCube(hex, b.Position.x, b.Position.y);

        switch (axis)
        {
            case Axis.X:
                return ca.x - cb.x;
            case Axis.Y:
                return ca.y - cb.y;
            case Axis.Z:
                return ca.z - cb.z;
        }

        return 0;
    }

    /// <summary>
    /// Oddity describes whether grid starts at odd or even hexes. 
    /// </summary>
    public enum Oddity
    {
        Odd,
        Even
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }

    public enum CubeDirections
    {
        Left,
        TopLeft,
        TopRight,
        Right,
        BottomRight,
        BottomLeft
    }

    /// <summary>
    /// Cube coordinates set.
    /// </summary>
    public struct CubeCoordinates
    {
        public int x;
        public int y;
        public int z;

        public CubeCoordinates (int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    /// <summary>
    /// Offset coordinates set.
    /// </summary>
    public struct OffsetCoordinates
    {
        public int col;
        public int row;

        public OffsetCoordinates (int col, int row) {
            this.col = col;
            this.row = row;
        }
    }
}