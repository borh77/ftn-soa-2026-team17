using System.Text.RegularExpressions;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Assembly = System.Reflection.Assembly;

namespace TouristApp.Architecture.Tests;

public class BaseArchitecturalTests
{
    protected ArchUnitNET.Domain.Architecture Architecture;

    public BaseArchitecturalTests()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        foreach (var dll in Directory.GetFiles(path, "TouristApp.*.dll"))
        {
            Assembly.LoadFile(dll);
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        Architecture = new ArchLoader().LoadAssemblies(assemblies
            .Where(a => a.FullName.StartsWith("TouristApp"))
            .Select(a => Assembly.Load(a.FullName))
            .ToArray()
        ).Build();
    }

    protected IEnumerable<IType> GetExaminedTypes(string assemblyName)
    {
        // Replace "Solution." prefix with "TouristApp." if needed
        var actualAssemblyName = assemblyName.Replace("Solution.", "TouristApp.");
        return Architecture.Assemblies
            .Where(a => Regex.IsMatch(a.FullName, actualAssemblyName))
            .SelectMany(a => Architecture.Types.Where(c => c.Assembly.Equals(a)));
    }

    protected IEnumerable<IType> GetForbiddenTypes(params string[] exemptAssemblyNames)
    {
        // Replace "Solution." prefix with "TouristApp." for each exempt assembly name
        var actualExemptNames = exemptAssemblyNames.Select(n => n.Replace("Solution.", "TouristApp.")).ToArray();
        return Architecture.Assemblies
            .Where(a => actualExemptNames.All(n => !Regex.IsMatch(a.FullName, n)))
            .SelectMany(a => Architecture.Types.Where(c => c.Assembly.Equals(a)));
    }
}