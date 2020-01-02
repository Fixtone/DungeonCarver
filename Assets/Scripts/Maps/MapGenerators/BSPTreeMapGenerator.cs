namespace DungeonCarver
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The BSPTreeMapGenerator creates a Map of the specified type by making an empty map with only the outermost border being solid walls
    /// </summary>
    /// <seealso href="http://www.roguebasin.com/index.php?title=Basic_BSP_Dungeon_generation">BSP Dungeon Generation</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class BSPTreeMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxLeafSize;
        private readonly int _minLeafSize;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;
        private System.Random _random;

        private T _map;
        private List<Leaf> _leafs = new List<Leaf>();
        
        public BSPTreeMapGenerator(int width, int height, int maxLeafSize, int minLeafSize, int roomMaxSize, int roomMinSize, System.Random random)
        {
            _width = width;
            _height = height;
            _maxLeafSize = maxLeafSize;
            _minLeafSize = minLeafSize;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _random = random;

            _map = new T();
        }

        public T CreateMap()
        {
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            Leaf rootLeaf = new Leaf(0, 0, _map.Width, _map.Height, _random);
            _leafs.Add(rootLeaf);

            bool splitSuccessfully = true;

            //Loop through all leaves until they can no longer split successfully
            while (splitSuccessfully)
            {
                splitSuccessfully = false;

                for (int i = 0; i < _leafs.Count; i++)
                {
                    if (_leafs[i].childLeafLeft == null && _leafs[i].childLeafRight == null)
                    {
                        if ((_leafs[i].leafWidth > _maxLeafSize) || (_leafs[i].leafHeight > _maxLeafSize))
                        {
                            //Try to split the leaf
                            if (_leafs[i].SplitLeaf(_minLeafSize))
                            {
                                _leafs.Add(_leafs[i].childLeafLeft);
                                _leafs.Add(_leafs[i].childLeafRight);
                                splitSuccessfully = true;
                            }
                        }
                    }
                }
            }

            rootLeaf.CreateRooms<T>(this, _maxLeafSize, _roomMaxSize, _roomMinSize);

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
            bool chance = Convert.ToBoolean(_random.Next(0, 2));
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
