namespace DungeonCarver
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Rather than implement a traditional cellular automata, I decided to try my hand at a method discribed by "Evil Scientist" Andy Stobirski that I recently learned about
    /// on the Grid Sage Games blog.	
    /// </summary>
    /// <seealso href="http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels">Cellular Automata Method from RogueBasin</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class CellularAutomataMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _fillProbability;
        private readonly int _totalIterations;
        private readonly int _cutoffOfBigAreaFill;
        private readonly System.Random _random;

        private T _map;

        /// <summary>
        /// Constructs a new CaveMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="width">The width of the Map to be created</param>
        /// <param name="height">The height of the Map to be created</param>
        /// <param name="fillProbability">Recommend int between 40 and 60. Percent chance that a given cell will be a floor when randomizing all cells.</param>
        /// <param name="totalIterations">Recommend int between 2 and 5. Number of times to execute the cellular automata algorithm.</param>
        /// <param name="cutoffOfBigAreaFill">Recommend int less than 4. The iteration number to switch from the large area fill algorithm to a nearest neighbor algorithm</param>        
        public CellularAutomataMapGenerator(int width, int height, int fillProbability, int totalIterations, int cutoffOfBigAreaFill, System.Random random)
        {
            _width = width;
            _height = height;
            _fillProbability = fillProbability;
            _totalIterations = totalIterations;
            _cutoffOfBigAreaFill = cutoffOfBigAreaFill;
            _random = random;

            _map = new T();
        }

        /// <summary>
        /// Creates a new IMap of the specified type.
        /// </summary>
        /// <remarks>
        /// The map will be generated using cellular automata. First each cell in the map will be set to a floor or wall randomly based on the specified fillProbability.
        /// Next each cell will be examined a number of times, and in each iteration it may be turned into a wall if there are enough other walls near it.
        /// Once finished iterating and examining neighboring cells, any isolated map regions will be connected with paths.
        /// </remarks>
        /// <returns>An IMap of the specified type</returns>
        public T CreateMap()
        {
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            RandomlyFillCells();

            for (int i = 0; i < _totalIterations; i++)
            {             
                if (i < _cutoffOfBigAreaFill)
                {
                    CellularAutomataBigAreaAlgorithm();
                }
                else if (i >= _cutoffOfBigAreaFill)
                {
                    CellularAutomataNearestNeighborsAlgorithm();
                }
            }

            ConnectCaves();

            return _map;
        }

        private void RandomlyFillCells()
        {
            foreach (TileData tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.Position))
                {
                    _map.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Block));                    
                }
                else if (_random.Next(1, 100) < _fillProbability)
                {
                    _map.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Empty));                    
                }
                else
                {
                    _map.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Block));                    
                }
            }
        }

        private void CellularAutomataBigAreaAlgorithm()
        {
            T updatedMap = _map.Clone<T>();

            foreach (TileData tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.Position))
                {
                    continue;
                }
                if ((CountWallsNear(tileData, 1) >= 5) || (CountWallsNear(tileData, 2) <= 2))
                {
                    updatedMap.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Block));                  
                }
                else
                {
                    updatedMap.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Empty));                  
                }
            }

            _map = updatedMap;
        }

        private void CellularAutomataNearestNeighborsAlgorithm()
        {
            T updatedMap = _map.Clone<T>();

            foreach (TileData tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.Position))
                {
                    continue;
                }
                if (CountWallsNear(tileData, 1) >= 5)
                {
                    updatedMap.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Block));                    
                }
                else
                {
                    updatedMap.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Empty));                    
                }
            }

            _map = updatedMap;
        }

        private int CountWallsNear(TileData tileData, int distance)
        {            
            int count = 0;
            foreach (TileData nearbyCell in _map.GetTilesInSquare(tileData.Position.x, tileData.Position.y, distance))
            {
                if (nearbyCell.Position.x == tileData.Position.x && nearbyCell.Position.y == tileData.Position.y)
                {
                    continue;
                }
                if (nearbyCell.Tile.type.Equals(Tile.Type.Block))
                {
                    count++;
                }
            }

            return count;
        }

        private void ConnectCaves()
        {
            var floodFillAnalyzer = new FloodFillAnalyzer(_map);
            List<MapSection> mapSections = floodFillAnalyzer.GetMapSections();
            UnionFind unionFind = new UnionFind(mapSections.Count);

            while (unionFind.Count > 1)
            {
                for (int i = 0; i < mapSections.Count; i++)
                {
                    int closestMapSectionIndex = FindNearestMapSection(mapSections, i, unionFind);
                    MapSection closestMapSection = mapSections[closestMapSectionIndex];
                    IEnumerable<TileData> tunnelTiles = _map.GetCellsAlongLine((int)mapSections[i].Bounds.center.x, (int)mapSections[i].Bounds.center.y,
                       (int)closestMapSection.Bounds.center.x, (int)closestMapSection.Bounds.center.y);

                    TileData previousTile = null;
                    foreach (TileData tileData in tunnelTiles)
                    {
                        _map.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Empty));

                        if (previousTile != null)
                        {
                            if (tileData.Position.x != previousTile.Position.x || tileData.Position.y != previousTile.Position.y)
                            {
                                _map.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Empty));                                
                            }
                        }
                        previousTile = tileData;
                    }
                    unionFind.Union(i, closestMapSectionIndex);
                }
            }
        }

        private static int FindNearestMapSection(IList<MapSection> mapSections, int mapSectionIndex, UnionFind unionFind)
        {
            MapSection start = mapSections[mapSectionIndex];
            int closestIndex = mapSectionIndex;
            int distance = int.MaxValue;
            for (int i = 0; i < mapSections.Count; i++)
            {
                if (i == mapSectionIndex)
                {
                    continue;
                }
                if (unionFind.Connected(i, mapSectionIndex))
                {
                    continue;
                }
                int distanceBetween = DistanceBetween(start, mapSections[i]);
                if (distanceBetween < distance)
                {
                    distance = distanceBetween;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        private static int DistanceBetween(MapSection startMapSection, MapSection destinationMapSection)
        {
            return (int)Math.Abs(startMapSection.Bounds.center.x - destinationMapSection.Bounds.center.x) + (int)Math.Abs(startMapSection.Bounds.center.y - destinationMapSection.Bounds.center.y);
        }

        private class FloodFillAnalyzer
        {
            private readonly IMap _map;
            private readonly List<MapSection> _mapSections;

            private readonly int[][] _offsets =
            {
            new[] { 0, -1 }, new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, 1 }
         };

            private readonly bool[][] _visited;

            public FloodFillAnalyzer(IMap map)
            {
                _map = map;
                _mapSections = new List<MapSection>();
                _visited = new bool[_map.Height][];
                for (int i = 0; i < _visited.Length; i++)
                {
                    _visited[i] = new bool[_map.Width];
                }
            }

            public List<MapSection> GetMapSections()
            {
                IEnumerable<TileData> tilesData = _map.GetAllTiles();
                foreach (TileData tileData in tilesData)
                {
                    MapSection section = Visit(tileData);
                    if (section.Tiles.Count > 0)
                    {
                        _mapSections.Add(section);
                    }
                }

                return _mapSections;
            }

            private MapSection Visit(TileData tileData)
            {
                Stack<TileData> stack = new Stack<TileData>(new List<TileData>());
                MapSection mapSection = new MapSection();
                stack.Push(tileData);
                while (stack.Count != 0)
                {
                    tileData = stack.Pop();
                    if (_visited[tileData.Position.y][tileData.Position.x] || tileData.Tile.Equals(Tile.Type.Block))
                    {
                        continue;
                    }

                    mapSection.AddTile(tileData);
                    _visited[tileData.Position.y][tileData.Position.x] = true;
                    
                    foreach (TileData neighbor in GetNeighbors(tileData))
                    {
                        if (tileData.Tile.Equals(Tile.Type.Empty) == neighbor.Tile.Equals(Tile.Type.Empty) && !_visited[neighbor.Position.y][neighbor.Position.x])
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
                return mapSection;
            }

            private TileData GetTile(int x, int y)
            {
                if (x < 0 || y < 0)
                {
                    return null;
                }
                if (x >= _map.Width || y >= _map.Height)
                {
                    return null;
                }
                return _map.GetTile(x, y);
            }

            private IEnumerable<TileData> GetNeighbors(TileData tileData)
            {
                List<TileData> neighbors = new List<TileData>(8);
                foreach (int[] offset in _offsets)
                {
                    TileData neighbor = GetTile(tileData.Position.x + offset[0], tileData.Position.y + offset[1]);
                    if (neighbor == null)
                    {
                        continue;
                    }
                    neighbors.Add(neighbor);
                }

                return neighbors;
            }
        }

        private class MapSection
        {
            private int _top;
            private int _bottom;
            private int _right;
            private int _left;

            public RectInt Bounds => new RectInt(_left, _top, _right - _left + 1, _bottom - _top + 1);

            public HashSet<TileData> Tiles { get; private set; }

            public MapSection()
            {
                Tiles = new HashSet<TileData>();
                _top = int.MaxValue;
                _left = int.MaxValue;
            }

            public void AddTile(TileData tileData)
            {
                Tiles.Add(tileData);
                UpdateBounds(tileData);
            }

            private void UpdateBounds(TileData tileData)
            {
                if (tileData.Position.x > _right)
                {
                    _right = tileData.Position.x;
                }
                if (tileData.Position.x < _left)
                {
                    _left = tileData.Position.x;
                }
                if (tileData.Position.y > _bottom)
                {
                    _bottom = tileData.Position.y;
                }
                if (tileData.Position.y < _top)
                {
                    _top = tileData.Position.y;
                }
            }
        }
               
    }    
}
