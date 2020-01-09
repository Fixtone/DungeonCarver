namespace DungeonCarver
{
    using System.Collections.Generic;
    using UnityEngine;

    public class MapUtils
    {
        public enum CardinalFourDirections
        {
            NORTH = 0,
            EAST = 1,
            WEST = 2,
            SOUTH = 3
        }

        /// <summary>
        /// Generic list of points which contain 4 directions
        /// </summary>
        public static List<Vector2Int> FourDirections = new List<Vector2Int>()
        {
            new Vector2Int (0,1)    //north
            , new Vector2Int(0,-1)    //south
            , new Vector2Int (1,0)   //east
            , new Vector2Int (-1,0)  //west
        };

        /// <summary>
        /// Generic list of points which contain 9 directions
        /// </summary>
        public static List<Vector2Int> NineDirections = new List<Vector2Int>()
        {
            new Vector2Int (0,-1)    //north
            , new Vector2Int(0,1)    //south
            , new Vector2Int (1,0)   //east
            , new Vector2Int (-1,0)  //west
            , new Vector2Int (1,-1)  //northeast
            , new Vector2Int(-1,-1)  //northwest
            , new Vector2Int (-1,1)  //southwest
            , new Vector2Int (1,1)   //southeast
            , new Vector2Int(0,0)    //centre
        };
    }
}
