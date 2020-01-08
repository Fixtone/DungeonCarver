namespace DungeonCarver
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The DrunkardsWalkMapGenerator generating a level with a single drunkard's walk is guaranteed to produce a fully connected level, with a highly variable mix of narrow paths and open spaces.
    /// </summary>
    /// <seealso href="http://www.roguebasin.com/index.php?title=Random_Walk_Cave_Generation">Random Walk Cave Generation RogueBasin</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class DrunkardsWalkMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
        private readonly float _percentGoal;
        private readonly int _walkIterations;
        private readonly float _weightedTowardCenter;
        private readonly float _weightedTowardPreviousDirection;
        private readonly float _filledGoal;
        private readonly System.Random _random;
        private int _drunkardX;
        private int _drunkardY;
        private MapUtils.CardinalFourDirections _previousDirection;
        private float _filled;

        private T _map;

        /// <summary>
        /// Constructs a new BSPTreeMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="width">The width of the Map to be created</param>
        /// <param name="height">The height of the Map to be created</param>
        /// <param name="maxRooms">The maximum number of rooms that will exist in the generated Map</param>
        /// <param name="roomMaxSize">The maximum width and height of each room that will be generated in the Map</param>
        /// <param name="roomMinSize">The minimum width and height of each room that will be generated in the Map</param>
        public DrunkardsWalkMapGenerator(int width, int height, float percentGoal, int walkIterations, float weightedTowardCenter, float weightedTowardPreviousDirection, System.Random random)
        {
            _width = width;
            _height = height;
            _percentGoal = percentGoal;
            _walkIterations = Math.Max(walkIterations, (_width * _height * 10));
            _weightedTowardCenter = weightedTowardCenter;
            _weightedTowardPreviousDirection = weightedTowardPreviousDirection;
            _random = random;
            _drunkardX = _random.Next(2, _width - 2);
            _drunkardY = _random.Next(2, _height - 2);
            _filledGoal = _width * _height * _percentGoal;
            _filled = 0.0f;
            _map = new T();
        }

        /// <summary>
        /// Creates a new IMap of the specified type.
        /// </summary>
        /// <remarks>
        /// The map will be generated using cellular automata. First each cell in the map will be set to a floor or wall randomly based on the specified fillProbability.
        /// Next each cell will be examined a number of times, and in each iteration it may be turned into a wall if there are enough other walls near it.
        /// Once finished iterating and examining neighboring cells, any isolated map regions will be connected with paths.
        /// </remarks>
        /// <returns>An IMap of the specified type</returns>
        public T CreateMap()
        {
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            int randomWalkIterations = _random.Next(1, _walkIterations);
            for (int iteration = 0; iteration < randomWalkIterations; iteration++)
            {
                Walk(_width, _height);
                if (_filled >= _filledGoal)
                {
                    break;
                }
            }

            return _map;
        }

        public void Walk(int width, int height)
        {
            // ==== Choose Direction ====
            float north = 1.0f;
            float south = 1.0f;
            float east = 1.0f;
            float west = 1.0f;

            //weight the random walk against edges

            //drunkard is at far left side of map
            if (_drunkardX < width * 0.25f)
            {
                east += _weightedTowardCenter;
            }
            //drunkard is at far right side of map
            else if (_drunkardX > _width * 0.75f)
            {
                west += _weightedTowardCenter;
            }

            //drunkard is at the top of the map
            if (_drunkardY < _height * 0.25f)
            {
                south += _weightedTowardCenter;
            }
            //drunkard is at the bottom of the map
            else if (_drunkardY > height * 0.75f)
            {
                north += _weightedTowardCenter;
            }

            //weight the random walk in favor of the previous direction

            if (_previousDirection == MapUtils.CardinalFourDirections.NORTH)
            {
                north += _weightedTowardPreviousDirection;
            }

            if (_previousDirection == MapUtils.CardinalFourDirections.SOUTH)
            {
                south += _weightedTowardPreviousDirection;
            }

            if (_previousDirection == MapUtils.CardinalFourDirections.EAST)
            {
                east += _weightedTowardPreviousDirection;
            }

            if (_previousDirection == MapUtils.CardinalFourDirections.WEST)
            {
                west += _weightedTowardPreviousDirection;
            }

            //normalize probabilities so they form a range from 0 to 1
            float total = north + south + east + west;
            north /= total;
            south /= total;
            east /= total;
            west /= total;

            //Choose the direction
            MapUtils.CardinalFourDirections direction;
            double choice = _random.NextDouble();
            int dx = 0;
            int dy = 0;

            if ((0.0f <= choice) && (choice < north))
            {
                dx = 0;
                dy = -1;
                direction = MapUtils.CardinalFourDirections.NORTH;
            }
            else if ((north <= choice) && choice < (north + south))
            {
                dx = 0;
                dy = 1;
                direction = MapUtils.CardinalFourDirections.SOUTH;
            }
            else if (((north + south) <= choice) && (choice < (north + south + east)))
            {
                dx = 1;
                dy = 0;
                direction = MapUtils.CardinalFourDirections.EAST;
            }
            else
            {
                dx = -1;
                dy = 0;
                direction = MapUtils.CardinalFourDirections.WEST;
            }

            //==== Walk ====		
            if (((0 < _drunkardX + dx) && (_drunkardX + dx < width - 1)) && ((0 < _drunkardY + dy) && (_drunkardY + dy < height - 1)))
            {
                _drunkardX += dx;
                _drunkardY += dy;

                if (_map.GetTile(_drunkardX, _drunkardY).type.Equals(Tile.Type.Block))
                {
                    _map.SetTile(_drunkardX, _drunkardY, new Tile(Tile.Type.Empty));
                    _filled += 1.0f;
                }

                _previousDirection = direction;
            }

        }
    }
}
