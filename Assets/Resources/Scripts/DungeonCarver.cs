namespace DungeonCarver
{
    using UnityEngine;

    public class DungeonCarver : MonoBehaviour
    {
        public Transform dungeonParent = null;
        public GameObject tilePrefab = null;
        public Sprite wall = null;
        public Sprite empty = null;

        // Start is called before the first frame update
        void Start()
        {
            IMapGenerator<Map> mapCreationStrategy = new BorderOnlyMapGenerator<Map>(50, 50);
            IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapCreationStrategy = new BSPTreeMapGenerator<Map>(50, 50, 24, 15, 6);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapCreationStrategy = new CaveMapGenerator<Map>(100, 100, 4, 50000, 45, 16, 500, 3, 4, 2, 10, 2, 5, 100000);
            //IMap map = Map.Create(mapCreationStrategy);

            //IMapGenerator<Map> mapcreationstrategy = new CellularAutomataGenerator<Map>(50, 50, 50, 3, 3);
            //IMap map = Map.Create(mapcreationstrategy);

            for (int x = 0; x < map.Width; x ++)
            {
                for(int y = 0; y < map.Height; y ++)
                {
                    GameObject newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity,  dungeonParent);
                    switch(map.GetTile(x, y).Tile.type)
                    {
                        case Tile.Type.Wall:
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
