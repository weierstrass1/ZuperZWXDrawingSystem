namespace ZuperZWXDrawingSystemBackend
{
    public enum SpriteTilePriority { Lowest = 0, Low = 1, Normal = 2, Highest };
    public enum SpriteTilePage { SP12 = 0, SP34 = 1 };
    public enum SpriteTilePalette 
    { 
        Palette8 = 0,
        Palette9 = 1,
        PaletteA = 2,
        PaletteB = 3,
        PaletteC = 4,
        PaletteD = 5,
        PaletteE = 6,
        PaletteF = 7,
    }
    public struct SpriteTileProperties
    {
        public byte Properties;
        public readonly bool HorizontalFlip { get => (Properties & 0x40) != 0; }
        public readonly bool VerticalFlip { get => (Properties & 0x80) != 0; }
        public readonly SpriteTilePriority Priority { get => (SpriteTilePriority)((Properties >> 4) & 0x03); }
        public readonly SpriteTilePage Page { get => (SpriteTilePage)(Properties & 0x01); }
        public readonly SpriteTilePalette Palette { get => (SpriteTilePalette)((Properties >> 1) & 0x07); }
        public SpriteTileProperties()
        {
            Properties = 0;
        }
        public SpriteTileProperties(byte properties)
        {
            Properties = properties;
        }
        public SpriteTileProperties(bool horizontalFlip, bool verticalFlip, SpriteTilePriority priority,
            SpriteTilePage page, SpriteTilePalette palette)
        {
            int hf = horizontalFlip ? 0x40 : 0x00;
            int vf = verticalFlip ? 0x80 : 0x00;
            int prior = ((int)priority) << 4;
            int pag = (int)page;
            int pal = ((int)palette) << 1;

            Properties = (byte)(vf | hf | prior | pal | pag);
        }
        public override readonly bool Equals(object? obj)
        {
            if (obj is not SpriteTileProperties prop)
                return base.Equals(obj);
            return Properties == prop.Properties;
        }
        public override readonly int GetHashCode()
        {
            return Properties;
        }
        public override string ToString()
        {
            return $"{Properties:X2}";
        }
        public static bool operator ==(SpriteTileProperties left, SpriteTileProperties right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(SpriteTileProperties left, SpriteTileProperties right)
        {
            return !(left == right);
        }
    }
}
