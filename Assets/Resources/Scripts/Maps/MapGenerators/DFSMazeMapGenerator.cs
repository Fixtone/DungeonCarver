namespace DungeonCarver
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The City Walls algorithm is very similar to the BSP Tree above. In fact their main difference is in how they generate rooms after the actual tree has been created. Instead of 
	/// starting with an array of solid walls and carving out rooms connected by tunnels, the City Walls generator starts with an array of floor tiles, then creates only the
	/// exterior of the rooms, then opens one wall for a door.	
    /// </summary>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class DFSMazeMapGenerator  <T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private System.Random _rand = new System.Random();

        private int _width;
        private int _height;
        private System.Random _random;

        private T _map;

        /// <summary>
        /// Constructs a new BorderOnlyMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="size">The size of the Map to be created</param>        
        public DFSMazeMapGenerator (int mapWidth, int mapHeight, System.Random random)
        {
            _width = mapWidth;
            _height = mapHeight;
            _random = random;
        }

        /// <summary>
        /// Creates a Map of the specified type by making an empty map with only the outermost border being solid walls
        /// </summary>
        /// <returns>An IMap of the specified type</returns>
        public T CreateMap()
        {
            _map = new T();
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            CreateMaze(1, 1);

            return _map;
        } 

        private void CreateMaze(int i, int j)
        {            
            List<int> visitOrder = new List<int> {0,1,2,3};

            //out of boundary
            if(i < 1 || j < 1 || i >= _width - 1 || j >= _height - 1)
            { 
                return ;
            }

            //visited, go back the the coming direction, return 
            if(!_map.GetTile(i, j).Tile.type.Equals(Tile.Type.Block))
            { 
                return ;
            }

            //some neightbors are visited in addition to the coming direction, return
            //this is to avoid circles in maze
            if(CountVisitedNeighbor(i, j) > 1)
            { 
                return ;
            }

            _map.SetTile(i, j, new Tile(Tile.Type.Empty));
            
            //shuffle the visitOrder
            Shuffle<int>(visitOrder);
            for (int k = 0; k < 4; ++k)
            {
                int ni = i + MapUtils.FourDirections[visitOrder[k]].x;
                int nj = j + MapUtils.FourDirections[visitOrder[k]].y;
                CreateMaze(ni, nj);
            }
        }

        private int CountVisitedNeighbor( int i, int j)
        {
            int count = 0;

            foreach(Vector2Int direction in MapUtils.FourDirections)
            {
                int ni = i + direction.x;
                int nj = j + direction.y;
                if(ni < 0 || nj < 0 || ni >= _width || nj >= _height)
                {
                    continue;
                }
                
                //visited
                if(!_map.GetTile(ni, nj).Tile.type.Equals(Tile.Type.Block))
                {
                    count++;
                }
            }

            return count;
        }        

        private void Shuffle<E>(IList<E> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = _rand.Next(n + 1);  
                E value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }

}