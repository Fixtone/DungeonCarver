namespace DungeonCarver
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The City Walls algorithm is very similar to the BSP Tree above. In fact their main difference is in how they generate rooms after the actual tree has been created. Instead of 
	/// starting with an array of solid walls and carving out rooms connected by tunnels, the City Walls generator starts with an array of floor tiles, then creates only the
	/// exterior of the rooms, then opens one wall for a door.	
    /// </summary>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class CityMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        public int maxLeafSize
        {
            get; private set;
        }
        public int roomMaxSize
        {
            get; private set;
        }
        public int roomMinSize
        {
            get; private set;
        }

        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly Vector2Int _inset;
        private readonly System.Random _random;

        private List<Leaf> _leafs = new List<Leaf>();
        private List<Rect> _rooms = new List<Rect>();
        private Rect _room;
        private T _map;

        /// <summary>
        /// Constructs a new BorderOnlyMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="size">The size of the Map to be created</param>        
        public CityMapGenerator(int mapWidth, int mapHeight, int maxLeafSize, int roomMaxSize, int roomMinSize, Vector2Int inset, System.Random random)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _inset = inset;
            _random = random;

            this.maxLeafSize = maxLeafSize;
            this.roomMaxSize = roomMaxSize;
            this.roomMinSize = roomMinSize;            
        }

        /// <summary>
        /// Creates a Map of the specified type by making an empty map with only the outermost border being solid walls
        /// </summary>
        /// <returns>An IMap of the specified type</returns>
        public T CreateMap()
        {
            _map = new T();
            _map.Initialize(_mapWidth, _mapHeight);
            _map.Clear(new Tile(Tile.Type.Empty));

            _leafs = new List<Leaf>();

            Leaf rootLeaf = new Leaf(_inset.x, _inset.y, _mapWidth - _inset.x, _mapHeight - _inset.y, _random);

            _leafs.Add(rootLeaf);

            bool splitSuccessfully = true;

            while (splitSuccessfully)
            {
                splitSuccessfully = false;

                for(int i = 0; i< _leafs.Count; i++)
                {
                    if ((_leafs[i].childLeft == null) && (_leafs[i].childRight == null))
                    {
                        if((_leafs[i].width > maxLeafSize) || (_leafs[i].height > maxLeafSize) || (UnityEngine.Random.Range(0.0f, 1.0f) > 0.8f))
                        { 
                            //Try to split the leaf
                            if(_leafs[i].SplitLeaf())
                            {
                                _leafs.Add(_leafs[i].childLeft);
                                _leafs.Add(_leafs[i].childRight);
                                splitSuccessfully = true;
                            }
                        }
                    }
                }
            }

            rootLeaf.CreateCityRooms<T>(this);
            CreateDoors();

            return _map;
        }

        public void createRoom(Rect room)
        {
            _rooms.Add(room);

            //Build Walls
		    //set all tiles within a rectangle to 1
            for(int x = (int)room.x; x <= room.max.x; x++)
            {
                for(int y = (int)room.y; y <= room.max.y; y++)
                {
                     _map.SetTile(x, y, new Tile(Tile.Type.Block));
                }
            }

            for(int x = (int)room.x+1; x < room.max.x; x++)
            {
                for(int y = (int)room.y+1; y < room.max.y; y++)
                {
                     _map.SetTile(x, y, new Tile(Tile.Type.Empty));
                }
            }
        }

        public void createHall(Rect room1, Rect room2)
        {
            //# This method actually creates a list of rooms,
            //# but since it is called from an outside class that is also
            //# used by other dungeon Generators, it was simpler to 
            //# repurpose the createHall method that to alter the leaf class.

            
            if(_rooms.Find(item => item.Equals(room1)) == null)
            {
                _rooms.Add(room1);
            }

            if(_rooms.Find(item => item.Equals(room2)) == null)
            {
                _rooms.Add(room2);
            }
        }

        public void CreateDoors()
        {
            foreach(Rect room in _rooms)
            {
                Vector2Int roomCenter = Vector2Int.CeilToInt(room.center);
                Array values = Enum.GetValues(typeof(MapUtils.CardinalFourDirections));

                MapUtils.CardinalFourDirections randomDirection = (MapUtils.CardinalFourDirections)values.GetValue(UnityEngine.Random.Range(0, values.Length));
                
                Vector2Int doorPosition = Vector2Int.zero;
                switch(randomDirection)
                {
                    case MapUtils.CardinalFourDirections.NORTH:
                        {
                            doorPosition = new Vector2Int(roomCenter.x, (int)room.max.y);
                            break;
                        }
                        case MapUtils.CardinalFourDirections.SOUTH:
                        {
                            doorPosition = new Vector2Int(roomCenter.x, (int)room.min.y);
                            break;
                        }
                        case MapUtils.CardinalFourDirections.EAST:
                        {
                            doorPosition = new Vector2Int((int)room.min.x, roomCenter.y);
                            break;
                        }
                        case MapUtils.CardinalFourDirections.WEST:
                        {
                            doorPosition = new Vector2Int((int)room.max.x, roomCenter.y);
                            break;
                        }
                }

                _map.SetTile(doorPosition.x, doorPosition.y, new Tile(Tile.Type.Empty));                
            }
        } 
    }
}