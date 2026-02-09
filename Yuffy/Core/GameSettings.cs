using System;
using System.IO;
using System.Text.Json;

namespace Yuffy.Core;

public class GameSettings
{
    private int _windowWidth = 1280;
    private int _windowHeight = 720;
    private float _musicVolume = 0.5f;
    private float _sfxVolume = 1.0f;

    public int WindowWidth
    {
        get => _windowWidth;
        set => _windowWidth = Math.Max(640, value);
    }

    public int WindowHeight
    {
        get => _windowHeight;
        set => _windowHeight = Math.Max(480, value);
    }

    public bool Fullscreen { get; set; }

    public float MusicVolume
    {
        get => _musicVolume;
        set => _musicVolume = Math.Clamp(value, 0f, 1f);
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = Math.Clamp(value, 0f, 1f);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static string FilePath => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    public static GameSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<GameSettings>(json, JsonOptions) ?? new GameSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }
        return new GameSettings();
    }

    public void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(this, JsonOptions);
            string tempPath = FilePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, FilePath, overwrite: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}
