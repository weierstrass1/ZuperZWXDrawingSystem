using System.Collections.Generic;
using System.Text;

namespace ZuperZWXDrawingSystemBackend
{
    public class GraphicRoutineManager
    {
        public static Dictionary<int, List<DrawInfo>> GetUnrepeatedDrawInfos(DrawInfo[] drawInfos)
        {
            Dictionary<int, List<DrawInfo>> result = [];
            bool[] repeated = new bool[drawInfos.Length];
            for (int i = 0; i < drawInfos.Length; i++)
            {
                if (!repeated[i])
                    result.Add(i, [drawInfos[i]]);
                for (int j = i + 1; j < drawInfos.Length && !repeated[i]; j++)
                {
                    if (drawInfos[i].Equals(drawInfos[j]))
                    {
                        repeated[j] = true;
                        result[i].Add(drawInfos[j]);
                    }
                }
            }
            return result;
        }
        public static (string, List<(string, string)>) GetGraphicRoutines(Dictionary<int, Dictionary<int, List<DrawInfo>>> drawinfos)
        {
            string name;

            List<(string, string)> namesAndRoutines = [];

            StringBuilder routine = new();
            StringBuilder address = new();
            StringBuilder numberOfTilesDefines = new();
            StringBuilder offsetDefines = new();

            StringBuilder tiles = new();
            StringBuilder props = new();
            StringBuilder xs = new();
            StringBuilder ys = new();
            StringBuilder xflips = new();
            StringBuilder yflips = new();
            StringBuilder sizes = new();

            DrawInfo info;
            int count = 0;
            //GraphicRoutine(onetile,dynamic,code,props,size,bigSize,x,y,xflip,yflip)
            foreach (var kvp in drawinfos)
            {
                tiles.Clear();
                props.Clear();
                xs.Clear();
                ys.Clear();
                xflips.Clear();
                yflips.Clear();
                sizes.Clear();
                routine.Clear();

                tiles.AppendLine("Tiles:");
                props.AppendLine("Properties:");
                xs.AppendLine("XDisplacements:");
                xflips.AppendLine("XDisplacementsFlipped:");
                ys.AppendLine("YDisplacements:");
                yflips.AppendLine("YDisplacementsFlipped:");
                sizes.AppendLine("Sizes:");
                info = kvp.Value.First().Value[0];
                name = info.ToString();
                routine.AppendLine($"namespace {info}\nMain:");
                routine.AppendLine($"\t%GraphicRoutine({b2s(info.OneTile)},{b2s(info.IsDynamic)},{b2s(info.DefaultCode)},{b2s(info.DefaultProperties)},{b2s(info.SameSize)},{b2s(info.AllBigSize)},{b2s(info.DefaultX)},{b2s(info.DefaultY)},{b2s(info.HasHorizontalFlip)},{b2s(info.HasVerticalFlip)})");

                tiles.Append(string.Join("", [.. TileCodeTable(kvp.Value).Values]));
                props.Append(string.Join("", [.. TilePropertiesTable(kvp.Value).Values]));
                xs.Append(string.Join("", [.. TileXTable(kvp.Value).Values]));
                ys.Append(string.Join("", [.. TileYTable(kvp.Value).Values]));
                xflips.Append(string.Join("", [.. TileXFlipTable(kvp.Value).Values]));
                yflips.Append(string.Join("", [.. TileYFlipTable(kvp.Value).Values]));
                sizes.Append(string.Join("", [.. TileSizesTable(kvp.Value).Values]));

                foreach (var tuple in kvp.Value)
                {
                    foreach (var drinfo in tuple.Value)
                    {
                        address.AppendLine($"!PoseRoutineAddress_{drinfo.FullName} = {name}_Main");
                        numberOfTilesDefines.AppendLine($"!PoseLength_{drinfo.FullName} = ${(drinfo.Tiles == null ? 1 : drinfo.Tiles.Length):X2}");
                        offsetDefines.AppendLine($"!PoseOffset_{drinfo.FullName} = ${count:X4}");
                    }
                    count += tuple.Value[0].Tiles == null ? 0 : tuple.Value[0].Tiles!.Length;
                }
                if (info.OneTile)
                {
                    routine.AppendLine($"namespace off");
                    namesAndRoutines.Add((name, routine.ToString()));
                    continue;
                }
                if (!info.DefaultCode)
                    routine.AppendLine(tiles.ToString());
                if(!info.DefaultProperties)
                    routine.AppendLine(props.ToString());
                if (!info.DefaultX)
                    routine.AppendLine(xs.ToString());
                if (info.HasHorizontalFlip)
                    routine.AppendLine(xflips.ToString());
                if (!info.DefaultY)
                    routine.AppendLine(ys.ToString());
                if (info.HasVerticalFlip)
                    routine.AppendLine(yflips.ToString());
                if (!info.SameSize)
                    routine.AppendLine(sizes.ToString());
                routine.AppendLine($"namespace off");
                namesAndRoutines.Add((name, routine.ToString()));
            }
            string defines = $"{address}\n{numberOfTilesDefines}\n{offsetDefines}";
            return (defines, namesAndRoutines);
        }
        private static string b2s(bool b)
        {
            return b ? "1" : "0";
        }
        public static Dictionary<int, Dictionary<int, List<DrawInfo>>> GroupByKey(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            Dictionary<int, Dictionary<int, List<DrawInfo>>> result = [];
            int key;

            foreach (var kvp in drawinfos)
            {
                key = kvp.Value[0].BaseKey;
                if (!result.ContainsKey(key))
                    result.Add(key, []);
                result[key].Add(kvp.Key, kvp.Value);
            }
            return result;
        }
        private static readonly string[] tableTypes = { "db", "db", "db", "dw", "dw", "dl", "dl", "dd", "dd" };
        private static string getStringFromValues(int[] values, int update = 16, int digitsPerNumber = 2)
        {
            digitsPerNumber = Math.Clamp(digitsPerNumber, 2, tableTypes.Length - 1);
            string tableType = tableTypes[digitsPerNumber];
            StringBuilder sb = new();
            for (int i = 0; i < values.Length; i += update)
            {
                sb.AppendLine($"\t{tableType} " + string.Join(",", values[i..Math.Min(i + 16, values.Length)].Select(v =>
                   $"${v.ToString($"X{digitsPerNumber}")}")
                   .ToArray()));
            }
            return sb.ToString();
        }
        public static string NumberOfTilesTable(DrawInfo[] drawInfos)
        {
            int[] values = drawInfos.Select(di =>
                   di.Tiles == null ?
                       0 :
                       di.Tiles.Length)
                .ToArray();
            return getStringFromValues(values);
        }
        private static Dictionary<int, string> getTileValuesTable(Dictionary<int, List<DrawInfo>> drawinfos, Func<SpriteTileInfo[], int[]> selector)
        {
            Dictionary<int, string> result = [];
            StringBuilder sb = new();
            SpriteTileInfo[]? tiles;
            int[] values;
            foreach (var kvp in drawinfos)
            {
                sb.Clear();
                foreach (var drawinfo in kvp.Value)
                {
                    sb.AppendLine($".{drawinfo.Context}_{drawinfo.Name}:");
                }
                tiles = kvp.Value[0].Tiles;
                if (tiles == null)
                    continue;
                values = selector(tiles);
                sb.Append(getStringFromValues(values));

                result.Add(kvp.Key, sb.ToString());
            }
            return result;
        }
        public static Dictionary<int, string> TileCodeTable(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            return getTileValuesTable(drawinfos, (tiles) => tiles.Select(t => (int)t.Code).ToArray());
        }
        public static Dictionary<int, string> TilePropertiesTable(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            return getTileValuesTable(drawinfos, (tiles) => tiles.Select(t => (int)t.Properties.Properties).ToArray());
        }
        public static Dictionary<int, string> TileXTable(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            return getTileValuesTable(drawinfos, (tiles) => tiles.Select(t => t.X).ToArray());
        }
        public static Dictionary<int, string> TileXFlipTable(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            return getTileValuesTable(drawinfos, (tiles) => tiles.Select(t => t.XFlippedValue).ToArray());
        }
        public static Dictionary<int, string> TileYTable(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            return getTileValuesTable(drawinfos, (tiles) => tiles.Select(t => t.Y).ToArray());
        }
        public static Dictionary<int, string> TileYFlipTable(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            return getTileValuesTable(drawinfos, (tiles) => tiles.Select(t => t.YFlippedValue).ToArray());
        }
        public static Dictionary<int, string> TileSizesTable(Dictionary<int, List<DrawInfo>> drawinfos)
        {
            return getTileValuesTable(drawinfos, (tiles) => tiles.Select(t => (int)t.Size).ToArray());
        }
    }
}
