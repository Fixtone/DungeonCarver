namespace DungeonCarver
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The CellularAutomataMapGenerator use a classic Cellular Automata Method for Generating Random Cave-Like Levels.
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
            foreach ((Vector2Int tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    _map.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Block));
                }
                else if (_random.Next(1, 100) < _fillProbability)
                {
                    _map.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Empty));
                }
                else
                {
                    _map.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Block));
                }
            }
        }

        private void CellularAutomataBigAreaAlgorithm()
        {
            T updatedMap = _map.Clone<T>();

            foreach ((Vector2Int tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                if ((CountWallsNear(tileData, 1) >= 5) || (CountWallsNear(tileData, 2) <= 2))
                {
                    updatedMap.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Block));
                }
                else
                {
                    updatedMap.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Empty));
                }
            }

            _map = updatedMap;
        }

        private void CellularAutomataNearestNeighborsAlgorithm()
        {
            T updatedMap = _map.Clone<T>();

            foreach ((Vector2Int tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                if (CountWallsNear(tileData, 1) >= 5)
                {
                    updatedMap.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Block));
                }
                else
                {
                    updatedMap.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Empty));
                }
            }

            _map = updatedMap;
        }

        private int CountWallsNear((Vector2Int tilePosition, Tile tile) tileData, int distance)
        {
            int count = 0;
            foreach ((Vector2Int tilePosition, Tile tile) nearbyCell in _map.GetTilesInSquare(tileData.tilePosition.x, tileData.tilePosition.y, distance))
            {
                if (nearbyCell.tilePosition.x == tileData.tilePosition.x && nearbyCell.tilePosition.y == tileData.tilePosition.y)
                {
                    continue;
                }

                if (nearbyCell.tile.type.Equals(Tile.Type.Block))
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
                    IEnumerable<(Vector2Int tilePosition, Tile tile)> tunnelTiles = _map.GetCellsAlongLine((int)mapSections[i].Bounds.center.x, (int)mapSections[i].Bounds.center.y,
                       (int)closestMapSection.Bounds.center.x, (int)closestMapSection.Bounds.center.y);

                    (Vector2Int tilePosition, Tile tile) previousTile = (Vector2Int.zero, null);
                    foreach ((Vector2Int tilePosition, Tile tile) tileData in tunnelTiles)
                    {
                        _map.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Empty));

                        if (previousTile.tile != null)
                        {
                            if (tileData.tilePosition.x != previousTile.tilePosition.x || tileData.tilePosition.y != previousTile.tilePosition.y)
                            {
                                _map.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Empty));
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
                IEnumerable<(Vector2Int tilePosition, Tile tile)> tilesData = _map.GetAllTiles();
                foreach ((Vector2Int tilePosition, Tile tile) tileData in tilesData)
                {
                    MapSection section = Visit(tileData);
                    if (section.Tiles.Count > 0)
                    {
                        _mapSections.Add(section);
                    }
                }

                return _mapSections;
            }

            private MapSection Visit((Vector2Int tilePosition, Tile tile) tileData)
            {
                Stack<(Vector2Int tilePosition, Tile tile)> stack = new Stack<(Vector2Int tilePosition, Tile tile)>(new List<(Vector2Int tilePosition, Tile tile)>());
                MapSection mapSection = new MapSection();
                stack.Push(tileData);
                while (stack.Count != 0)
                {
                    tileData = stack.Pop();
                    if (_visited[tileData.tilePosition.y][tileData.tilePosition.x] || tileData.Equals(Tile.Type.Block))
                    {
                        continue;
                    }

                    mapSection.AddTile(tileData);
                    _visited[tileData.tilePosition.y][tileData.tilePosition.x] = true;

                    foreach ((Vector2Int tilePosition, Tile tile) neighbor in GetNeighbors(tileData))
                    {
                        if (tileData.Equals(Tile.Type.Empty) == neighbor.Equals(Tile.Type.Empty) && !_visited[neighbor.tilePosition.y][neighbor.tilePosition.x])
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
                return mapSection;
            }

            private IEnumerable<(Vector2Int tilePosition, Tile tile)> GetNeighbors((Vector2Int tilePosition, Tile tile) tileData)
            {
                List<(Vector2Int tilePosition, Tile tile)> neighbors = new List<(Vector2Int tilePosition, Tile tile)>(8);
                foreach (int[] offset in _offsets)
                {
                    Tile neighbor = _map.GetTile(tileData.tilePosition.x + offset[0], tileData.tilePosition.y + offset[1]);
                    if (neighbor == null)
                    {
                        continue;
                    }
                    neighbors.Add((new Vector2Int(tileData.tilePosition.x + offset[0], tileData.tilePosition.y + offset[1]), neighbor));
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

            public HashSet<(Vector2Int tilePosition, Tile tile)> Tiles
            {
                get; private set;
            }

            public MapSection()
            {
                Tiles = new HashSet<(Vector2Int tilePosition, Tile tile)>();
                _top = int.MaxValue;
                _left = int.MaxValue;
            }

            public void AddTile((Vector2Int tilePosition, Tile tile) tileData)
            {
                Tiles.Add(tileData);
                UpdateBounds(tileData);
            }

            private void UpdateBounds((Vector2Int tilePosition, Tile tile) tileData)
            {
                if (tileData.tilePosition.x > _right)
                {
                    _right = tileData.tilePosition.x;
                }

                if (tileData.tilePosition.x < _left)
                {
                    _left = tileData.tilePosition.x;
                }

                if (tileData.tilePosition.y > _bottom)
                {
                    _bottom = tileData.tilePosition.y;
                }

                if (tileData.tilePosition.y < _top)
                {
                    _top = tileData.tilePosition.y;
                }
            }
        }

    }
}
