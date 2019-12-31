namespace DungeonCarver
{
    /// <summary>
    /// Represents a single tile in the map
    /// </summary>
    public class Tile
    {
        public enum Type
        {
            Block = 0,
            Empty = 1
        }

        public Type type;

        public Tile(Type type)
        {
            this.type = type;
        }
    }
}
