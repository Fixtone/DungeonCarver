namespace DungeonCarver
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// The CaveMapGenerator creates a Map of the specified type by using a complex cave system for generated map.
    /// </summary>
    /// <seealso href="https://www.evilscience.co.uk/a-c-algorithm-to-build-roguelike-cave-systems-part-1/">An implementation of cave system</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class CaveMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
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
        private readonly System.Random _random;

        private List<List<Vector2Int>> _caves;
        private List<Vector2Int> _corridors;

        private T _map;
        
        public CaveMapGenerator(int width, int height, int neighbours, int iterations, int closeTileProb, int lowerLimit, int upperLimit, int emptyNeighbours,
                                       int emptyTileNeighbours, int corridorSpace, int corridorMaxTurns, int corridorMin, int corridorMax, int breakOut, System.Random random)
        {
            _width = width;
            _height = height;
            _neighbours = neighbours;
            _iterations = iterations;
            _closeTileProb = closeTileProb;
            _lowerLimit = lowerLimit;
            _upperLimit = upperLimit;
            _emptyNeighbours = emptyNeighbours;
            _emptyTileNeighbours = emptyTileNeighbours;
            _corridorSpace = corridorSpace;
            _corridor_MaxTurns = corridorMaxTurns;
            _corridor_Min = corridorMin;
            _corridor_Max = corridorMax;
            _breakOut = breakOut;
            _random = random;

            _map = new T();
        }

        public T CreateMap()
        {
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            BuildCaves();
            GetCaves();
            ConnectCaves();

            return _map;
        }

        private void BuildCaves()
        {
            foreach ((Vector2Int tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                if (_random.Next(0, 100) < _closeTileProb)
                {
                    _map.SetTile(tileData.tilePosition.x, tileData.tilePosition.y, new Tile(Tile.Type.Empty));
                }
            }

            Vector2Int tilePosition;

            //Pick cells at random
            for (int x = 0; x <= _iterations; x++)
            {
                tilePosition = new Vector2Int(_random.Next(0, _width), _random.Next(0, _height));

                //if the randomly selected cell has more closed neighbours than the property Neighbours
                //set it closed, else open it
                if (NeighboursGetNineDirections(tilePosition).Where(n => !_map.GetTile(n.x, n.y).type.Equals(Tile.Type.Block)).Count() > _neighbours)
                {
                    _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Empty));
                }
                else
                {
                    _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Block));
                }
            }

            //
            //  Smooth of the rough cave edges and any single blocks by making several 
            //  passes on the map and removing any cells with 3 or more empty neighbours
            //
            for (int ctr = 0; ctr < 5; ctr++)
            {
                //examine each cell individually
                foreach ((Vector2Int tilePosition, Tile tile) tileData in _map.GetAllTiles())
                {
                    if (_map.IsBorderTile(tileData.tilePosition))
                    {
                        continue;
                    }

                    tilePosition = tileData.tilePosition;

                    if (!_map.GetTile(tilePosition.x, tilePosition.y).type.Equals(Tile.Type.Block) && NeighboursGetFourDirections(tilePosition).Where(position => _map.GetTile(position.x, position.y).type.Equals(Tile.Type.Block)).Count() >= _emptyNeighbours)
                    {
                        _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Block));
                    }
                }
            }

            //
            //  fill in any empty cells that have 4 full neighbours
            //  to get rid of any holes in an cave
            //
            foreach ((Vector2Int tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                tilePosition = tileData.tilePosition;

                if (_map.GetTile(tilePosition.x, tilePosition.y).type.Equals(Tile.Type.Block) && NeighboursGetFourDirections(tilePosition).Where(postion => !_map.GetTile(postion.x, postion.y).type.Equals(Tile.Type.Block)).Count() >= _emptyTileNeighbours)
                {
                    _map.SetTile(tilePosition.x, tilePosition.y, new Tile(Tile.Type.Empty));
                }
            }
        }
        
        private void GetCaves()
        {
            _caves = new List<List<Vector2Int>>();

            List<Vector2Int> cave;
            Vector2Int tilePosition;

            //examine each cell in the map...
            foreach ((Vector2Int tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                tilePosition = tileData.tilePosition;

                //if the cell is closed, and that cell doesn't occur in the list of caves..
                if (!_map.GetTile(tilePosition.x, tilePosition.y).type.Equals(Tile.Type.Block) && _caves.Count(s => s.Contains(tilePosition)) == 0)
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
                            _map.SetTile(p.x, p.y, new Tile(Tile.Type.Block));
                        }
                    }
                    else
                    {
                        _caves.Add(cave);
                    }
                }
            }

        }

        private void LocateCave(Vector2Int tilePosition, List<Vector2Int> cave)
        {
            foreach (Vector2Int p in NeighboursGetFourDirections(tilePosition).Where(n => !_map.GetTile(n.x, n.y).type.Equals(Tile.Type.Block)))
            {
                if (!cave.Contains(p))
                {
                    cave.Add(p);
                    LocateCave(p, cave);
                }
            }
        }
        
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
            currentcave = _caves[_random.Next(0, _caves.Count())];
            ConnectedCaves.Add(currentcave);
            _caves.Remove(currentcave);

            //starting builder
            do
            {

                //no corridors are present, sp build off a cave
                if (_corridors.Count() == 0)
                {
                    currentcave = ConnectedCaves[_random.Next(0, ConnectedCaves.Count())];
                    CaveGetEdge(currentcave, ref cor_point, ref cor_direction);
                }
                else
                {
                    //corridors are presnt, so randomly chose whether a get a start
                    //point from a corridor or cave
                    if (_random.Next(0, 100) > 50)
                    {
                        currentcave = ConnectedCaves[_random.Next(0, ConnectedCaves.Count())];
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

        private void CaveGetEdge(List<Vector2Int> pCave, ref Vector2Int pCavePoint, ref Vector2Int pDirection)
        {
            do
            {
                //random point in cave
                pCavePoint = pCave.ToList()[_random.Next(0, pCave.Count())];

                pDirection = FourDirectiosnGet(pDirection);

                do
                {
                    pCavePoint += (pDirection);

                    if (!TilePositionCheck(pCavePoint))
                    {
                        break;
                    }
                    else if (_map.GetTile(pCavePoint.x, pCavePoint.y).type.Equals(Tile.Type.Block))
                    {
                        return;
                    }

                } while (true);
            } while (true);
        }
        
        private void CorridorGetEdge(ref Vector2Int pLocation, ref Vector2Int pDirection)
        {
            List<Vector2Int> validdirections = new List<Vector2Int>();

            do
            {
                //the modifiers below prevent the first of last point being chosen
                pLocation = _corridors[_random.Next(1, _corridors.Count)];

                //attempt to locate all the empy map points around the location
                //using the directions to offset the randomly chosen point
                foreach (Vector2Int p in MapUtils.FourDirections)
                {
                    if (TilePositionCheck(new Vector2Int(pLocation.x + p.x, pLocation.y + p.y)))
                    {
                        if (_map.GetTile(pLocation.x + p.x, pLocation.y + p.y).type.Equals(Tile.Type.Block))
                        {
                            validdirections.Add(p);
                        }
                    }
                }

            } while (validdirections.Count == 0);

            pDirection = validdirections[_random.Next(0, validdirections.Count)];
            pLocation += pDirection;
        }

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

                corridorlength = _random.Next(_corridor_Min, _corridor_Max);
                //build corridor
                while (corridorlength > 0)
                {
                    corridorlength--;

                    //make a point and offset it
                    pStart += pDirection;

                    if (TilePositionCheck(pStart) && !_map.GetTile(pStart.x, pStart.y).type.Equals(Tile.Type.Block))
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
                        if (!_map.GetTile(pPoint.x + r, pPoint.y).type.Equals(Tile.Type.Block))
                        {
                            return false;
                        }
                    }
                }
                else if (pDirection.y == 0)//east west
                {
                    if (TilePositionCheck(new Vector2Int(pPoint.x, pPoint.y + r)))
                    {
                        if (!_map.GetTile(pPoint.x, pPoint.y + r).type.Equals(Tile.Type.Block))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        
        private Vector2Int FourDirectiosnGet(Vector2Int p)
        {
            Vector2Int newdir;
            do
            {

                newdir = MapUtils.FourDirections[_random.Next(0, MapUtils.FourDirections.Count())];

            } while (newdir.x != -p.x & newdir.y != -p.y);

            return newdir;
        }
        
        private Vector2Int FourDirectiosnGet(Vector2Int pDir, Vector2Int pDirExclude)
        {
            Vector2Int NewDir;
            do
            {

                NewDir = MapUtils.FourDirections[_random.Next(0, MapUtils.FourDirections.Count())];

            } while (DirectionReverse(NewDir) == pDir | DirectionReverse(NewDir) == pDirExclude);


            return NewDir;
        }
       
        private Vector2Int DirectionReverse(Vector2Int pDir)
        {
            return new Vector2Int(-pDir.x, -pDir.y);
        }
        
        private List<Vector2Int> NeighboursGetFourDirections(Vector2Int tilePosition)
        {
            return MapUtils.FourDirections.Select(direction => new Vector2Int(tilePosition.x + direction.x, tilePosition.y + direction.y)).Where(direction => TilePositionCheck(direction)).ToList();
        }
        
        private List<Vector2Int> NeighboursGetNineDirections(Vector2Int tilePosition)
        {
            return MapUtils.NineDirections.Select(direction => new Vector2Int(tilePosition.x + direction.x, tilePosition.y + direction.y)).Where(direction => TilePositionCheck(direction)).ToList();
        }
        
        private bool TilePositionCheck(Vector2Int tilePosition)
        {
            return tilePosition.x >= 0 & tilePosition.x < _width & tilePosition.y >= 0 & tilePosition.y < _height;
        }
    }
}