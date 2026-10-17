using System.IO;
using System.Linq;

namespace Source2MapCompiler;

internal sealed record ListedMap(string Path, bool Enabled);

// resourcecompiler reads with -filelist.. a .vmap path per each line
internal static class MapList
{
    public const string FileName = "maplist.txt";

    // maps in the maplist.txt with trimmed empty lines
    public static string[] Read(string list)
    {
        return [.. File.ReadLines(list).Select(line => line.Trim()).Where(line => line.Length > 0)];
    }

    // writes the maps that are turned on using a full path
    public static void Write(string list, IEnumerable<ListedMap> maps)
    {
        File.WriteAllLines(list, maps.Where(map => map.Enabled).Select(map => map.Path));
    }

    // gets every .vmap in a dir and its sub dirs
    public static string[] Find(string folder)
    {
        var search = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
        var maps = Directory.GetFiles(folder, "*.vmap", search);
        Array.Sort(maps, StringComparer.OrdinalIgnoreCase);
        return maps;
    }

    // puts the turned off maps back in place... if maplist.txt was edited outside the compiler every map in it is turned on agne
    public static List<ListedMap> Merge(string[] inFile, IReadOnlyList<ListedMap> remembered)
    {
        var rememberedOn = remembered.Where(map => map.Enabled).Select(map => map.Path);

        if (rememberedOn.SequenceEqual(inFile, StringComparer.OrdinalIgnoreCase))
        {
            return [.. remembered];
        }

        return [.. inFile.Select(path => new ListedMap(path, true))];
    }
}
