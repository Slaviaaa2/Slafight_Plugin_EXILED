using System;
using System.ComponentModel;
using System.IO;

namespace Slafight_Plugin_EXILED;

using Exiled.API.Interfaces;
public class Config : IConfig
{
    [Description("Set Enable or Disable")]
    public bool IsEnabled { get; set; } = true;
    [Description("Show Debug Logs?")]
    public bool Debug { get; set; } = true;
    [Description("Please Set Season Info. 0=normal,1=halloween,2=christmas,3=april")]
    public int Season { get; set; } = 0;
        
    [Description("")]
    private string _audioReferences;
    public string AudioReferences 
    { 
        get => string.IsNullOrEmpty(_audioReferences) 
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED", "ServerContents")
            : _audioReferences;
        set => _audioReferences = value;
    }
        
    [Description("")]
    public bool WarheadLockAllowed { get; set; } = true;
    [Description("")]
    public float WarheadLockTimeMultiplier { get; set; } = 0.75f;
        
    [Description("")]
    public bool EventAllowed { get; set; } = true;
    [Description("")]
    public float OwBoomTime { get; set; } = 160f;
    [Description("")]
    public float DwBoomTime { get; set; } = 100f;
}