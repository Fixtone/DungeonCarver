namespace DungeonCarver
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The City Walls algorithm is very similar to the BSP Tree above. In fact their main difference is in how they generate rooms after the actual tree has been created. Instead of 
	/// starting with an array of solid walls and carving out rooms connected by tunnels, the City Walls generator starts with an array of floor tiles, then creates only the
	/// exterior of the rooms, then opens one wall for a door.	
    /// </summary>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class TunnelingMazeMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {  
        private const int _jumpTile = 2;

        private readonly int _width;
        private readonly int _height;
        private readonly int _maxTileWidth;
        private readonly int _maxTileHeight;
        private readonly int _magicNumber;
        private readonly System.Random _random;

        private int[,] _tiles;

        private T _map;

        /// <summary>
        /// Constructs a new BorderOnlyMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="size">The size of the Map to be created</param>        
        public TunnelingMazeMapGenerator(int width, int height, int magicNumber, System.Random random)
        {
            _width = width;
            _height = height;
            _magicNumber = magicNumber;
            _maxTileWidth = (width / _jumpTile);
            _maxTileHeight = (height / _jumpTile);
            _random = random;

            _tiles = new int[_maxTileWidth, _maxTileHeight];
        }

        /// <summary>
        /// Creates a Map of the specified type by making an empty map with only the outermost border being solid walls
        /// </summary>
        /// <returns>An IMap of the specified type</returns>
        public T CreateMap()
        {
            _map = new T();
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            int rx = 0;
            int ry = 0;
            int count = 0;

            int totalCells = _maxTileWidth * _maxTileHeight;
            rx = _maxTileWidth / 2;
            ry = _maxTileHeight / 2;

            _tiles[rx, ry] = 1;

            int visitedCells = 1;

            while (visitedCells < totalCells)
            {
                count++;
                if (count < _magicNumber)
                {
                    FillCells();
                }

                // Use Direction Lookup table for more Generic dig function.
                Vector2Int direction = MapUtils.FourDirections[_random.Next(0, MapUtils.FourDirections.Count)];
                bool isInRange = IsInRange(rx * _jumpTile + direction.x, ry * _jumpTile + direction.y);

                int x = rx + direction.x;
                int y = ry + direction.y;
                if (isInRange && _tiles[rx + direction.x, ry + direction.y] == 0)
                {
                    LinkCells(rx * _jumpTile, ry * _jumpTile, (rx + direction.x) * _jumpTile, (ry + direction.y) * _jumpTile);
                    rx += direction.x;
                    ry += direction.y;
                }
                else
                {
                    do
                    {
                        rx = _random.Next(0, _maxTileWidth);
                        ry = _random.Next(0, _maxTileHeight);
                    }
                    while (_tiles[rx, ry] != 1);
                }

                _tiles[rx, ry] = 1;
                _map.SetTile(rx * _jumpTile, ry * _jumpTile, new Tile(Tile.Type.Empty));

                visitedCells++;
            }

            return _map;
        }

        private void FillCells()
        {
            for (int i = 0; i < _maxTileWidth; i++)
            {
                for (int j = 0; j < _maxTileHeight; j++)
                {
                    if (_tiles[i, j] == 1)
                    {
                        _map.SetTile(i * _jumpTile, j * _jumpTile, new Tile(Tile.Type.Empty));
                    }
                }
            }
        }

        // Links our Cells
        void LinkCells(int x0, int y0, int x1, int y1)
        {
            int tileX = x0;
            int tileY = y0;
            while (tileX != x1)
            {
                if (x0 > x1)
                {
                    tileX--;
                }
                else
                {
                    tileX++;
                }

                if (IsInRange(tileX, tileY))
                {
                    _map.SetTile(tileX, tileY, new Tile(Tile.Type.Empty));
                }
            }

            while (tileY != y1)
            {
                if (y0 > y1)
                {
                    tileY--;
                }
                else
                {
                    tileY++;
                }

                if (IsInRange(tileX, tileY))
                {
                    _map.SetTile(tileX, tileY, new Tile(Tile.Type.Empty));
                }
            }
        }

        private bool IsInRange(int x, int y)
        {
            if (x > 2 && y > 2 && x < _width - 2 && y < _height - 2)
            {
                return true;
            }

            return false;
        }
    }
}