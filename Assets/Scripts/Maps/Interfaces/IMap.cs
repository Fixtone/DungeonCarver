namespace DungeonCarver
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A Map represents a rectangular grid of Tiles, each of which has a number of properties for determining walkability
    /// The upper left corner of the Map is Tiles (0,0) and the X value increases to the right, as the Y value increases downward
    /// </summary>
    public interface IMap
    {
        int Width
        {
            get;
        }
        int Height
        {
            get;
        }

        void Initialize(int width, int height);
        
        void Clear(Tile tile);

        IEnumerable<TileData> GetAllTiles();

        IEnumerable<TileData> GetTilesInRows(params int[] rowNumbers);

        IEnumerable<(Vector2Int tilePosition, Tile tile)> GetTilesInRows2(params int[] rowNumbers);

        IEnumerable<TileData> GetTilesInColumns(params int[] columnNumbers);

        IEnumerable<TileData> GetTilesInSquare(int xCenter, int yCenter, int distance);

        IEnumerable<TileData> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination);

        TileData GetTile(int x, int y);

        void SetTile(int x, int y, Tile tile);

        T Clone<T>() where T : IMap, new();

        bool IsBorderTile(Vector2Int position);

        int ClampX(int x);

        int ClampY(int y);
    }
}