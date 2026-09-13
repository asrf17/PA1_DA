using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Puente local de desarrollo: solo procesa solicitudes explícitas en Temp.
[InitializeOnLoad]
public static class PA2EditorBridge
{
    static PA2EditorBridge()
    {
        EditorApplication.update += Tick;
    }
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;
        string path = "Temp/pa2-command.txt";
        if (!File.Exists(path))
            return;
        string command = File.ReadAllText(path).Trim();
        File.Delete(path);
        try
        {
            if (command == "build")
            {
                var type = Type.GetType("PA2LevelBuilder, Assembly-CSharp-Editor");
                type.GetMethod("Build").Invoke(null, null);
            }
            else if (command == "play")
                EditorApplication.isPlaying = true;
            else if (command == "stop")
                EditorApplication.isPlaying = false;
            else if (command == "save")
                EditorSceneManager.SaveOpenScenes();
            else if (command == "refresh")
                AssetDatabase.Refresh();
            else if (command == "game")
                EditorApplication.ExecuteMenuItem("Window/General/Game");
            else if (command == "start")
                GameManager.Instancia.Continuar();
            else if (command == "status")
                File.WriteAllText("Temp/pa2-status.txt", "playing=" + EditorApplication.isPlaying + ";scene=" + EditorSceneManager.GetActiveScene().path);
            else if (command == "verify")
                Type.GetType("PA2Validation, Assembly-CSharp-Editor").GetMethod("Run").Invoke(null, null);
            else if (command == "route")
                Type.GetType("PA2Validation, Assembly-CSharp-Editor").GetMethod("Route").Invoke(null, null);
            else if (command == "restart")
                GameManager.Instancia.Reiniciar();
            else if (command.StartsWith("capture"))
            {
                int width = command == "capture4x3" ? 960 : 1280;
                var camera = Camera.main;
                var rt = new RenderTexture(width, 720, 24);
                var prior = camera.targetTexture;
                camera.targetTexture = rt;
                camera.Render();
                var old = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(width, 720, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, 720), 0, 0);
                tex.Apply();
                File.WriteAllBytes("Temp/pa2-view.png", tex.EncodeToPNG());
                camera.targetTexture = prior;
                RenderTexture.active = old;
                UnityEngine.Object.DestroyImmediate(tex);
                UnityEngine.Object.DestroyImmediate(rt);
            }
            File.WriteAllText("Temp/pa2-result.txt", "OK " + command + " " + DateTime.Now);
        }
        catch (Exception e) { File.WriteAllText("Temp/pa2-result.txt", e.ToString()); Debug.LogException(e); }
    }
}
