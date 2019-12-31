namespace TheDivineComedy
{
    using UnityEngine;

    /// <summary>
    /// Represents a single tile in the map
    /// </summary>
    public class Tile
    {
        public enum Type
        {
            Wall = 0,
            Empty = 1
        }

        public Type type;

        public Tile(Type type)
        {
            this.type = type;
        }
    }
}
