using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheDivineComedy.MapCreation
{
    /// <summary>
    /// The CaveMapCreationStrategy creates a Map of the specified type by using a cellular automata algorithm for creating a cave-like map.
    /// </summary>
    /// <seealso href="http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels">Cellular Automata Method from RogueBasin</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class BSPTreeMapCreationStrategy<T> : IMapCreationStrategy<T> where T : class, IMap, new()
    {
        public int Width
        {
            get; private set;
        }
        public int Height
        {
            get; private set;
        }
        public int MaxLeafSize
        {
            get; private set;
        }
        public int RoomMaxSize
        {
            get; private set;
        }
        public int RoomMinSize
        {
            get; private set;
        }

        private T _map;
        private List<Leaf> _leafs = new List<Leaf>();

        /// <summary>
        /// Constructs a new BSPTreeMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="width">The width of the Map to be created</param>
        /// <param name="height">The height of the Map to be created</param>
        /// <param name="maxRooms">The maximum number of rooms that will exist in the generated Map</param>
        /// <param name="roomMaxSize">The maximum width and height of each room that will be generated in the Map</param>
        /// <param name="roomMinSize">The minimum width and height of each room that will be generated in the Map</param>
        public BSPTreeMapCreationStrategy(int width, int height, int maxLeafSize, int roomMaxSize, int roomMinSize)
        {
            Width = width;
            Height = height;
            MaxLeafSize = maxLeafSize;
            RoomMaxSize = roomMaxSize;
            RoomMinSize = roomMinSize;

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
            _map.Initialize(Width, Height);
            _map.Clear(new Tile(Tile.Type.Wall));

            Leaf rootLeaf = new Leaf(0, 0, _map.Width, _map.Height);
            _leafs.Add(rootLeaf);

            bool splitSuccessfully = true;

            //Loop through all leaves until they can no longer split successfully
            while (splitSuccessfully)
            {
                splitSuccessfully = false;

                for (int i = 0; i < _leafs.Count; i++)
                {
                    if (_leafs[i].ChildLeft == null && _leafs[i].ChildRight == null)
                    {
                        if ((_leafs[i].Width > MaxLeafSize) || (_leafs[i].Height > MaxLeafSize))
                        {
                            //Try to split the leaf
                            if (_leafs[i].SplitLeaf())
                            {
                                _leafs.Add(_leafs[i].ChildLeft);
                                _leafs.Add(_leafs[i].ChildRight);
                                splitSuccessfully = true;
                            }
                        }
                    }
                }
            }

            rootLeaf.CreateRooms<T>(this);

            return _map;
        }

        public void createRoom(Rect room)
        {
            for (int x = (int)room.x + 1; x < room.max.x; x++)
            {
                for (int y = (int)room.y + 1; y < room.max.y; y++)
                {
                    _map.SetTile(x, y, new Tile(Tile.Type.Empty));
                }
            }
        }

        public void createHall(Rect room1, Rect room2)
        {
            //# connect two rooms by hallways
            Vector2Int room1Center = Vector2Int.CeilToInt(room1.center);
            Vector2Int room2Center = Vector2Int.CeilToInt(room2.center);

            //# 50% chance that a tunnel will start horizontally
            bool chance = Convert.ToBoolean(UnityEngine.Random.Range(0, 1));
            if (chance)
            {
                MakeHorizontalTunnel(room1Center.x, room2Center.x, room1Center.y);
                MakeVerticalTunnel(room1Center.y, room2Center.y, room2Center.x);
            }
            else
            {
                MakeVerticalTunnel(room1Center.y, room2Center.y, room1Center.x);
                MakeHorizontalTunnel(room1Center.x, room2Center.x, room2Center.y);
            }
        }

        private void MakeHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _map.SetTile(x, yPosition, new Tile(Tile.Type.Empty));
            }
        }

        private void MakeVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _map.SetTile(xPosition, y, new Tile(Tile.Type.Empty));
            }
        }
    }
}
