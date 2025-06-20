// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.RegularExpressions;
using ZuperZWXDrawingSystemBackend;

string drawInfosPath = "DrawInfos";

string[] drawinfosFiles = Directory.GetFiles(drawInfosPath, "*.drawinfo");

List<DrawInfo> drawinfoList = [];
string filename;

foreach (string drawinfo in drawinfosFiles )
{
    filename = Regex.Replace(Path.GetFileNameWithoutExtension(drawinfo), @"\s", "_");
    drawinfoList.AddRange(DrawInfoReader.Read(filename, File.ReadAllText(drawinfo)));
}
var infos = drawinfoList.ToArray();
var infosUnrepeated = GraphicRoutineManager.GetUnrepeatedDrawInfos(infos);
var grouped = GraphicRoutineManager.GroupByKey(infosUnrepeated);
var grroutines = GraphicRoutineManager.GetGraphicRoutines(grouped);

string constantsPath = Path.Combine("..", "Constants", "DrawingSystemConstants.asm");
string graphicRoutinesFolderPath = Path.Combine("..", "GraphicRoutines");
string graphicRoutinesIncludePath = Path.Combine(graphicRoutinesFolderPath, "GraphicRoutinesInclude.asm");

File.WriteAllText(constantsPath, grroutines.Item1);

string graphicRoutinePath;

StringBuilder include = new();

Dictionary<string, string> includedFiles = [];

string[] oldfiles = Directory.GetFiles(graphicRoutinesFolderPath, "*.asm");
foreach(string oldfile in oldfiles)
{
    File.Delete(oldfile);
}

foreach (var routine in grroutines.Item2)
{
    graphicRoutinePath = Path.Combine(graphicRoutinesFolderPath, $"{routine.Item1}.asm");
    File.WriteAllText(graphicRoutinePath, routine.Item2);

    if (!includedFiles.ContainsKey(graphicRoutinePath))
        include.AppendLine($"incsrc \"{graphicRoutinePath.Replace('\\', '/')}\"");
}

File.WriteAllText(graphicRoutinesIncludePath, include.ToString());
