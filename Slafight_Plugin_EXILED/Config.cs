using System;
using System.ComponentModel;
using System.IO;
using Slafight_Plugin_EXILED.SpecialEvents;

namespace Slafight_Plugin_EXILED;

using Exiled.API.Interfaces;
public class Config : IConfig
{
    [Description("Set Enable or Disable")]
    public bool IsEnabled { get; set; } = true;
    [Description("Show Debug Logs?")]
    public bool Debug { get; set; } = true;
    [Description("Server Specific Season. 0=Normal,1=Halloween,2=Christmas,3=April,4=FifthFestival")]
    public int Season { get; set; } = 4;
        
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
    public bool EventAllowed { get; set; } = true;
    [Description("各 SpecialEvent の抽選重み設定")]
    public SpecialEventWeightContext EventWeights { get; set; } = new();
    [Description("")]
    public float OwBoomTime { get; set; } = 160f;
    [Description("")]
    public float DwBoomTime { get; set; } = 100f;
}