namespace ZuperZWXDrawingSystemBackend
{
    public enum SpriteTileSize { Small = 0, Big = 2 };
    public class SpriteTileInfo
    {
        public int X;
        public int Y;
        public int? XFlipped;
        public int? YFlipped;
        public int XFlippedValue { get => XFlipped == null ? X : XFlipped.Value; }
        public int YFlippedValue { get => YFlipped == null ? Y : YFlipped.Value; }
        public byte Code;
        public SpriteTileProperties Properties;
        public SpriteTileSize Size;
        public override bool Equals(object? obj)
        {
            if (obj is not SpriteTileInfo tile)
                return base.Equals(obj);
            return X == tile.X && Y == tile.Y && Code == tile.Code &&
                Properties.Equals(tile.Properties) && Size == tile.Size;
        }
        public override int GetHashCode()
        {
            return X << 17 + Y << 9 + Properties.Properties << 1 + ((int)Size >> 1);
        }
        public override string ToString()
        {
            return $"{Code:X2}-{Size}: ({X},{Y},{XFlippedValue},{YFlippedValue}) ";
        }
    }
}
