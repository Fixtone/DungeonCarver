namespace TheDivineComedy
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
            //MapCreation.IMapCreationStrategy<Map> mapCreationStrategy = new MapCreation.BorderOnlyMapCreationStrategy<Map>(50, 50);
            //IMap map = Map.Create(mapCreationStrategy);
            
            MapCreation.IMapCreationStrategy<Map> mapCreationStrategy = new MapCreation.BSPTreeMapCreationStrategy<Map>(50, 50, 24, 15, 6);
            IMap map = Map.Create(mapCreationStrategy);

            for(int x = 0; x < map.Width; x ++)
            {
                for(int y = 0; y < map.Height; y ++)
                {
                    GameObject newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity,  dungeonParent);
                    newTile.GetComponent<SpriteRenderer>().sprite = map.GetTile(x, y).Item2.blocked ? wall : empty;
                }
            }

            Camera.main.transform.localPosition = new Vector3( map.Width/2, map.Height/2, -10);
        }
    }
}
