namespace DungeonCarver
{
    using System;
    using UnityEngine;

    public class DungeonCarver : MonoBehaviour
    {
        public Transform dungeonParent = null;
        public GameObject tilePrefab = null;
        public Sprite wall = null;
        public Sprite empty = null;

        //Generic Vars
        [HideInInspector]
        public Generators generator;
        [HideInInspector]
        public int mapWidth = 25;
        [HideInInspector]
        public int mapHeight = 25;

        //BSP Tree Specific Var
        [HideInInspector]
        public int maxLeafSize = 24;
        [HideInInspector]
        public int minLeafSize = 10;
        [HideInInspector]
        public int roomMaxSize = 15;
        [HideInInspector]
        public int roomMinSize = 6;

        //Cave System Specific Vars
        [HideInInspector]
        public int neighbours = 4;
        [HideInInspector]
        public int iterations = 50000;
        [HideInInspector]
        public int closeTileProb = 45;
        [HideInInspector]
        public int lowerLimit = 16;
        [HideInInspector]
        public int upperLimit = 500;
        [HideInInspector]
        public int emptyNeighbours = 3;
        [HideInInspector]
        public int emptyTileNeighbours = 4;
        [HideInInspector]
        public int corridorSpace = 2;
        [HideInInspector]
        public int corridorMaxTurns = 10;
        [HideInInspector]
        public int corridorMin = 2;
        [HideInInspector]
        public int corridorMax = 5;
        [HideInInspector]
        public int breakOut = 100000;

        //Cellullar Automata Vars
        [HideInInspector]
        public int fillProbability = 50;
        [HideInInspector]
        public int totalIterations = 3;
        [HideInInspector]
        public int cutoffOfBigAreaFill = 3;

        // City Vars
        [HideInInspector]
        public int maxCityLeafSize = 30;
        [HideInInspector]
        public int minCityLeafSize = 8;
        [HideInInspector]
        public int roomMaxCitySize = 16;
        [HideInInspector]
        public int roomMinCitySize = 8;

        //Drunkards Walk
        [HideInInspector]
        public float percentGoal = 0.3f;
        [HideInInspector]
        public int walkIterations = 50000;
        [HideInInspector]
        public float weightedTowardCenter = 0.15f;
        [HideInInspector]
        public float weightedTowardPreviousDirection = 0.7f;

        //Tunneling Maze With Rooms
        [HideInInspector]
        public int magicNumber = 666;

        //Tunneling With Rooms
        [HideInInspector]
        public int maxTunnelingRooms = 30;
        [HideInInspector]
        public int roomMaxTunnelingSize = 15;
        [HideInInspector]
        public int roomMinTunnelingSize = 6;

        private IMap _map;

        // Start is called before the first frame update
        private void Start()
        {
            System.Random random = new System.Random(DateTime.Now.Millisecond);
            IMapGenerator<Map> mapGenerator;

            switch (generator)
            {
                case Generators.BorderOnlyMapGenerator:
                    {
                        mapGenerator = new BorderOnlyMapGenerator<Map>(mapWidth, mapHeight);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.BSPTreeMapGenerator:
                    {
                        mapGenerator = new BSPTreeMapGenerator<Map>(mapWidth, mapHeight, maxLeafSize, minLeafSize, roomMaxSize, roomMinSize, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.CaveMapGenerator:
                    {
                        mapGenerator = new CaveMapGenerator<Map>(mapWidth, mapHeight, neighbours, iterations, closeTileProb, lowerLimit, upperLimit, emptyNeighbours, emptyTileNeighbours, corridorSpace, corridorMaxTurns, corridorMin, corridorMax, breakOut, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.CellularAutomataMapGenerator:
                    {
                        mapGenerator = new CellularAutomataMapGenerator<Map>(mapWidth, mapHeight, fillProbability, totalIterations, cutoffOfBigAreaFill, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.CityMapGenerator:
                    {
                        mapGenerator = new CityMapGenerator<Map>(mapWidth, mapHeight, maxCityLeafSize, minCityLeafSize, roomMaxCitySize, roomMinCitySize, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.DFSMazeMapGenerator:
                    {
                        mapGenerator = new DFSMazeMapGenerator<Map>(mapWidth, mapHeight, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.DrunkardsWalkMapGenerator:
                    {
                        mapGenerator = new DrunkardsWalkMapGenerator<Map>(mapWidth, mapHeight, percentGoal, walkIterations, weightedTowardCenter, weightedTowardPreviousDirection, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.TunnelingMazeMapGenerator:
                    {
                        mapGenerator = new TunnelingMazeMapGenerator<Map>(mapWidth, mapHeight, magicNumber, random);
                        _map = Map.Create(mapGenerator);
                        break;
                    }
                case Generators.TunnelingWithRoomsMapGenerator:
                    {
                        mapGenerator = new TunnelingWithRoomsMapGenerator<Map>(mapWidth, mapHeight, maxTunnelingRooms, roomMaxTunnelingSize, roomMinTunnelingSize, random);
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
                    switch (_map.GetTile(x, y).type)
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
