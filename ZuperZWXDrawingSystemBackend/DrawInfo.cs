using System.Text;

namespace ZuperZWXDrawingSystemBackend
{
    public class DrawInfo
    {
        public required string Context { get; init; }
        public required string Name { get; init; }
        public bool IsDynamic;
        public SpriteTileInfo[]? Tiles;
        public bool OneTile { get => (Tiles == null || Tiles.Length == 1) && DefaultCode && DefaultProperties && DefaultX && DefaultY; }
        public bool? ConstantX
        {
            get
            {
                return useConstantValue((tile) => tile.X,
                        (a, b) => a == b ? 0 : 1);
            }
        }
        public bool? ConstantY
        {
            get
            {
                return useConstantValue((tile) => tile.Y,
                        (a, b) => a == b ? 0 : 1);
            }
        }
        public bool? ConstantCode
        {
            get
            {
                return useConstantValue((tile) => tile.Code,
                        (a, b) => a == b ? 0 : 1);
            }
        }
        public bool? ConstantProperties
        {
            get
            {
                return useConstantValue((tile) => tile.Properties.Properties,
                            (a, b) => a == b ? 0 : 1);
            }
        }
        public bool? ConstantSize
        {
            get
            {
                return useConstantValue((tile) => tile.Size,
                            (a, b) => a == b ? 0 : 1);
            }
        }
        public bool HasHorizontalFlip
        {
            get
            {
                if (Tiles == null || Tiles.Length == 0)
                    return false;
                foreach (SpriteTileInfo tileInfo in Tiles)
                {
                    if (tileInfo.XFlippedValue != tileInfo.X)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool HasVerticalFlip
        {
            get
            {
                if (Tiles == null || Tiles.Length == 0)
                {
                    return false;
                }
                foreach (SpriteTileInfo tileInfo in Tiles)
                {
                    if (tileInfo.YFlippedValue != tileInfo.Y)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool DefaultX { get => ConstantX == null || (ConstantX.Value && Tiles![0].X == 0); }
        public bool DefaultY { get => ConstantY == null || (ConstantY.Value && Tiles![0].Y == 0); }
        public bool DefaultCode { get => ConstantCode == null || (ConstantCode.Value && Tiles![0].Code == 0); }
        public bool DefaultProperties { get => ConstantProperties == null || (ConstantProperties.Value && Tiles![0].Properties.Properties == 0); }
        public bool AllBigSize { get => ConstantSize == null || (ConstantSize.Value && Tiles![0].Size == SpriteTileSize.Big); }
        public bool AllSmallSize { get => ConstantSize != null && ConstantSize.Value && Tiles![0].Size == SpriteTileSize.Small; }
        public bool SameSize { get => ConstantSize == null || ConstantSize.Value; }
        public string FullName { get => $"{Context}_{Name}"; }
        public int BaseKey
        {
            get
            {
                int oneTile = OneTile ? 0x01 : 0x00;
                int allBigSize = AllBigSize ? 0x02 : 0x00;
                int constSize = ConstantSize == null || ConstantSize.Value ? 0x04 : 0x00;
                int defX = DefaultX ? 0x08 : 0x00;
                int defY = DefaultY ? 0x10 : 0x00;
                int defCode = DefaultCode ? 0x20 : 0x00;
                int defProp = DefaultProperties ? 0x40 : 0x00;
                int flipy = HasVerticalFlip ? 0x80 : 0x00;
                int flipx = HasHorizontalFlip ? 0x100 : 0x00;
                int dyn = IsDynamic ? 0x200 : 0x00;

                return dyn | flipx | flipy | defProp | defCode | defY |
                    defX | constSize | allBigSize | oneTile;
            }
        }
        private bool? useConstantValue<T>(Func<SpriteTileInfo, T> func, Comparison<T> comparison)
        {
            if (Tiles == null || Tiles.Length == 0)
                return null;
            T val = func(Tiles[0]);
            for (int i = 1; i < Tiles.Length; i++)
            {
                if (comparison(func(Tiles[i]), val) != 0)
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            if (Tiles == null)
                return 0;
            int counter = 0;
            foreach (var tile in Tiles)
                counter += tile.GetHashCode();
            return counter;
        }
        public override bool Equals(object? obj)
        {
            if (obj is not DrawInfo drawInfo)
                return base.Equals(obj);
            if (Tiles != null && drawInfo.Tiles == null)
                return false;
            if (Tiles == null && drawInfo.Tiles != null)
                return false;
            if (Tiles == null && drawInfo.Tiles == null)
                return true;
            if (Tiles!.Length != drawInfo.Tiles!.Length)
                return false;

            for (int i = 0; i < Tiles.Length; i++)
            {
                if (!Tiles[i].Equals(drawInfo.Tiles[i]))
                    return false;
            }
            return true;
        }
        public override string ToString()
        {
            if (OneTile)
                return AllBigSize ? "OneBigTile" : "OneSmallTile";
            StringBuilder sb = new();
            if (DefaultCode | DefaultProperties | DefaultX | DefaultY)
                sb.Append("Same");
            else if (!SameSize)
                sb.Append("AllDifferents");

            if (DefaultX)
                sb.Append('X');
            if (DefaultY)
                sb.Append('Y');
            if (DefaultCode)
                sb.Append("Code");
            if (DefaultProperties)
                sb.Append("Properties");
            if (AllBigSize)
                sb.Append("AllBigTiles");
            if (AllSmallSize)
                sb.Append("AllSmallTiles");

            if (HasHorizontalFlip || HasVerticalFlip)
                sb.Append("WithFlip");
            if (HasHorizontalFlip)
                sb.Append('X');
            if (HasVerticalFlip)
                sb.Append('Y');
            return sb.ToString();
        }
    }
}
