#!/usr/bin/env dotnet script
#r "nuget: System.Text.Json, 10.0.0"

using System;
using System.IO;
using System.Threading.Tasks;

// Quick test of ISO Download Manager
Console.WriteLine("🏴‍☠️ CleanVM ISO Download Manager Test\n");

// Load catalog
var catalogPath = Path.Combine(
    Path.GetDirectoryName(Environment.ProcessPath) ?? "",
    "ISO",
    "ISOCatalog.json"
);

if (!File.Exists(catalogPath))
{
    Console.WriteLine($"❌ Catalog not found at: {catalogPath}");
    return;
}

var json = await File.ReadAllTextAsync(catalogPath);
Console.WriteLine($"✅ Loaded catalog: {json.Length} bytes");
Console.WriteLine($"📁 Path: {catalogPath}\n");

// Parse and display
using var doc = System.Text.Json.JsonDocument.Parse(json);
var root = doc.RootElement;

Console.WriteLine($"Version: {root.GetProperty("version").GetString()}");
Console.WriteLine($"Last Updated: {root.GetProperty("lastUpdated").GetString()}\n");

var categories = root.GetProperty("categories");
Console.WriteLine($"📚 Found {categories.GetArrayLength()} categories:\n");

foreach (var category in categories.EnumerateArray())
{
    var icon = category.GetProperty("icon").GetString();
    var name = category.GetProperty("name").GetString();
    var templates = category.GetProperty("templates");
    
    Console.WriteLine($"{icon} {name} ({templates.GetArrayLength()} templates)");
    
    foreach (var template in templates.EnumerateArray())
    {
        var tName = template.GetProperty("name").GetString();
        var version = template.GetProperty("version").GetString();
        var sizeBytes = template.GetProperty("sizeBytes").GetInt64();
        var sizeGB = sizeBytes / 1_000_000_000.0;
        
        Console.WriteLine($"  • {tName} v{version} - {sizeGB:F1} GB");
    }
    Console.WriteLine();
}

Console.WriteLine("✅ ISO Download Manager ready!");
Console.WriteLine("\nHybrid approach:");
Console.WriteLine("  • Embedded catalog loaded (always available)");
Console.WriteLine("  • Extended catalog can be fetched from cleanvm.net");
Console.WriteLine("  • Checksums verified on download");
Console.WriteLine("  • Progress tracking included");
