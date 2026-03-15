using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace GreyAnnouncer.Util;

public static class UnityPathManager
{
    public static Canvas FindCanvas()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            var canvas = root.GetComponent<Canvas>();
            if (canvas != null)
                return canvas;
        }
        return null;
    }

    public static async Task<Texture2D> LoadTextureFromFileAsync(string path)
    {
        using var request = UnityWebRequestTexture.GetTexture("file:///" + path);
        
        var tcs = new TaskCompletionSource<Texture2D>();
        var operation = request.SendWebRequest();
        
        operation.completed += _ =>
        {
            if (request.result == UnityWebRequest.Result.Success)
                tcs.SetResult(DownloadHandlerTexture.GetContent(request));
            else
                tcs.SetResult(null);
        };
        
        return await tcs.Task;
    }

}