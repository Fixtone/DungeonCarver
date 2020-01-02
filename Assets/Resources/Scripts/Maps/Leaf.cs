namespace DungeonCarver
{
    using System;
    using UnityEngine;

    public class Leaf
    {
        public int width
        {
            get; private set;
        }
        public int height
        {
            get; private set;
        }
        public Leaf childLeft
        {
            get; private set;
        }
        public Leaf childRight
        {
            get; private set;
        }

        private readonly System.Random _random;
        private readonly int _x;
        private readonly int _y;
        private Rect _room;
        private Rect _room1;
        private Rect _room2;

        public Leaf(int x, int y, int width, int height, System.Random random)
        {
            this.width = width;
            this.height = height;
            _x = x;
            _y = y;
            _random = random;
        }

        public bool SplitLeaf(int minLeafSize)
        {
            if (childLeft != null || childRight != null)
            {
                return false;
            }

            //==== Determine the direction of the split ====
            //If the width of the leaf is >25% larger than the height,
            //split the leaf vertically.
            //If the height of the leaf is >25 larger than the width,
            //split the leaf horizontally.
            //Otherwise, choose the direction at random.

            bool splitHorizontally = Convert.ToBoolean(_random.Next(0, 2));

            float hotizontalFactor = (float)width / height;
            float verticalFactor = (float)height / width;

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
                max = height - minLeafSize;
            }
            else
            {
                max = width - minLeafSize;
            }

            if (max <= minLeafSize)
            {
                return false;
            }

            int split = _random.Next(minLeafSize, max);

            if (splitHorizontally)
            {
                childLeft = new Leaf(_x, _y, width, split, _random);
                childRight = new Leaf(_x, _y + split, width, height - split, _random);
            }
            else
            {
                childLeft = new Leaf(_x, _y, split, height, _random);
                childRight = new Leaf(_x + split, _y, width - split, height, _random);
            }

            return true;
        }

        public void CreateRooms<T>(BSPTreeMapGenerator<T> mapGenerator, int maxLeafSize, int roomMaxSize, int roomMinSize) where T : class, IMap, new()
        {
            if (childLeft != null || childRight != null)
            {
                //# recursively search for children until you hit the end of the branch
                if (childLeft != null)
                {
                    childLeft.CreateRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childRight != null)
                {
                    childRight.CreateRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childLeft != null && childRight != null)
                {
                    mapGenerator.createHall(childLeft.GetRoom(), childRight.GetRoom());
                }
            }
            else
            {
                int w = UnityEngine.Random.Range(roomMinSize, Math.Min(roomMaxSize, width - 1));
                int h = UnityEngine.Random.Range(roomMinSize, Math.Min(roomMaxSize, height - 1));
                int x = UnityEngine.Random.Range(_x, _x + (width - 1) - w);
                int y = UnityEngine.Random.Range(_y, _y + (height - 1) - h);
            
                _room = new Rect(x, y, w, h);

                mapGenerator.createRoom(_room);
            }
        }

        public void CreateCityRooms<T>(CityMapGenerator<T> mapGenerator, int maxLeafSize, int roomMaxSize, int roomMinSize) where T : class, IMap, new()
        {
            if (childLeft != null || childRight != null)
            {
                //# recursively search for children until you hit the end of the branch
                if (childLeft != null)
                {
                    childLeft.CreateCityRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childRight != null)
                {
                    childRight.CreateCityRooms(mapGenerator, maxLeafSize, roomMaxSize, roomMinSize);
                }

                if (childLeft != null && childRight != null)
                {
                    mapGenerator.createHall(childLeft.GetRoom(), childRight.GetRoom());
                }
            }
            else
            {
                int w = UnityEngine.Random.Range(roomMinSize, Math.Min(roomMaxSize, width - 1));
                int h = UnityEngine.Random.Range(roomMinSize, Math.Min(roomMaxSize, height - 1));
                int x = UnityEngine.Random.Range(_x, _x + (width - 1) - w);
                int y = UnityEngine.Random.Range(_y, _y + (height - 1) - h);
                
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
                if (childLeft != null)
                {
                    _room1 = childLeft.GetRoom();
                }

                if (childRight != null)
                {
                    _room2 = childRight.GetRoom();
                }
            }

            if (childLeft == null && childRight == null)
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
            else if (Convert.ToBoolean(_random.Next(0, 1)))
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
