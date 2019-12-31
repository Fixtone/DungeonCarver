namespace TheDivineComedy.MapCreation
{
    using System;
    using UnityEngine;

    public class Leaf
    {
        public const int MIN_LEAF_SIZE = 10;
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

        private readonly int _x;
        private readonly int _y;
        private Rect _room;
        private Rect _room1;
        private Rect _room2;

        public Leaf(int x, int y, int width, int height)
        {
            this.width = width;
            this.height = height;
            _x = x;
            _y = y;
        }

        public bool SplitLeaf()
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

            bool splitHorizontally = Convert.ToBoolean(UnityEngine.Random.Range(0, 1));

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
                max = height - MIN_LEAF_SIZE;
            }
            else
            {
                max = width - MIN_LEAF_SIZE;
            }

            if (max <= MIN_LEAF_SIZE)
            {
                return false;
            }

            int split = UnityEngine.Random.Range(MIN_LEAF_SIZE, max);

            if (splitHorizontally)
            {
                childLeft = new Leaf(_x, _y, width, split);
                childRight = new Leaf(_x, _y + split, width, height - split);
            }
            else
            {
                childLeft = new Leaf(_x, _y, split, height);
                childRight = new Leaf(_x + split, _y, width - split, height);
            }

            return true;
        }

        public void CreateRooms<T>(BSPTreeMapCreationStrategy<T> mapStrategy) where T : class, IMap, new()
        {
            if (childLeft != null || childRight != null)
            {
                //# recursively search for children until you hit the end of the branch
                if (childLeft != null)
                {
                    childLeft.CreateRooms(mapStrategy);
                }

                if (childRight != null)
                {
                    childRight.CreateRooms(mapStrategy);
                }

                if (childLeft != null && childRight != null)
                {
                    mapStrategy.createHall(childLeft.GetRoom(), childRight.GetRoom());
                }
            }
            else
            {
                int w = UnityEngine.Random.Range(mapStrategy.roomMinSize, Math.Min(mapStrategy.maxLeafSize, width - 1));
                int h = UnityEngine.Random.Range(mapStrategy.roomMinSize, Math.Min(mapStrategy.maxLeafSize, height - 1));
                int x = UnityEngine.Random.Range(_x, _x + (width - 1) - w);
                int y = UnityEngine.Random.Range(_y, _y + (height - 1) - h);

                _room = new Rect(x, y, w, h);

                mapStrategy.createRoom(_room);
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
            else if (Convert.ToBoolean(UnityEngine.Random.Range(0, 1)))
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
