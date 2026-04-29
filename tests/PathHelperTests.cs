using System.IO;
using GreyAnnouncer.Util;

namespace GreyAnnouncer.Tests;

public class PathHelperTests
{
    // base dir as seen from PathHelper itself (GreyAnnouncer.dll lives in test output)
    private static readonly string PluginDir =
        Path.GetDirectoryName(typeof(PathHelper).Assembly.Location)!;

    // --- ResolveUserPath: absolute paths ---

    [Theory]
    [InlineData(@"C:\MyAudio")]
    [InlineData(@"C:/MyAudio")]
    [InlineData(@"D:\some\deep\path")]
    public void ResolveUserPath_AbsolutePath_ReturnedUnchanged(string path)
    {
        Assert.Equal(path, PathHelper.ResolveUserPath(path));
    }

    // --- ResolveUserPath: relative paths → rebased onto plugin dir ---

    [Theory]
    [InlineData("announcers")]
    [InlineData("./announcers")]
    public void ResolveUserPath_RelativePath_IsRooted(string path)
    {
        var result = PathHelper.ResolveUserPath(path);
        Assert.True(Path.IsPathRooted(result), $"Expected rooted path, got: {result}");
    }

    [Theory]
    [InlineData("announcers")]
    [InlineData("./announcers")]
    public void ResolveUserPath_RelativePath_BasedOnPluginDir(string path)
    {
        var result = PathHelper.ResolveUserPath(path);
        Assert.StartsWith(PluginDir, result);
    }

    [Fact]
    public void ResolveUserPath_RelativePath_MatchesGetCurrentPluginPath()
    {
        Assert.Equal(
            PathHelper.GetCurrentPluginPath("announcers"),
            PathHelper.ResolveUserPath("announcers")
        );
    }

    // --- CleanPath: strips Unicode directional characters ---

    [Theory]
    [InlineData("\u202AC:\\path")]   // LEFT-TO-RIGHT EMBEDDING
    [InlineData("\u202BC:\\path")]   // RIGHT-TO-LEFT EMBEDDING
    [InlineData("\u202CC:\\path")]   // POP DIRECTIONAL FORMATTING
    [InlineData("\u202DC:\\path")]   // LEFT-TO-RIGHT OVERRIDE
    [InlineData("\u202EC:\\path")]   // RIGHT-TO-LEFT OVERRIDE
    public void CleanPath_DirectionalPrefix_IsStripped(string dirtyPath)
    {
        Assert.Equal(@"C:\path", PathHelper.CleanPath(dirtyPath));
    }

    [Fact]
    public void CleanPath_NormalPath_Unchanged()
    {
        const string path = @"C:\normal\path";
        Assert.Equal(path, PathHelper.CleanPath(path));
    }
}
