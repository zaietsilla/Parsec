﻿using System;
using Parsec.Shaiya.Data;
using Parsec.Shaiya.Svmap;

namespace Sample.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        #region Read Data

        // Read data from .sah and .saf files
        Parsec.Shaiya.Data.Data data = new("data.sah", "data.saf");

        // Find the file you want to extract with it's full relative path
        var file = data.GetFile("world/2.svmap");
        // Extract the selected file
        data.Extract(file, "extracted");

        // Read and parse the file's content directly from the saf file
        Svmap svmap = Parsec.ParsecReader.FromBuffer<Svmap>(file.Name, data.GetFileBuffer(file));

        Console.WriteLine($"File: {svmap.FileName}");
        Console.WriteLine($"MapSize: {svmap.MapSize}");
        Console.WriteLine($"Ladder Count: {svmap.Ladders.Count}");
        Console.WriteLine($"Monster Area Count: {svmap.MonsterAreas.Count}");
        Console.WriteLine($"Npc Count: {svmap.Npcs.Count}");
        Console.WriteLine($"Portal Count: {svmap.Portals.Count}");
        Console.WriteLine($"Spawn Count: {svmap.Spawns.Count}");
        Console.WriteLine($"Named Area Count: {svmap.NamedAreas.Count}");

        #endregion

        #region Create Data/Patch

        // Create patch data
        Parsec.Shaiya.Data.Data createdData = DataBuilder.CreateFromDirectory("input", "output");

        Console.WriteLine($"Data file count: {createdData.FileCount}");

        #endregion
    }
}
