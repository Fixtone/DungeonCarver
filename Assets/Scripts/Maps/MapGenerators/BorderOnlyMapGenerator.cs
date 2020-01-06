namespace DungeonCarver
{
    using UnityEngine;

    /// <summary>
    /// The BorderOnlyMapGenerator creates a Map of the specified type by making an empty map with only the outermost border being solid walls
    /// </summary>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class BorderOnlyMapGenerator<T> : IMapGenerator<T> where T : IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
        
        public BorderOnlyMapGenerator(int width, int height)
        {
            _width = width;
            _height = height;
        }
        
        public T CreateMap()
        {
            T map = new T();
            map.Initialize(_width, _height);
            map.Clear(new Tile(Tile.Type.Empty));

            //foreach (TileData tileData in map.GetTilesInRows(0, _height - 1))
            //{
            //    map.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Block));
            //}

            foreach ((Vector2Int tilePosition, Tile tile) tileInfo in map.GetTilesInRows2(0, _height - 1))
            {
                map.SetTile(tileInfo.tilePosition.x, tileInfo.tilePosition.y, new Tile(Tile.Type.Block));
            }

            foreach (TileData tileData in map.GetTilesInColumns(0, _width - 1))
            {
                map.SetTile(tileData.Position.x, tileData.Position.y, new Tile(Tile.Type.Block));
            }

            return map;
        }
    }
}