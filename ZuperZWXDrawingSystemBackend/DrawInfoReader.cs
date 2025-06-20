using FormatReader;
using System.Text.RegularExpressions;

namespace ZuperZWXDrawingSystemBackend
{
    public class DrawInfoReader
    {
        public static DrawInfo[] Read(string context, string input)
        {
            Regex commentsRegex = new(@";.*", RegexOptions.Multiline);
            input = commentsRegex.Replace(input, "");
            Regex emptyLines = new(@"\s*$",RegexOptions.Multiline);
            input = emptyLines.Replace(input, "");

            string? dynamicLabel = TableReader.LabelReader(input, "Dynamic");
            dynamicLabel ??= "false";
            bool dynamic;
            if (!bool.TryParse(dynamicLabel, out dynamic))
                dynamic = false;

            string tilesSuffix = "Tiles";
            string propertiesSuffix = "Properties";
            string xDispSuffix = "XDisp";
            string xDispFlipSuffix = "XDispFlipX";
            string yDispSuffix = "YDisp";
            string yDispFlipSuffix = "YDispFlipY";
            string sizesSuffix = "Sizes";

            List<(string, List<int>)> tiles = TableReader.ReadTable(input, "Tiles", 2, tilesSuffix);
            List<(string, List<int>)> props = TableReader.ReadTable(input, "Properties", 2, propertiesSuffix);
            List<(string, List<int>)> xDisp = TableReader.ReadTable(input, "XDisplacements", 2, xDispSuffix, xDispFlipSuffix);
            List<(string, List<int>)> yDisp = TableReader.ReadTable(input, "YDisplacements", 2, yDispSuffix, yDispFlipSuffix);
            List<(string, List<int>)> sizes = TableReader.ReadTable(input, "Sizes", 2, sizesSuffix);

            List<(string, List<int>)> xDispNoFlip = [], xDispFlip = [];
            List<(string, List<int>)> yDispNoFlip = [], yDispFlip = [];

            Regex suffix;

            if (xDisp.Count > 0)
            {
                suffix = new(@"XDisp$");
                xDispNoFlip = xDisp.Where(x => suffix.IsMatch(x.Item1)).ToList();

                suffix = new(@"XDispFlipX$");
                xDispFlip = xDisp.Where(x => suffix.IsMatch(x.Item1)).ToList();
            }
            if (yDisp.Count > 0)
            {
                suffix = new(@"YDisp$");
                yDispNoFlip = yDisp.Where(x => suffix.IsMatch(x.Item1)).ToList();

                suffix = new(@"YDispFlipY$");
                yDispFlip = yDisp.Where(x => suffix.IsMatch(x.Item1)).ToList();
            }

            Dictionary<string, DrawInfo> infos = [];

            setTilesValues(context, tilesSuffix.Length + 1, infos, tiles, (t, v) => t.Code = (byte)v);
            setTilesValues(context, propertiesSuffix.Length + 1, infos, props, (t, v) => t.Properties = new((byte)v));
            setTilesValues(context, xDispSuffix.Length + 1, infos, xDispNoFlip, (t, v) => t.X = (byte)v);
            setTilesValues(context, xDispFlipSuffix.Length + 1, infos, xDispFlip, (t, v) => t.XFlipped = (byte)v);
            setTilesValues(context, yDispSuffix.Length + 1, infos, yDispNoFlip, (t, v) => t.Y = (byte)v);
            setTilesValues(context, yDispFlipSuffix.Length + 1, infos, yDispFlip, (t, v) => t.YFlipped = (byte)v);
            setTilesValues(context, sizesSuffix.Length + 1, infos, sizes, (t, v) => t.Size = (SpriteTileSize)v);

            return infos.Values.ToArray();
        }
        private static void setTilesValues(string context,int suffixSize, Dictionary<string, DrawInfo> infos, List<(string, List<int>)> values, Action<SpriteTileInfo, int> setValue)
        {
            string name;
            DrawInfo info;
            int i;
            foreach (var t in values)
            {
                name = t.Item1[..^suffixSize];
                if (!infos.ContainsKey(name))
                {
                    info = new()
                    {
                        Context = context,
                        Name = name,
                        Tiles = new SpriteTileInfo[t.Item2.Count]
                    };
                    infos.Add(name, info);
                }
                info = infos[name];
                i = 0;
                foreach(var v in t.Item2)
                {
                    if (info.Tiles![i] == null)
                        info.Tiles[i] = new();
                    setValue(info.Tiles[i], v);
                    i++;
                }
            }
        }
    }
}
