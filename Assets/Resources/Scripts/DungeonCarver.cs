namespace DungeonCarver
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class DungeonCarver : MonoBehaviour
    {
        public enum Generators
        {
            BorderOnlyMapGenerator,
            BSPTreeMapGenerator,
            CaveMapGenerator,
            CellularAutomataMapGenerator,
            CityMapGenerator,
            DFSMazeMapGenerator,
            DrunkardsWalkMapGenerator,
            TunnelingMazeMapGenerator,
            TunnelingWithRoomsMapGenerator
        };

        public Generators selectedGenerator = Generators.BorderOnlyMapGenerator;

        public Transform dungeonParent = null;
        public GameObject tilePrefab = null;
        public Sprite wall = null;
        public Sprite empty = null;

        private IMap _map;

        // Start is called before the first frame update
        private void Start()
        {
            System.Random random = new System.Random(DateTime.Now.Millisecond);
            IMapGenerator<Map> mapGenerator;

            switch (selectedGenerator)
            {
                case Generators.BorderOnlyMapGenerator:
                    {
                        mapGenerator = new BorderOnlyMapGenerator<Map>(50, 50);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.BSPTreeMapGenerator:
                    {
                        mapGenerator = new BSPTreeMapGenerator<Map>(50, 50, 24, 15, 6, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.CaveMapGenerator:
                    {
                        mapGenerator = new CaveMapGenerator<Map>(100, 100, 4, 50000, 45, 16, 500, 3, 4, 2, 10, 2, 5, 100000, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.CellularAutomataMapGenerator:
                    {
                        mapGenerator = new CellularAutomataMapGenerator<Map>(50, 50, 50, 3, 3, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.CityMapGenerator:
                    {
                        mapGenerator = new CityMapGenerator<Map>(50, 50, 30, 16, 8, new Vector2Int(1, 1), random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.DFSMazeMapGenerator:
                    {
                        mapGenerator = new DFSMazeMapGenerator<Map>(50, 50, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.DrunkardsWalkMapGenerator:
                    {
                        mapGenerator = new DrunkardsWalkMapGenerator<Map>(80, 60, 0.3f, 50000, 0.15f, 0.7f, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.TunnelingMazeMapGenerator:
                    {
                        mapGenerator = new TunnelingMazeMapGenerator<Map>(50, 50, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.TunnelingWithRoomsMapGenerator:
                    {
                        mapGenerator = new TunnelingWithRoomsMapGenerator<Map>(80, 60, 30, 15, 6, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
            }


            Camera.main.transform.localPosition = new Vector3(_map.Width / 2, _map.Height / 2, -10);

            RenderMap();
        }

        private void RenderMap()
        {
            for (int x = 0; x < _map.Width; x++)
            {
                for (int y = 0; y < _map.Height; y++)
                {
                    GameObject newTile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, dungeonParent);
                    switch (_map.GetTile(x, y).Tile.type)
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
        }
    }
}
