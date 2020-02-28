﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;


public class AndroidManifestPlaceholdersPostprocess : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 999;


    void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
    {
        var xmls = GetAndroidManifestPlaceholdersXMLsInProject();
        if (!xmls.Any()) return;
        
        var gradlePath = path + "/build.gradle";
        var placeHolders = new Dictionary<string, object>();
        
        if(!File.Exists(gradlePath))
            throw new Exception("build.gradle does not exists and you are trying to modify it using AndroidManifestPlaceHolders.xml");
        
        foreach (var holdersXml in xmls)
        {
            var doc = new XmlDocument();
            doc.Load(holdersXml);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                placeHolders.Add(node.Name, node.InnerText);
            }
        }

        var writer = File.ReadAllText(gradlePath);

        var keyValuesAsStrings = placeHolders.Aggregate("", (s, pair) =>
        {
            var value = pair.Value;
            if (value is string)
            {
                value = $"\'{value}\'";
            }

            return s + $"{pair.Key}:{value},\n";
        });

        var result = writer.Replace("##MANIFEST_PLACEHOLDERS##",
            "manifestPlaceholders = [\n " + keyValuesAsStrings + "]");

        File.WriteAllText(gradlePath, result);
    }

    private static string[] GetAndroidManifestPlaceholdersXMLsInProject()
    {
        return Directory.GetFiles(Application.dataPath, "**AndroidManifestPlaceholders.xml",
            SearchOption.AllDirectories);
    }
}