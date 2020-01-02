namespace DungeonCarver
{
    using UnityEngine;
    using System;

    public class DungeonCarver : MonoBehaviour
    {
        public Transform dungeonParent = null;
        public GameObject tilePrefab = null;
        public Sprite wall = null;
        public Sprite empty = null;

        // Start is called before the first frame update
        void Start()
        {
            System.Random random = new System.Random(DateTime.Now.Millisecond);
            //IMapGenerator<Map> mapCreationStrategy = new BorderOnlyMapGenerator<Map>(50, 50);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapCreationStrategy = new BSPTreeMapGenerator<Map>(50, 50, 24, 15, 6, random);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapCreationStrategy = new CaveMapGenerator<Map>(100, 100, 4, 50000, 45, 16, 500, 3, 4, 2, 10, 2, 5, 100000, random);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapcreationstrategy = new CellularAutomataMapGenerator<Map>(50, 50, 50, 3, 3, random);
            //IMap map = Map.Create(mapcreationstrategy);

            //IMapGenerator<Map> mapCreationStrategy = new CityMapGenerator<Map>(50, 50, 30, 16, 8, new Vector2Int(1, 1), random);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapCreationStrategy = new DFSMazeMapGenerator<Map>(50, 50, random);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapCreationStrategy = new DrunkardsWalkMapGenerator<Map>(80, 60, 0.3f, 50000, 0.15f, 0.7f, random);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapCreationStrategy = new TunnelingMazeMapGenerator<Map>(50, 50, random);
            //IMap map = Map.Create(mapCreationStrategy);

            IMapGenerator<Map> mapCreationStrategy = new TunnelingWithRoomsMapGenerator<Map>(80, 60, 30, 15, 6, random);
            IMap map = Map.Create(mapCreationStrategy);

            for (int x = 0; x < map.Width; x ++)
            {
                for(int y = 0; y < map.Height; y ++)
                {
                    GameObject newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity,  dungeonParent);
                    switch(map.GetTile(x, y).Tile.type)
                    {
                        case Tile.Type.Block:
                        {
                            newTile.GetComponent<SpriteRenderer>().sprite = wall;
                            break;
                        }
                        case Tile.Type.Empty:
                        {
                            newTile.GetComponent<SpriteRenderer>().sprite = empty;
                            break;
                        }
                    }
                }
            }

            Camera.main.transform.localPosition = new Vector3( map.Width/2, map.Height/2, -10);
        }
    }
}
