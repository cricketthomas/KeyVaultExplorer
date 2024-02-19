﻿namespace kvexplorer.shared.Models;
using System.ComponentModel.DataAnnotations;

public class AppSettings
{
    public bool BackgroundTransparency { get; set; }
    public int ClipboardTimeout { get; set; }

    [AllowedValues("Left", "Auto", "Top")]
    public string NavigationLayout { get; set; } = "Left";


}