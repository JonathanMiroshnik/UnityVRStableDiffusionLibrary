using UnityEngine;

/// <summary>
/// Holds needed information for a Tile in the Outpainting Mechanism screen
/// </summary>
public class OutpaintingTile : MonoBehaviour
{
    // Tile position in the screen(in the 2D screen grid)
    public Vector2Int tilePosition;
    
    public bool painted = false;
    public bool paintable = false;

    // Screen the tile is contained in
    public OutpaintingScreenScr out_screen;

    /// <summary>
    /// Used to indicate to the tile that it has been painted
    /// </summary>
    /// <param name="curPaintedStatus">Final painting status</param>
    public void SetPainted(bool curPaintedStatus)
    {
        painted = curPaintedStatus;
        out_screen.UpdateTiles(tilePosition);
    }
}
