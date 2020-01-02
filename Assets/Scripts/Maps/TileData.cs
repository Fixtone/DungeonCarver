namespace DungeonCarver
{
    using UnityEngine;

    public class TileData
    {
        public Tile Tile;
        public Vector2Int Position;

        public TileData(Tile tile, Vector2Int position)
        {
            this.Tile = tile;
            this.Position = position;
        }
    }
}
