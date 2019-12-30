﻿namespace TheDivineComedy
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A Map represents a rectangular grid of Cells, each of which has a number of properties for determining walkability, field-of-view and so on
    /// The upper left corner of the Map is Cell (0,0) and the X value increases to the right, as the Y value increases downward
    /// </summary>
    public class Map : IMap
    {
        private Tile[,] mTerrain;

        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Constructor creates a new uninitialized Map
        /// </summary>
        public Map()
        {
        }
                
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            mTerrain = new Tile[Width, Height];
        }

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            mTerrain = new Tile[Width, Height];
        }

        public void Clear()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetTile(new Vector2Int(x,y), new Tile(true));
                }
            }
        }

        public void Clear(Tile tile)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetTile(new Vector2Int(x, y), tile);
                }
            }
        }

        public void SetTile(Vector2Int pos, Tile tile)
        {
            mTerrain[pos.x, pos.y] = tile;
        }

        public IEnumerable<Tuple<Vector2Int, Tile>> GetAllTiles()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return GetTile(x, y);
                }
            }
        }

        public IEnumerable<Tuple<Vector2Int, Tile>> GetTilesInRows(params int[] rowNumbers)
        {
            foreach (int y in rowNumbers)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return GetTile(x, y);
                }
            }
        }

        public IEnumerable<Tuple<Vector2Int, Tile>> GetTilesInColumns(params int[] columnNumbers)
        {
            foreach (int x in columnNumbers)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return GetTile(x, y);
                }
            }
        }

        /// <summary>
        /// Get an IEnumerable of Cells in a square area around the center Cell up to the specified distance
        /// </summary>
        /// <param name="xCenter">X location of the center Cell with 0 as the farthest left</param>
        /// <param name="yCenter">Y location of the center Cell with 0 as the top</param>
        /// <param name="distance">The number of Cells to get in each direction from the center Cell</param>
        /// <returns>IEnumerable of Cells in a square area around the center Cell</returns>
        public IEnumerable<Tuple<Vector2Int, Tile>> GetTilesInSquare(int xCenter, int yCenter, int distance)
        {
            int xMin = Math.Max(0, xCenter - distance);
            int xMax = Math.Min(Width - 1, xCenter + distance);
            int yMin = Math.Max(0, yCenter - distance);
            int yMax = Math.Min(Height - 1, yCenter + distance);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    yield return GetTile(x, y);
                }
            }
        }

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
        public IEnumerable<Tuple<Vector2Int, Tile>> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination)
        {
            xOrigin = ClampX(xOrigin);
            yOrigin = ClampY(yOrigin);
            xDestination = ClampX(xDestination);
            yDestination = ClampY(yDestination);

            int dx = Math.Abs(xDestination - xOrigin);
            int dy = Math.Abs(yDestination - yOrigin);

            int sx = xOrigin < xDestination ? 1 : -1;
            int sy = yOrigin < yDestination ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return GetTile(xOrigin, yOrigin);
                if (xOrigin == xDestination && yOrigin == yDestination)
                {
                    break;
                }
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    xOrigin = xOrigin + sx;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    yOrigin = yOrigin + sy;
                }
            }
        }

        public Tuple<Vector2Int, Tile> GetTile(int x, int y)
        {
            return new Tuple<Vector2Int, Tile>(new Vector2Int(x, y), mTerrain[x, y]);            
        }

        public static T Create<T>(MapCreation.IMapCreationStrategy<T> mapCreationStrategy) where T : IMap
        {
            if (mapCreationStrategy == null)
            {
                Debug.LogError(nameof(mapCreationStrategy) + "Map creation strategy cannot be null");                
            }

            return mapCreationStrategy.CreateMap();
        }

        /// <summary>
        /// Create and return a deep copy of an existing Map.
        /// Override when a derived class has additional properties to clone.
        /// </summary>
        /// <returns>T of type IMap which is a deep copy of the original Map</returns>
        public virtual T Clone<T>() where T : IMap, new()
        {
            T map = Create(new MapCreation.BorderOnlyMapCreationStrategy<T>(Width, Height));
            foreach (Tuple<Vector2Int, Tile> tile in GetAllTiles())
            {
                map.SetTile(new Vector2Int(tile.Item1.x, tile.Item1.y), tile.Item2);
                //map.SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, cell.IsExplored);
            }

            return map;
        }

        private int ClampX(int x)
        {
            return (x < 0) ? 0 : (x > Width - 1) ? Width - 1 : x;
        }

        private int ClampY(int y)
        {
            return (y < 0) ? 0 : (y > Height - 1) ? Height - 1 : y;
        }
    }
}