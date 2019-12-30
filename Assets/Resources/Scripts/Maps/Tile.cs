namespace TheDivineComedy
{
    using UnityEngine;

    /// <summary>
    /// Represents a single tile in the map
    /// </summary>
    public class Tile
    {        
        public bool blocked;

        public Tile(bool blocked)
        {
            this.blocked = blocked;
        }
    }
}
