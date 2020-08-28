using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LoadStaticDataFromGoogleSheets : EditorWindow
{

    private static string outputJsonDirectory = Path.GetFullPath(Application.dataPath + "/Resources/StaticData/");
    private static string outputScriptsDirectory = Path.GetFullPath(Application.dataPath + "/Scripts/StaticData/");
    private static string errorMessage;

    [MenuItem("StaticData/Load All Static")]
    public static void LoadAll()
    {
        EditorUtility.DisplayProgressBar("Загрузка static", "Ждем пока сформируются данные", 1);
        LoadStaticData("main.py");
    }

    private static void LoadStaticData(string file)
    {
        var pathVariable = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) + ";" + Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
        var pathToFolder = pathVariable.Split(';').FirstOrDefault(x => File.Exists($"{x}\\python.exe"));
        if (string.IsNullOrEmpty(pathToFolder))
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Не найден Python", "Не найден Python или не правильно настроен.\nВозможно не добавлен путь в переменных среды", "Попробую исправить!");
            return;
        }
        var pathToPython = pathToFolder + "\\python.exe";
        var pathToFile = Path.GetFullPath(Application.dataPath + "/../GoogleSheets/" + file);

        var info = new ProcessStartInfo(pathToPython, pathToFile + " " + outputJsonDirectory + " " + outputScriptsDirectory)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(pathToFile),
        };
        try
        {
            var proc = new Process();
            errorMessage = "";
            proc.StartInfo = info;
            proc.StartInfo.RedirectStandardError = true;
            proc.ErrorDataReceived += new DataReceivedEventHandler(NetErrorDataHandler);
            proc.Start();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
            if (proc.ExitCode == 0)
            {
                EditorUtility.DisplayDialog("Успех", "Выполнение скриптов вернулось без ошибок", "ОК");
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("Ошибка выполнения скрипта", "Python завершился с ошибкой: " + GetErrorCode(proc.ExitCode), "ОК");
                Debug.LogError("GoogleSheets ExitCode: " + proc.ExitCode);
                Debug.LogError("GoogleSheets Error: " + errorMessage);
            }
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Ошибка запуска процесса Python", "Подробности смотри в консоле", "ОК");
            Debug.LogError(e);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void NetErrorDataHandler(object sendingProcess, DataReceivedEventArgs errLine)
    {
        if (!String.IsNullOrEmpty(errLine.Data))
        {
            errorMessage += errLine.Data + "\r\n";
        }
    }

    private static string GetErrorCode(int i)
    {
        switch (i)
        {
            case 1: return "1. Подробности в консоле";
            case 404: return "нет доступа к гугл-таблице";
            case 429: return "превышен лимит 'Read requests per user per 100 seconds' при обращении к сервису 'sheets.googleapis.com'";
            default: return "неизвестная ошибка";
        }
    }
}
