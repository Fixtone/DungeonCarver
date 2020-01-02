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
    public class TunnelingMazeMapGenerator   <T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private const int MAGIC = 666;
        private const int CELL_R = 2;

        private readonly int _width;
        private readonly int _height;
        private readonly int _maxCellWidth;
        private readonly int _maxCellHeight;
        private readonly System.Random _random;

        private int[,] _cell;        

        private T _map;

        /// <summary>
        /// Constructs a new BorderOnlyMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="size">The size of the Map to be created</param>        
        public TunnelingMazeMapGenerator (int width, int height, System.Random random)
        {
            _width = width;
            _height = height;
            _maxCellWidth = (width / CELL_R);
            _maxCellHeight = (height / CELL_R);
            _random = random;

            _cell = new int[_maxCellWidth, _maxCellHeight];
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

            int rx = 0; int ry = 0;
            int count = 0;

            int totalCells = _maxCellWidth * _maxCellHeight;
            rx = _maxCellWidth / 2;
            ry = _maxCellHeight / 2;

            _cell[rx , ry] = 1;

            int visitedCells = 1; 

            while( visitedCells < totalCells ) 
            {
                count++;
                if (count < MAGIC)
                {
                    FillCells();
                }

                // Use Direction Lookup table for more Generic dig function.
                Vector2Int direction = MapUtils.FourDirections[_random.Next(0, MapUtils.FourDirections.Count)];
                bool isInRange = IsInRange(rx * CELL_R + direction.x, ry * CELL_R + direction.y);
                
                int x = rx + direction.x;
                int y =  ry + direction.y;
                if(isInRange && _cell[rx + direction.x, ry + direction.y] == 0 || UnityEngine.Random.Range(0, 6) == 6)
                {
                    LinkCells(rx * CELL_R, ry * CELL_R, (rx + direction.x) * CELL_R, (ry+direction.y) * CELL_R);
                    rx += direction.x;
                    ry += direction.y;
                }
                else 
                {
                    do 
                    {
                        rx = _random.Next(0, _maxCellWidth);
                        ry = _random.Next(0, _maxCellHeight);
                    } 
                    while (_cell[rx, ry] != 1);
                }

                _cell[rx, ry] = 1; 
                _map.SetTile(rx * CELL_R, ry * CELL_R, new Tile(Tile.Type.Empty));
 
                visitedCells++;
            }            

            return _map;
        }

        private void FillCells()
        {            
            for (int i = 0; i < _maxCellWidth; i++ )
            {
                for (int j = 0; j < _maxCellHeight; j++)
                {
                    if (_cell[i, j] == 1)
                    { 
                        _map.SetTile(i * CELL_R, j * CELL_R, new Tile(Tile.Type.Empty));
                    }
                }
    }
        }

        // Links our Cells
        void LinkCells(int x0, int y0, int x1, int y1)
        {
            int cx = x0; int cy = y0;
            while(cx != x1) 
            {
                if (x0 > x1)
                    cx--;
                else
                    cx++;
                if (IsInRange(cx, cy))
                {
                    _map.SetTile(cx, cy, new Tile(Tile.Type.Empty));
                }
            }

            while(cy != y1) 
            {
                if (y0 > y1)
                    cy--;
                else
                    cy++;
                if (IsInRange(cx, cy))
                {
                    _map.SetTile(cx, cy, new Tile(Tile.Type.Empty));
                }
            }
        }

        private bool IsInRange(int x, int y)
        {
            if ( x > 2 && y > 2 && x < _width - 2 && y < _height - 2)
            {
                return true;
            }

            return false;
        }
    }
}