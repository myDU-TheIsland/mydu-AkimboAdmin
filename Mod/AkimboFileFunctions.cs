using Orleans;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Backend;
using Backend.Business;
using Backend.Database;
using NQutils.Config;
using Backend.Storage;
using Backend.Scenegraph;
using NQ;
using NQ.RDMS;
using NQ.Interfaces;
using NQutils;
using NQutils.Net;
using NQutils.Serialization;
using NQutils.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Runtime;
using Backend.AWS;
using NQ.Visibility;
using NQutils.Messages;

public class AkimboFileFunctions
{
    private static string logFilePath = GetFilePath("AkimboModLog.log"); // Log file in the Mods directory
    public AkimboFileFunctions()
	{
	}
    public static string GetModsDirectoryPath()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(basePath, "Mods/ModAkimboAdmin");
    }

    // get the file path 
    public static string GetFilePath(string filename)
    {
        var modsDirectory = GetModsDirectoryPath();
        if (!Directory.Exists(modsDirectory))
        {
            Directory.CreateDirectory(modsDirectory); // Ensure the Mods directory exists
        }
        return Path.Combine(modsDirectory, filename);
    }
    public static string LoadCssContent()
    {
        var filePath = GetFilePath("AkimboAdminHud.css");
        if (File.Exists(filePath))
        {
            var cssContent = File.ReadAllText(filePath);
            return cssContent;
        }
        else
        {
            LogError($"CSS file not found at {filePath}.");
            return string.Empty;
        }
    }
    public static string LoadDependencies()
    {
        var filePath = GetFilePath("CodeMirrorJS.js");
        if (File.Exists(filePath))
        {
            var jsContent = File.ReadAllText(filePath);
            return jsContent;
        }
        else
        {
            LogError($"Dependency file not found at {filePath}.");
            return string.Empty;
        }
    }

    public static string LoadJsContent(List<string> locations)
    {
        var filePath = GetFilePath("AkimboAdminHud.js");
        if (File.Exists(filePath))
        {
            LogInfo($"Loading Main JS hold on...");
            var jsContent = File.ReadAllText(filePath);
            LogInfo($"Loading CSS hold on...");
            var cssContent = LoadCssContent();
            LogInfo($"Loading Dependencies hold on...");
            var dependencies = LoadDependencies();
            var jsArray = $"[{string.Join(",", locations.Select(n => $"\"{n}\""))}]";
            if (!string.IsNullOrEmpty(cssContent))
            {
                jsContent = $"const injectedCss = `{cssContent}`;\n" + $"var teleportTags = `{jsArray}`;\n" + dependencies + jsContent;
            }
            return jsContent;
        }
        else
        {
            LogError($"JavaScript file not found at {filePath}.");
            return string.Empty;
        }
    }
    public static string LoadConstructJsContent(List<string> locations)
    {
        var filePath = GetFilePath("AkimboAdminConstructHud.js");
        if (File.Exists(filePath))
        {
            LogInfo($"Construct JavaScript file found at {filePath}.");
            var jsContent = File.ReadAllText(filePath);
            //logger.LogInformation($"Loaded JS content: {jsContent}");
            return jsContent;
        }
        else
        {
            LogError($"Construct JavaScript file not found at {filePath}.");
            return string.Empty;
        }
    }
    public static string LoadElementJsContent(List<string> locations)
    {
        var filePath = GetFilePath("AkimboAdminElementHud.js");
        if (File.Exists(filePath))
        {
            LogInfo($"Element JavaScript file found at {filePath}.");
            var jsContent = File.ReadAllText(filePath);
            //logger.LogInformation($"Loaded JS content: {jsContent}");
            return jsContent;
        }
        else
        {
            LogError($"Element JavaScript file not found at {filePath}.");
            return string.Empty;
        }
    }
    public static void SaveNamesToFile(bool debugState,List<string> locations)
    {
        var config = new Config
        {
            Debug = debugState, // Set this as needed
            TeleportLocations = locations, // Assuming 'locations' is still a List<string>
        };

        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        var filePath = GetFilePath("config.json");
        File.WriteAllText(filePath, json);
        LogInfo($"Config saved to {filePath}.");
    }
    // Method to write log messages
    public static void WriteToLog(string message, string level = "INFO")
    {
        try
        {
            // Create a log message with a timestamp and level
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            // Append the log message to the log file
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Handle any errors that occur during logging
            Console.WriteLine($"Failed to write to log: {ex.Message}");
        }
    }

    // Example method to log different levels
    public static void LogInfo(string message)
    {
        WriteToLog(message, "INFO");
    }

    public static void LogError(string message)
    {
        WriteToLog(message, "ERROR");
    }

    public static void LogDebug(string message)
    {
        WriteToLog(message, "DEBUG");
    }
}
