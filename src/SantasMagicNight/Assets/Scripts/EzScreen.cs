using System.IO;
using UnityEngine;

#if UNITY_EDITOR_WIN
using File = UnityEngine.Windows.File;
#endif

public class EzScreen : CrossSceneSingleInstance
{
    [SerializeField] private string basePath;
    
    protected override string UniqueTag => "Screenshots";
    private static int _counter;

    private string DirPath => Path.Combine(basePath, Application.productName);
    private string FilePath => Path.Combine(DirPath, "scr");

    protected override void OnAwake()
    {
#if UNITY_EDITOR_WIN
        if (!Directory.Exists(DirPath))
            Directory.CreateDirectory(DirPath);
        while (File.Exists($"{FilePath}_{_counter}.png"))
            _counter++;
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            var n = $"{FilePath}_{_counter++}.png";
            ScreenCapture.CaptureScreenshot(n);
            Debug.Log($"Captured screenshot: {n}");
        }
    }
}
