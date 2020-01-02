namespace DungeonCarver
{
    using System;
    using UnityEngine;

    public class Leaf
    {
        public int leafWidth
        {
            get; private set;
        }
        public int leafHeight
        {
            get; private set;
        }
        public Leaf childLeafLeft
        {
            get; private set;
        }
        public Leaf childLeafRight
        {
            get; private set;
        }

        private readonly System.Random _random;
        private readonly int _x;
        private readonly int _y;
        private Rect _room;
        private Rect _room1;
        private Rect _room2;

        public Leaf(int x, int y, int leafWidth, int leafHeight, System.Random random)
        {
            this.leafWidth = leafWidth;
            this.leafHeight = leafHeight;
            _x = x;
            _y = y;
            _random = random;
        }

        public bool SplitLeaf(int minLeafSize)
        {
            if (childLeafLeft != null || childLeafRight != null)
            {
                return false;
            }

            //==== Determine the direction of the split ====
            //If the leafWidth of the leaf is >25% larger than the leafHeight,
            //split the leaf vertically.
            //If the leafHeight of the leaf is >25 larger than the leafWidth,
            //split the leaf horizontally.
            //Otherwise, choose the direction at random.

            bool splitHorizontally = Convert.ToBoolean(_random.Next(0, 2));

            float hotizontalFactor = (float)leafWidth / leafHeight;
            float verticalFactor = (float)leafHeight / leafWidth;

            if (hotizontalFactor >= 1.25)
            {
                splitHorizontally = false;
            }
            else if (verticalFactor >= 1.25)
            {
                splitHorizontally = true;
            }

            int max = 0;
            if (splitHorizontally)
            {
                max = leafHeight - minLeafSize;
            }
            else
            {
                max = leafWidth - minLeafSize;
            }

            if (max <= minLeafSize)
            {
                return false;
            }

            int split = _random.Next(minLeafSize, max);

            if (splitHorizontally)
            {
                childLeafLeft = new Leaf(_x, _y, leafWidth, split, _random);
                childLeafRight = new Leaf(_x, _y + split, leafWidth, leafHeight - split, _random);
            }
            else
            {
                childLeafLeft = new Leaf(_x, _y, split, leafHeight, _random);
                childLeafRight = new Leaf(_x + split, _y, leafWidth - split, leafHeight, _random);
            }

            return true;
        }

        public void CreateRooms<T>(BSPTreeMapGenerator<T> mapGenerator, int maxLeafSize, int roomMaxSize, int roomMinSize) where T : class, IMap, new()
        {
            if (childLeafLeft != null || childLeafRight != null)
            {
                //# recursively search for children until you hit the end of the branch
                if (childLeafLeft != null)
                {
                    childLeafLeft.CreateRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childLeafRight != null)
                {
                    childLeafRight.CreateRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childLeafLeft != null && childLeafRight != null)
                {
                    mapGenerator.createHall(childLeafLeft.GetRoom(), childLeafRight.GetRoom());
                }
            }
            else
            {
                int w = _random.Next(roomMinSize, Math.Min(roomMaxSize, leafWidth - 1));
                int h = _random.Next(roomMinSize, Math.Min(roomMaxSize, leafHeight - 1));
                int x = _random.Next(_x, _x + (leafWidth - 1) - w);
                int y = _random.Next(_y, _y + (leafHeight - 1) - h);
            
                _room = new Rect(x, y, w, h);

                mapGenerator.createRoom(_room);
            }
        }

        public void CreateCityRooms<T>(CityMapGenerator<T> mapGenerator, int maxLeafSize, int roomMaxSize, int roomMinSize) where T : class, IMap, new()
        {
            if (childLeafLeft != null || childLeafRight != null)
            {
                //# recursively search for children until you hit the end of the branch
                if (childLeafLeft != null)
                {
                    childLeafLeft.CreateCityRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childLeafRight != null)
                {
                    childLeafRight.CreateCityRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childLeafLeft != null && childLeafRight != null)
                {
                    mapGenerator.createHall(childLeafLeft.GetRoom(), childLeafRight.GetRoom());
                }
            }
            else
            {
                int w = _random.Next(roomMinSize, Math.Min(roomMaxSize, leafWidth - 1));
                int h = _random.Next(roomMinSize, Math.Min(roomMaxSize, leafHeight - 1));
                int x = _random.Next(_x, _x + (leafWidth - 1) - w);
                int y = _random.Next(_y, _y + (leafHeight - 1) - h);
                
                _room = new Rect(x, y, w, h);

                mapGenerator.createRoom(_room);
            }
        }

        private Rect GetRoom()
        {
            if (_room != Rect.zero)
            {
                return _room;
            }
            else
            {
                if (childLeafLeft != null)
                {
                    _room1 = childLeafLeft.GetRoom();
                }

                if (childLeafRight != null)
                {
                    _room2 = childLeafRight.GetRoom();
                }
            }

            if (childLeafLeft == null && childLeafRight == null)
            {
                return Rect.zero;
            }
            else if (_room2 == Rect.zero)
            {
                return _room1;
            }
            else if (_room1 == Rect.zero)
            {
                return _room2;
            }
            else if (Convert.ToBoolean(_random.Next(0, 2)))
            {
                return _room1;
            }
            else
            {
                return _room2;
            }
        }
    }
}
