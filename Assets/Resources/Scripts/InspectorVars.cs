using UnityEngine;
using UnityEditor;
namespace DungeonCarver
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
    }

    [CustomEditor(typeof(DungeonCarver))]
    public class InspectorVars : Editor
    {

        private DungeonCarver _script;

        void OnEnable()
        {
            _script = target as DungeonCarver;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            _script.generator = (Generators)EditorGUILayout.EnumPopup("Generators:", _script.generator);

            _script.mapWidth = EditorGUILayout.IntField("Map Width:", _script.mapWidth);
            _script.mapHeight = EditorGUILayout.IntField("Map Height:", _script.mapHeight);

            switch (_script.generator)
            {
                case Generators.BSPTreeMapGenerator:
                    {
                        _script.maxLeafSize = EditorGUILayout.IntField("Max Leaf Size:", _script.maxLeafSize);
                        _script.minLeafSize = EditorGUILayout.IntField("Min Leaf Size:", _script.minLeafSize);
                        _script.roomMaxSize = EditorGUILayout.IntField("Room Max Size:", _script.roomMaxSize);
                        _script.roomMinSize = EditorGUILayout.IntField("Room Min Size:", _script.roomMinSize);
                        break;
                    }
                case Generators.CaveMapGenerator:
                    {


                        _script.neighbours = EditorGUILayout.IntField("Neighbours:", _script.neighbours);
                        _script.iterations = EditorGUILayout.IntField("Iterations:", _script.iterations);
                        _script.closeTileProb = EditorGUILayout.IntField("CloseTileProb:", _script.closeTileProb);
                        _script.lowerLimit = EditorGUILayout.IntField("Lower Limit:", _script.lowerLimit);
                        _script.upperLimit = EditorGUILayout.IntField("Upper Limit:", _script.upperLimit);
                        _script.emptyNeighbours = EditorGUILayout.IntField("Empty Neighbours:", _script.emptyNeighbours);
                        _script.emptyTileNeighbours = EditorGUILayout.IntField("Empty Tile Neighbours:", _script.emptyTileNeighbours);
                        _script.corridorSpace = EditorGUILayout.IntField("Corridor Space:", _script.corridorSpace);
                        _script.corridorMaxTurns = EditorGUILayout.IntField("Corridor Max Turns:", _script.corridorMaxTurns);
                        _script.corridorMin = EditorGUILayout.IntField("Corridor Min:", _script.corridorMin);
                        _script.corridorMax = EditorGUILayout.IntField("Corridor Max:", _script.corridorMax);
                        _script.breakOut = EditorGUILayout.IntField("BreakOut:", _script.breakOut);

                        break;
                    }
                case Generators.CellularAutomataMapGenerator:
                    {
                        _script.fillProbability = EditorGUILayout.IntField("Fill Probability:", _script.fillProbability);
                        _script.totalIterations = EditorGUILayout.IntField("Total Iterations:", _script.totalIterations);
                        _script.cutoffOfBigAreaFill = EditorGUILayout.IntField("Cutoff Of Big Area Fill:", _script.cutoffOfBigAreaFill);
                        break;
                    }
                case Generators.CityMapGenerator:
                    {
                        _script.maxCityLeafSize = EditorGUILayout.IntField("Max Leaf Size:", _script.maxCityLeafSize);
                        _script.minCityLeafSize = EditorGUILayout.IntField("Min Leaf Size:", _script.minCityLeafSize);
                        _script.roomMaxCitySize = EditorGUILayout.IntField("Room Max Size:", _script.roomMaxCitySize);
                        _script.roomMinCitySize = EditorGUILayout.IntField("Room Min Size:", _script.roomMinCitySize);
                        break;
                    }
                case Generators.DrunkardsWalkMapGenerator:
                    {
                        _script.percentGoal = EditorGUILayout.FloatField("Percent Goal:", _script.percentGoal);
                        _script.walkIterations = EditorGUILayout.IntField("Room Max Size:", _script.walkIterations);
                        _script.weightedTowardCenter = EditorGUILayout.FloatField("Weighted Toward Center:", _script.weightedTowardCenter);
                        _script.weightedTowardPreviousDirection = EditorGUILayout.FloatField("Weighted Toward Previous Direction:", _script.weightedTowardPreviousDirection);
                        break;
                    }
                    case Generators.TunnelingMazeMapGenerator:
                    {
                        _script.magicNumber = EditorGUILayout.IntField("Magic Number:", _script.magicNumber);
                        break;
                    }
                case Generators.TunnelingWithRoomsMapGenerator:
                    {
                        _script.maxTunnelingRooms = EditorGUILayout.IntField("Max Rooms:", _script.maxTunnelingRooms);
                        _script.roomMaxTunnelingSize = EditorGUILayout.IntField("Room Max Size:", _script.roomMaxTunnelingSize);
                        _script.roomMinTunnelingSize = EditorGUILayout.IntField("Room Min Size:", _script.roomMinTunnelingSize);
                        break;
                    }
            }

            // Save the changes back to the object
            EditorUtility.SetDirty(target);
        }

    }
}
