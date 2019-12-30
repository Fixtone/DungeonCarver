namespace TheDivineComedy.MapCreation
{
    using System;
    using UnityEngine;

    public class Leaf 
    {
        public const int MIN_LEAF_SIZE = 10;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Leaf ChildLeft { get; private set; }
        public Leaf ChildRight { get; private set; }

        private readonly int _x;
        private readonly int _y;
        private Rect _room;
        private Rect _room1;
        private Rect _room2;

        public Leaf(int x, int y, int width, int height)
        {
            this.Width = width;
            this.Height = height;
            _x = x;
            _y = y;            
        }

        public bool SplitLeaf()
        {
            if (ChildLeft != null ||  ChildRight != null)
            {
                return false;
            }

		    //==== Determine the direction of the split ====
		    //If the width of the leaf is >25% larger than the height,
		    //split the leaf vertically.
		    //If the height of the leaf is >25 larger than the width,
		    //split the leaf horizontally.
		    //Otherwise, choose the direction at random.

            bool splitHorizontally =  Convert.ToBoolean(UnityEngine.Random.Range(0,1));

            float hotizontalFactor = (float) Width/Height;
            float verticalFactor =(float) Height/Width;

            if (hotizontalFactor >= 1.25)
            {
			    splitHorizontally = false;
            }
		    else if(verticalFactor >= 1.25)
            {
			    splitHorizontally = true;
            }

            int max = 0;
		    if (splitHorizontally)
            { 
			    max = Height - MIN_LEAF_SIZE;
            }
		    else
            { 
			    max = Width - MIN_LEAF_SIZE;
            }

            if (max <= MIN_LEAF_SIZE)
            {
			    return false;
            }

            int split = UnityEngine.Random.Range(MIN_LEAF_SIZE,max);

		    if (splitHorizontally)
            { 
			    ChildLeft = new Leaf(_x, _y, Width, split);
			    ChildRight = new Leaf(_x, _y+split, Width, Height-split);
            }
		    else
            {
                ChildLeft = new Leaf(_x, _y, split, Height);
			    ChildRight = new Leaf(_x + split, _y, Width-split, Height);
            } 

            return true;
        }

        public void CreateRooms<T>(BSPTreeMapCreationStrategy<T> mapStrategy) where T : class, IMap, new()
        {
            if (ChildLeft != null || ChildRight != null)
            {
                //# recursively search for children until you hit the end of the branch
                if (ChildLeft != null)
                {
                    ChildLeft.CreateRooms(mapStrategy);
                }

                if (ChildRight != null)
                {
                    ChildRight.CreateRooms(mapStrategy);
                }

                if (ChildLeft != null && ChildRight != null)
                {                    
                    mapStrategy.createHall(ChildLeft.GetRoom(), ChildRight.GetRoom());
                }
            }            
            else
            {
                int w = UnityEngine.Random.Range(mapStrategy.RoomMinSize, Math.Min(mapStrategy.MaxLeafSize, Width-1));
                int h = UnityEngine.Random.Range(mapStrategy.RoomMinSize, Math.Min(mapStrategy.MaxLeafSize, Height-1));
                int x = UnityEngine.Random.Range(_x, _x+(Width-1)-w);
                int y = UnityEngine.Random.Range(_y, _y+(Height-1)-h);

                _room = new Rect(x,y,w,h);

                mapStrategy.createRoom(_room);
            }
        }       
		
	    private Rect GetRoom()
        {
            if(_room != Rect.zero)
            {
                return _room;
            }
            else
            {
                if (ChildLeft != null)
                {
                    _room1 = ChildLeft.GetRoom();
                }
				
			    if (ChildRight != null)
                { 
				    _room2 = ChildRight.GetRoom();
                }
            }

            if (ChildLeft == null &&  ChildRight == null)
            {
                return Rect.zero;
            }
            else if(_room2 == Rect.zero)
            {
                return _room1;
            }
            else if(_room1 == Rect.zero)
            {
                return _room2;
            }
            else if(Convert.ToBoolean(UnityEngine.Random.Range(0,1)))
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
