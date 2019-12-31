namespace TheDivineComedy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A Map represents a rectangular grid of Cells, each of which has a number of properties for determining walkability, field-of-view and so on
    /// The upper left corner of the Map is Cell (0,0) and the X value increases to the right, as the Y value increases downward
    /// </summary>
    public interface IMap
    {
        int Width {get;}
        int Height { get; }

        void Initialize(int width, int height);
        
        /// <summary>
        /// Sets the properties of all Tiles in the Map to be transparent and walkable
        /// </summary>
        void Clear(Tile tile);        

        IEnumerable<TileData> GetAllTiles();

        /// <summary>
        /// Get an IEnumerable of all the Tiles in the specified row numbers
        /// </summary>
        /// <param name="rowNumbers">Integer array of row numbers with 0 as the top</param>
        /// <returns>IEnumerable of all the Tiles in the specified row numbers</returns>
        IEnumerable<TileData> GetTilesInRows(params int[] rowNumbers);

        /// <summary>
        /// Get an IEnumerable of all the Tiles in the specified column numbers
        /// </summary>
        /// <param name="columnNumbers">Integer array of column numbers with 0 as the farthest left</param>
        /// <returns>IEnumerable of all the Tiles in the specified column numbers</returns>
        IEnumerable<TileData> GetTilesInColumns(params int[] columnNumbers);

        /// <summary>
        /// Get an IEnumerable of Cells in a square area around the center Cell up to the specified distance
        /// </summary>
        /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
        /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
        /// <param name="distance">The number of Cells to get in each direction from the center Cell</param>
        /// <returns>IEnumerable of Cells in a square area around the center Cell</returns>
        IEnumerable<TileData> GetTilesInSquare(int xCenter, int yCenter, int distance);

        /// <summary>
        /// Get an IEnumerable of Cells in a line from the Origin Cell to the Destination Cell
        /// The resulting IEnumerable includes the Origin and Destination Cells
        /// Uses Bresenham's line algorithm to determine which Cells are in the closest approximation to a straight line between the two Cells
        /// </summary>
        /// <param name="xOrigin">X location of the Origin Cell at the start of the line with 0 as the farthest left</param>
        /// <param name="yOrigin">Y location of the Origin Cell at the start of the line with 0 as the top</param>
        /// <param name="xDestination">X location of the Destination Cell at the end of the line with 0 as the farthest left</param>
        /// <param name="yDestination">Y location of the Destination Cell at the end of the line with 0 as the top</param>
        /// <returns>IEnumerable of Cells in a line from the Origin Cell to the Destination Cell which includes the Origin and Destination Cells</returns>
        IEnumerable<TileData> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination);

        /// <summary>
        /// Get a Cell at the specified location
        /// </summary>
        /// <param name="x">X location of the Cell to get starting with 0 as the farthest left</param>
        /// <param name="y">Y location of the Cell to get, starting with 0 as the top</param>
        /// <returns>Cell at the specified location</returns>
        TileData GetTile(int x, int y);

        void SetTile(int x, int y, Tile tile);

        /// <summary>
        /// Create and return a deep copy of an existing Map
        /// </summary>
        /// <returns>T of type IMap which is a deep copy of the original Map</returns>
        T Clone<T>() where T : IMap, new();

        bool IsBorderTile(Vector2Int position);

        int ClampX(int x);

        int ClampY(int y);
    }
}