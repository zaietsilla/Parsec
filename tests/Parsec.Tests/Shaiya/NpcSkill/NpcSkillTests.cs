﻿using System.Linq;
using Parsec.Common;
using Parsec.Shaiya.NpcSkill;

namespace Parsec.Tests.Shaiya.NpcSkill;

public class NpcSkillTests
{
    [Theory]
    [InlineData("NpcSkillEp5", Episode.EP5)]
    [InlineData("NpcSkillEp6", Episode.EP6)]
    public void NpcSkillTest(string fileName, Episode episode)
    {
        string filePath = $"Shaiya/NpcSkill/{fileName}.SData";
        string outputPath = $"Shaiya/NpcSkill/output_{fileName}.SData";
        string jsonPath = $"Shaiya/NpcSkill/{fileName}.SData.json";
        string csvPath = $"Shaiya/NpcSkill/{fileName}.SData.csv";

        var npcSkill = Reader.ReadFromFile<Parsec.Shaiya.Skill.Skill>(filePath, episode);
        npcSkill.Write(outputPath, episode);
        npcSkill.WriteJson(jsonPath);
        npcSkill.WriteCsv(csvPath);

        var outputSkill = Reader.ReadFromFile<Parsec.Shaiya.Skill.Skill>(outputPath, episode);
        var jsonSkill = Reader.ReadFromJsonFile<Parsec.Shaiya.Skill.Skill>(jsonPath);
        var csvSkill = Parsec.Shaiya.Skill.Skill.ReadFromCsv(csvPath, episode);

        var expected = npcSkill.GetBytes().ToList();
        Assert.Equal(expected, outputSkill.GetBytes());
        Assert.Equal(expected, jsonSkill.GetBytes());
        Assert.Equal(expected, csvSkill.GetBytes());
    }

    [Fact]
    public void DbSkillTest()
    {
        const string filePath = "Shaiya/NpcSkill/DBNpcSkillData.SData";
        const string outputPath = "Shaiya/NpcSkill/output_DBNpcSkillData.SData";
        const string jsonPath = "Shaiya/NpcSkill/DBNpcSkillData.SData.json";
        const string csvPath = "Shaiya/NpcSkill/DBNpcSkillData.SData.csv";

        var npcSkill = Reader.ReadFromFile<DBNpcSkillData>(filePath);
        npcSkill.Write(outputPath);
        npcSkill.WriteJson(jsonPath);
        npcSkill.WriteCsv(csvPath);

        var outputNpcSkill = Reader.ReadFromFile<DBNpcSkillData>(outputPath);
        var jsonNpcSkill = Reader.ReadFromJsonFile<DBNpcSkillData>(jsonPath);
        var csvNpcSkill = DBNpcSkillData.ReadFromCsv<DBNpcSkillData>(csvPath);

        var expected = npcSkill.GetBytes().ToList();
        Assert.Equal(expected, outputNpcSkill.GetBytes());
        Assert.Equal(expected, jsonNpcSkill.GetBytes());
        // For the csv check, skip the 128-byte header, which gets lost in the process
        Assert.Equal(expected.Skip(128), csvNpcSkill.GetBytes().Skip(128));
    }

    [Fact]
    public void DbSkillTextTest()
    {
        const string filePath = "Shaiya/NpcSkill/DBNpcSkillText_USA.SData";
        const string outputPath = "Shaiya/NpcSkill/output_DBNpcSkillText_USA.SData";
        const string jsonPath = "Shaiya/NpcSkill/DBNpcSkillText_USA.SData.json";
        const string csvPath = "Shaiya/NpcSkill/DBNpcSkillText_USA.SData.csv";

        var npcSkillText = Reader.ReadFromFile<DBNpcSkillText>(filePath);
        npcSkillText.Write(outputPath);
        npcSkillText.WriteJson(jsonPath);
        npcSkillText.WriteCsv(csvPath);

        var outputNpcSkillText = Reader.ReadFromFile<DBNpcSkillText>(outputPath);
        var jsonNpcSkillText = Reader.ReadFromJsonFile<DBNpcSkillText>(jsonPath);
        var csvNpcSkillText = DBNpcSkillText.ReadFromCsv<DBNpcSkillText>(csvPath);

        var expected = npcSkillText.GetBytes().ToList();
        Assert.Equal(expected, outputNpcSkillText.GetBytes());
        Assert.Equal(expected, jsonNpcSkillText.GetBytes());
        // For the csv check, skip the 128-byte header, which gets lost in the process
        Assert.Equal(expected.Skip(128), csvNpcSkillText.GetBytes().Skip(128));
    }
}
