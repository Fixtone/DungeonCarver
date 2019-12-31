using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheDivineComedy.MapCreation
{
    /// <summary>
    /// The CaveMapCreationStrategy creates a Map of the specified type by using a cellular automata algorithm for creating a cave-like map.
    /// </summary>
    /// <seealso href="http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels">Cellular Automata Method from RogueBasin</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class CaveMapCreationStrategy<T> : IMapCreationStrategy<T> where T : class, IMap, new()
    {
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly int _neighbours;
        private readonly int _iterations;
        private readonly int _closeTileProb;
        private readonly int _lowerLimit;
        private readonly int _upperLimit;
        private readonly int _emptyNeighbours;
        private readonly int _emptyTileNeighbours;
        private readonly int _corridorSpace;
        private readonly int _corridor_MaxTurns;
        private readonly int _corridor_Min;
        private readonly int _corridor_Max;
        private readonly int _breakOut;

        /// <summary>
        /// Caves within the map are stored here
        /// </summary>
        private List<List<Vector2Int>> _caves;

        /// <summary>
        /// Corridors within the map stored here
        /// </summary>
        private List<Vector2Int> _corridors;

        private T _map;

        /// <summary>
        /// Constructs a new CaveMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="width">The width of the Map to be created</param>
        /// <param name="height">The height of the Map to be created</param>
        /// <param name="fillProbability">Recommend int between 40 and 60. Percent chance that a given cell will be a floor when randomizing all cells.</param>
        /// <param name="totalIterations">Recommend int between 2 and 5. Number of times to execute the cellular automata algorithm.</param>
        /// <param name="cutoffOfBigAreaFill">Recommend int less than 4. The iteration number to switch from the large area fill algorithm to a nearest neighbor algorithm</param>
        /// <param name="random">A class implementing IRandom that will be used to generate pseudo-random numbers necessary to create the Map</param>
        public CaveMapCreationStrategy(int mapWidth, int mapHeight, int neighbours, int iterations, int closeTileProb, int lowerLimit, int upperLimit, int emptyNeighbours,
                                       int emptyTileNeighbours, int corridorSpace, int corridor_MaxTurns, int corridor_Min, int corridor_Max, int breakOut)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _neighbours = neighbours;
            _iterations = iterations;
            _closeTileProb = closeTileProb;
            _lowerLimit = lowerLimit;
            _upperLimit = upperLimit;
            _emptyNeighbours = emptyNeighbours;
            _emptyTileNeighbours = emptyTileNeighbours;
            _corridorSpace = corridorSpace;
            _corridor_MaxTurns = corridor_MaxTurns;
            _corridor_Min = corridor_Min;
            _corridor_Max = corridor_Max;
            _breakOut = breakOut;

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
            _map.Initialize(_mapWidth, _mapHeight);
            _map.Clear(new Tile(Tile.Type.Wall));

            BuildCaves();
            GetCaves();
            ConnectCaves();

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"D:\WriteLines2.txt"))
            {
                List<char> line = new List<char>();
                for (int y = 0; y < _mapHeight; y++)
                {
                    for (int x = 0; x < _mapWidth; x++)
                    {
                        if (_map.GetTile(x, y).Item2.type.Equals(Tile.Type.Wall))
                        {
                            line.Add('#');
                        }
                        else
                        {
                            line.Add('.');
                        }
                    }
                    file.WriteLine(line.ToArray());
                    line.Clear();
                }
            }

            return _map;
        }

        private void BuildCaves()
        {
            foreach (Tuple<Vector2Int, Tile> tile in _map.GetAllTiles())
            {
                if (IsBorderTile(tile))
                {
                    continue;
                }

                if (UnityEngine.Random.Range(0, 99) < _closeTileProb)
                {
                    _map.SetTile(tile.Item1.x, tile.Item1.y, new Tile(Tile.Type.Empty));
                }
            }

            Vector2Int tilePosition;

            //Pick cells at random
            for (int x = 0; x <= _iterations; x++)
            {
                tilePosition = new Vector2Int(UnityEngine.Random.Range(0, _mapWidth), UnityEngine.Random.Range(0, _mapHeight));

                //if the randomly selected cell has more closed neighbours than the property Neighbours
                //set it closed, else open it
                if (NeighboursGetNineDirections(tilePosition).Where(n => !_map.GetTile(n.x, n.y).Item2.type.Equals(Tile.Type.Wall)).Count() > _neighbours)
                {
                    _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Empty));
                }
                else
                {
                    _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Wall));
                }
            }

            //
            //  Smooth of the rough cave edges and any single blocks by making several 
            //  passes on the map and removing any cells with 3 or more empty neighbours
            //
            for (int ctr = 0; ctr < 5; ctr++)
            {
                //examine each cell individually
                foreach (Tuple<Vector2Int, Tile> tile in _map.GetAllTiles())
                {
                    if (IsBorderTile(tile))
                    {
                        continue;
                    }

                    tilePosition = tile.Item1;

                    if (!_map.GetTile(tilePosition.x, tilePosition.y).Item2.type.Equals(Tile.Type.Wall) && NeighboursGetFourDirections(tilePosition).Where(position => _map.GetTile(position.x, position.y).Item2.type.Equals(Tile.Type.Wall)).Count() >= _emptyNeighbours)
                    {
                        _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Wall));
                    }
                }
            }

            //
            //  fill in any empty cells that have 4 full neighbours
            //  to get rid of any holes in an cave
            //
            foreach (Tuple<Vector2Int, Tile> tile in _map.GetAllTiles())
            {
                if (IsBorderTile(tile))
                {
                    continue;
                }

                tilePosition = tile.Item1;

                if (_map.GetTile(tilePosition.x, tilePosition.y).Item2.type.Equals(Tile.Type.Wall) && NeighboursGetFourDirections(tilePosition).Where(postion => !_map.GetTile(postion.x, postion.y).Item2.type.Equals(Tile.Type.Wall)).Count() >= _emptyTileNeighbours)
                {
                    _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Empty));
                }
            }
        }

        /// <summary>
        /// Locate all the caves within the map and place each one into the generic list Caves
        /// </summary>
        private void GetCaves()
        {
            _caves = new List<List<Vector2Int>>();

            List<Vector2Int> cave;
            Vector2Int tilePosition;

            //examine each cell in the map...
            foreach (Tuple<Vector2Int, Tile> tile in _map.GetAllTiles())
            {
                if (IsBorderTile(tile))
                {
                    continue;
                }

                tilePosition = tile.Item1;

                //if the cell is closed, and that cell doesn't occur in the list of caves..
                if (!_map.GetTile(tilePosition.x, tilePosition.y).Item2.type.Equals(Tile.Type.Wall) && _caves.Count(s => s.Contains(tilePosition)) == 0)
                {
                    cave = new List<Vector2Int>();

                    //launch the recursive
                    LocateCave(tilePosition, cave);

                    //check that cave falls with the specified property range size...
                    if (cave.Count() <= _lowerLimit | cave.Count() > _upperLimit)
                    {
                        //it does, so bin it
                        foreach (Vector2Int p in cave)
                        {
                            _map.SetTile(p.x, p.y, new Tile(Tile.Type.Wall));
                        }
                    }
                    else
                    {
                        _caves.Add(cave);
                    }
                }
            }

        }

        /// <summary>
        /// Recursive method to locate the cells comprising a cave, 
        /// based on flood fill algorithm
        /// </summary>
        /// <param name="cell">Cell being examined</param>
        /// <param name="current">List containing all the cells in the cave</param>
        private void LocateCave(Vector2Int tilePosition, List<Vector2Int> cave)
        {
            foreach (Vector2Int p in NeighboursGetFourDirections(tilePosition).Where(n => !_map.GetTile(n.x, n.y).Item2.type.Equals(Tile.Type.Wall)))
            {
                if (!cave.Contains(p))
                {
                    cave.Add(p);
                    LocateCave(p, cave);
                }
            }
        }

        /// <summary>
        /// Attempt to connect the caves together
        /// </summary>
        public bool ConnectCaves()
        {
            if (_caves.Count() == 0)
            {
                return false;
            }

            List<Vector2Int> currentcave;
            List<List<Vector2Int>> ConnectedCaves = new List<List<Vector2Int>>();
            Vector2Int cor_point = new Vector2Int();
            Vector2Int cor_direction = new Vector2Int();
            List<Vector2Int> potentialcorridor = new List<Vector2Int>();
            int breakoutctr = 0;

            _corridors = new List<Vector2Int>(); //corridors built stored here

            //get started by randomly selecting a cave..
            currentcave = _caves[UnityEngine.Random.Range(0, _caves.Count())];
            ConnectedCaves.Add(currentcave);
            _caves.Remove(currentcave);

            //starting builder
            do
            {

                //no corridors are present, sp build off a cave
                if (_corridors.Count() == 0)
                {
                    currentcave = ConnectedCaves[UnityEngine.Random.Range(0, ConnectedCaves.Count())];
                    CaveGetEdge(currentcave, ref cor_point, ref cor_direction);
                }
                else
                {
                    //corridors are presnt, so randomly chose whether a get a start
                    //point from a corridor or cave
                    if (UnityEngine.Random.Range(0, 99) > 50)
                    {
                        currentcave = ConnectedCaves[UnityEngine.Random.Range(0, ConnectedCaves.Count())];
                        CaveGetEdge(currentcave, ref cor_point, ref cor_direction);
                    }
                    else
                    {
                        currentcave = null;
                        CorridorGetEdge(ref cor_point, ref cor_direction);
                    }
                }

                //using the points we've determined above attempt to build a corridor off it
                potentialcorridor = CorridorAttempt(cor_point, cor_direction, true);

                //if not null, a solid object has been hit
                if (potentialcorridor != null)
                {
                    //examine all the caves
                    for (int ctr = 0; ctr < _caves.Count(); ctr++)
                    {
                        //check if the last point in the corridor list is in a cave
                        if (_caves[ctr].Contains(potentialcorridor.Last()))
                        {
                            //we've built of a corridor or built of a room 
                            if (currentcave == null | currentcave != _caves[ctr])
                            {
                                //the last corridor point intrudes on the room, so remove it
                                potentialcorridor.Remove(potentialcorridor.Last());
                                //add the corridor to the corridor collection
                                _corridors.AddRange(potentialcorridor);
                                //write it to the map
                                foreach (Vector2Int p in potentialcorridor)
                                {
                                    _map.SetTile(p.x, p.y, new Tile(Tile.Type.Empty));
                                }

                                //the room reached is added to the connected list...
                                ConnectedCaves.Add(_caves[ctr]);
                                //...and removed from the Caves list
                                _caves.RemoveAt(ctr);

                                break;
                            }
                        }
                    }
                }

                //breakout
                if (breakoutctr++ > _breakOut)
                {
                    return false;
                }

            } while (_caves.Count() > 0);

            _caves.AddRange(ConnectedCaves);
            ConnectedCaves.Clear();

            return true;
        }

        /// <summary>
        /// Locate the edge of the specified cave
        /// </summary>
        /// <param name="pCaveNumber">Cave to examine</param>
        /// <param name="pCavePoint">Point on the edge of the cave</param>
        /// <param name="pDirection">Direction to start formting the tunnel</param>
        /// <returns>Boolean indicating if an edge was found</returns>
        private void CaveGetEdge(List<Vector2Int> pCave, ref Vector2Int pCavePoint, ref Vector2Int pDirection)
        {
            do
            {
                //random point in cave
                pCavePoint = pCave.ToList()[UnityEngine.Random.Range(0, pCave.Count())];

                pDirection = FourDirectiosnGet(pDirection);

                do
                {
                    pCavePoint += (pDirection);

                    if (!TilePositionCheck(pCavePoint))
                    {
                        break;
                    }
                    else if (_map.GetTile(pCavePoint.x, pCavePoint.y).Item2.type.Equals(Tile.Type.Wall))
                    {
                        return;
                    }

                } while (true);
            } while (true);
        }

        // <summary>
        /// Randomly get a point on an existing corridor
        /// </summary>
        /// <param name="Location">Out: location of point</param>
        /// <returns>Bool indicating success</returns>
        private void CorridorGetEdge(ref Vector2Int pLocation, ref Vector2Int pDirection)
        {
            List<Vector2Int> validdirections = new List<Vector2Int>();

            do
            {
                //the modifiers below prevent the first of last point being chosen
                pLocation = _corridors[UnityEngine.Random.Range(1, _corridors.Count - 1)];

                //attempt to locate all the empy map points around the location
                //using the directions to offset the randomly chosen point
                foreach (Vector2Int p in MapUtils.FourDirections)
                {
                    if (TilePositionCheck(new Vector2Int(pLocation.x + p.x, pLocation.y + p.y)))
                    {
                        if (_map.GetTile(pLocation.x + p.x, pLocation.y + p.y).Item2.type.Equals(Tile.Type.Wall))
                        {
                            validdirections.Add(p);
                        }
                    }
                }

            } while (validdirections.Count == 0);

            pDirection = validdirections[UnityEngine.Random.Range(0, validdirections.Count)];
            pLocation += pDirection;
        }

        /// <summary>
        /// Attempt to build a corridor
        /// </summary>
        /// <param name="pStart"></param>
        /// <param name="pDirection"></param>
        /// <param name="pPreventBackTracking"></param>
        /// <returns></returns>
        private List<Vector2Int> CorridorAttempt(Vector2Int pStart, Vector2Int pDirection, bool pPreventBackTracking)
        {

            List<Vector2Int> lPotentialCorridor = new List<Vector2Int>();
            lPotentialCorridor.Add(pStart);

            int corridorlength;
            Vector2Int startdirection = new Vector2Int(pDirection.x, pDirection.y);

            int pTurns = _corridor_MaxTurns;

            while (pTurns >= 0)
            {
                pTurns--;

                corridorlength = UnityEngine.Random.Range(_corridor_Min, _corridor_Max);
                //build corridor
                while (corridorlength > 0)
                {
                    corridorlength--;

                    //make a point and offset it
                    pStart += pDirection;

                    if (TilePositionCheck(pStart) && !_map.GetTile(pStart.x, pStart.y).Item2.type.Equals(Tile.Type.Wall))
                    {
                        lPotentialCorridor.Add(pStart);
                        return lPotentialCorridor;
                    }

                    if (!TilePositionCheck(pStart))
                    {
                        return null;
                    }
                    else if (!CorridorPointTest(pStart, pDirection))
                    {
                        return null;
                    }

                    lPotentialCorridor.Add(pStart);

                }

                if (pTurns > 1)
                {
                    if (!pPreventBackTracking)
                    {
                        pDirection = FourDirectiosnGet(pDirection);
                    }
                    else
                    {
                        pDirection = FourDirectiosnGet(pDirection, startdirection);
                    }
                }
            }

            return null;
        }

        private bool CorridorPointTest(Vector2Int pPoint, Vector2Int pDirection)
        {
            //using the property corridor space, check that number of cells on
            //either side of the point are empty
            foreach (int r in Enumerable.Range(-_corridorSpace, 2 * _corridorSpace + 1).ToList())
            {
                if (pDirection.x == 0)//north or south
                {
                    if (TilePositionCheck(new Vector2Int(pPoint.x + r, pPoint.y)))
                    {
                        if (!_map.GetTile(pPoint.x + r, pPoint.y).Item2.type.Equals(Tile.Type.Wall))
                        {
                            return false;
                        }
                    }
                }
                else if (pDirection.y == 0)//east west
                {
                    if (TilePositionCheck(new Vector2Int(pPoint.x, pPoint.y + r)))
                    {
                        if (!_map.GetTile(pPoint.x, pPoint.y + r).Item2.type.Equals(Tile.Type.Wall))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Get a random direction, provided it isn't equal to the opposite one provided
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Vector2Int FourDirectiosnGet(Vector2Int p)
        {
            Vector2Int newdir;
            do
            {

                newdir = MapUtils.FourDirections[UnityEngine.Random.Range(0, MapUtils.FourDirections.Count())];

            } while (newdir.x != -p.x & newdir.y != -p.y);

            return newdir;
        }

        /// <summary>
        /// Get a random direction, excluding the provided directions and the opposite of 
        /// the provided direction to prevent a corridor going back on it's self.
        /// 
        /// The parameter pDirExclude is the first direction chosen for a corridor, and
        /// to prevent it from being used will prevent a corridor from going back on 
        /// it'self
        /// </summary>
        /// <param name="dir">Current direction</param>
        /// <param name="pDirectionList">Direction to exclude</param>
        /// <param name="pDirExclude">Direction to exclude</param>
        /// <returns></returns>
        private Vector2Int FourDirectiosnGet(Vector2Int pDir, Vector2Int pDirExclude)
        {
            Vector2Int NewDir;
            do
            {

                NewDir = MapUtils.FourDirections[UnityEngine.Random.Range(0, MapUtils.FourDirections.Count())];

            } while (DirectionReverse(NewDir) == pDir | DirectionReverse(NewDir) == pDirExclude);


            return NewDir;
        }

        private Vector2Int DirectionReverse(Vector2Int pDir)
        {
            return new Vector2Int(-pDir.x, -pDir.y);
        }


        /// <summary>
        /// Return a list of the valid neighbouring cells of the provided point
        /// using only north, south, east and west
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private List<Vector2Int> NeighboursGetFourDirections(Vector2Int tilePosition)
        {
            return MapUtils.FourDirections.Select(direction => new Vector2Int(tilePosition.x + direction.x, tilePosition.y + direction.y)).Where(direction => TilePositionCheck(direction)).ToList();
        }

        /// <summary>
        /// Return a list of the valid neighbouring cells of the provided point
        /// using north, south, east, ne,nw,se,sw
        private List<Vector2Int> NeighboursGetNineDirections(Vector2Int tilePosition)
        {
            return MapUtils.NineDirections.Select(direction => new Vector2Int(tilePosition.x + direction.x, tilePosition.y + direction.y)).Where(direction => TilePositionCheck(direction)).ToList();
        }

        /// <summary>
        /// Check if the provided point is valid
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns></returns>
        private bool TilePositionCheck(Vector2Int tilePosition)
        {
            return tilePosition.x >= 0 & tilePosition.x < _mapWidth & tilePosition.y >= 0 & tilePosition.y < _mapHeight;
        }

        private bool IsBorderTile(Tuple<Vector2Int, Tile> tile)
        {
            return tile.Item1.x == 0 || tile.Item1.x == _map.Width - 1
                   || tile.Item1.y == 0 || tile.Item1.y == _map.Height - 1;
        }
    }
}