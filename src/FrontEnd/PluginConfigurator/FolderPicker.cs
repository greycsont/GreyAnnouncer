/*using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FolderPicker : MonoBehaviour
{
    public Text currentPathText;
    public Transform contentParent;
    public Button folderButtonPrefab;
    public Button upButton;
    public Button confirmButton;

    private string currentPath;
    private Action<string> callback;

    public void Open(string startPath, Action<string> onSelected)
    {
        callback = onSelected;
        currentPath = startPath;
        Refresh();
        gameObject.SetActive(true);
    }

    private void Refresh()
    {
        currentPathText.text = currentPath;

        foreach (Transform t in contentParent) Destroy(t.gameObject);

        try
        {
            foreach (var dir in Directory.GetDirectories(currentPath))
            {
                var btn = Instantiate(folderButtonPrefab, contentParent);
                btn.GetComponentInChildren<Text>().text = Path.GetFileName(dir);
                btn.onClick.AddListener(() =>
                {
                    currentPath = dir;
                    Refresh();
                });
            }
        }
        catch {}
    }

    public void UpDirectory()
    {
        var parent = Directory.GetParent(currentPath);
        if (parent != null)
        {
            currentPath = parent.FullName;
            Refresh();
        }
    }

    public void Confirm()
    {
        callback?.Invoke(currentPath);
        gameObject.SetActive(false);
    }
}
*/