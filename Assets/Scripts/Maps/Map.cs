namespace DungeonCarver
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A Map represents a rectangular grid of Tiles, each of which has a number of properties for determining walkability
    /// The upper left corner of the Map is Tiles (0,0) and the X value increases to the right, as the Y value increases downward
    /// </summary>
    public class Map : IMap
    {
        private TileData[,] mTerrain;

        public int Width
        {
            get; private set;
        }
        public int Height
        {
            get; private set;
        }

        public Map()
        {
        }

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            mTerrain = new TileData[Width, Height];
        }

        public void Clear(Tile tile)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetTile(x, y, tile);
                }
            }
        }

        public IEnumerable<TileData> GetAllTiles()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return GetTile(x, y);
                }
            }
        }

        public IEnumerable<TileData> GetTilesInRows(params int[] rowNumbers)
        {
            foreach (int y in rowNumbers)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return GetTile(x, y);
                }
            }
        }

        public IEnumerable<(Vector2Int tilePosition, Tile tile)> GetTilesInRows2(params int[] rowNumbers)
        {
            foreach (int y in rowNumbers)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return (new Vector2Int(x,y), new Tile(Tile.Type.Block));
                }
            }
        }

        public IEnumerable<TileData> GetTilesInColumns(params int[] columnNumbers)
        {
            foreach (int x in columnNumbers)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return GetTile(x, y);
                }
            }
        }
        
        public IEnumerable<TileData> GetTilesInSquare(int xCenter, int yCenter, int distance)
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
      
        public IEnumerable<TileData> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination)
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

        public TileData GetTile(int x, int y)
        {
            return mTerrain[x, y];
        }

        public void SetTile(int x, int y, Tile tile)
        {            
            mTerrain[x, y] = new TileData(tile, new Vector2Int(x, y));
        }

        public static T Create<T>(IMapGenerator<T> mapCreationStrategy) where T : IMap
        {
            if (mapCreationStrategy == null)
            {
                Debug.LogError(nameof(mapCreationStrategy) + "Map creation strategy cannot be null");
            }

            return mapCreationStrategy.CreateMap();
        }

        public virtual T Clone<T>() where T : IMap, new()
        {
            T map = Create(new BorderOnlyMapGenerator<T>(Width, Height));
            foreach (TileData tileData in GetAllTiles())
            {
                map.SetTile(tileData.Position.x, tileData.Position.y, tileData.Tile);
            }

            return map;
        }

        public virtual bool IsBorderTile(Vector2Int position)
        {
            return position.x == 0 || position.x == Width - 1 || position.y == 0 || position.y == Height - 1;
        }

        public virtual int ClampX(int x)
        {
            return (x < 0) ? 0 : (x > Width - 1) ? Width - 1 : x;
        }

        public virtual int ClampY(int y)
        {
            return (y < 0) ? 0 : (y > Height - 1) ? Height - 1 : y;
        }
    }
}