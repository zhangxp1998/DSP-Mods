﻿using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using FullSerializer;
using Steamworks;

namespace DysonSphereSave
{
    public static class DysonSphereUtils
    {
        public static void Clear<T>(T[] arr)
        {
            if (arr == null)
            {
                return;
            }
            Array.Clear(arr, 0, arr.Length);
        }
        public static IEnumerable<DysonSphereLayer> GetLayers(DysonSphere sphere)
        {
            for (int i = 0; i < sphere.layersIdBased.Length; i ++)
            {
                var layer = sphere.layersIdBased[i];
                if (layer == null || layer.id != i)
                {
                    continue;
                }
                yield return layer;
            }
        }
        public static IEnumerable<DysonNode> GetNodes(DysonSphereLayer layer)
        {
            for (int i = 0; i < layer.nodeCursor; i++)
            {
                var node = layer.nodePool[i];
                if (node == null || node.id != i)
                {
                    continue;
                }
                yield return node;
            }
        }
        public static IEnumerable<DysonFrame> GetFrames(DysonSphereLayer layer)
        {
            for (int i = 0; i < layer.frameCursor; i++)
            {
                var frame = layer.framePool[i];
                if (frame == null || frame.id != i)
                {
                    continue;
                }
                yield return frame;
            }
        }
        public static IEnumerable<DysonShell> GetShells(DysonSphereLayer layer)
        {
            for (int i = 0; i < layer.shellCursor; i++)
            {
                var shell = layer.shellPool[i];
                if (shell == null || shell.id != i)
                {
                    continue;
                }
                yield return shell;
            }
        }
        public static void ResetSphereProgress(this DysonSphere dysonSphere)
        {
            if (dysonSphere == null)
            {
                return;
            }
            // reset cell point progress
            for (int index1 = 1; index1 < dysonSphere.layersIdBased.Length; ++index1)
            {
                if (dysonSphere.layersIdBased[index1] != null && dysonSphere.layersIdBased[index1].id == index1)
                {
                    DysonShell[] shellPool = dysonSphere.layersIdBased[index1].shellPool;
                    for (int index2 = 1; index2 < dysonSphere.layersIdBased[index1].shellCursor; ++index2)
                    {
                        if (shellPool[index2] != null && shellPool[index2].id == index2)
                        {
                            DysonShell shell = shellPool[index2];
                            shell.cellPoint = 0;
                            Clear(shell.nodecps);
                            Clear(shell.vertcps);
                            shell.vertRecycleCursor = 0;
                            shell.buffer.SetData(shell.vertcps);
                        }
                    }
                }
            }
            // reset structure point progress
            for (int index1 = 1; index1 < dysonSphere.layersIdBased.Length; ++index1)
            {
                DysonSphereLayer dysonSphereLayer = dysonSphere.layersIdBased[index1];
                if (dysonSphereLayer != null && dysonSphereLayer.id == index1)
                {
                    for (int index2 = 1; index2 < dysonSphereLayer.frameCursor; ++index2)
                    {
                        DysonFrame dysonFrame = dysonSphereLayer.framePool[index2];
                        if (dysonFrame != null && dysonFrame.id == index2)
                        {
                            dysonFrame.spA = 0;
                            dysonFrame.spB = 0;
                        }
                        dysonFrame.GetSegments().Clear();
                    }
                    for (int index2 = 1; index2 < dysonSphereLayer.nodeCursor; ++index2)
                    {
                        DysonNode dysonNode = dysonSphereLayer.nodePool[index2];
                        if (dysonNode != null && dysonNode.id == index2)
                        {
                            dysonNode.sp = 0;
                            dysonNode.cpOrdered = 0;
                            dysonNode.spOrdered = 0;
                        }
                    }
                }
            }
            //sync cell buffer
            for (int index1 = 1; index1 < dysonSphere.layersIdBased.Length; ++index1)
            {
                if (dysonSphere.layersIdBased[index1] != null && dysonSphere.layersIdBased[index1].id == index1)
                {
                    DysonShell[] shellPool = dysonSphere.layersIdBased[index1].shellPool;
                    for (int index2 = 1; index2 < dysonSphere.layersIdBased[index1].shellCursor; ++index2)
                    {
                        if (shellPool[index2] != null && shellPool[index2].id == index2)
                        {
                            DysonShell shell = shellPool[index2];
                            shell.SyncCellBuffer();
                        }
                    }
                }
            }
        }
        public const string GUID = "org.zhangxp1998.plugins.dysonspheresave";
    }
    [BepInPlugin(DysonSphereUtils.GUID, "Dyson Sphere Save Plug-In", "1.2.5.0")]
    class SphereProgress : BaseUnityPlugin
    {
        Harmony harmony;
        public static SpherePgoressConfig config;

        internal void Awake()
        {
            config = new SpherePgoressConfig(Config);
            harmony = new Harmony(DysonSphereUtils.GUID);
            harmony.PatchAll(typeof(PatchSphereProgress));
        }
        internal void OnDestroy()
        {
            harmony.UnpatchSelf();  // For ScriptEngine hot-reloading
        }
        public static readonly string DSP_SAVE_PATH = $"{Paths.GameRootPath}/dsp_save.txt";
        class PatchSphereProgress
        {
            public static string Export(DysonSphere dysonSphere)
            {
                System.Console.WriteLine("Auto node count: " + dysonSphere.autoNodeCount);
                foreach (var autoNode in dysonSphere.autoNodes)
                {
                    System.Console.WriteLine(autoNode);
                }
                var memoryStream = new MemoryStream();
                // var deflateStream = new GZipStream(memoryStream, CompressionMode.Compress);
                dysonSphere.Export(new System.IO.BinaryWriter(memoryStream));
                System.Console.WriteLine("Export: raw compressed bits length: " + memoryStream.Length);
                return System.Convert.ToBase64String(memoryStream.ToArray());
            }

            public static void Import(DysonSphere dysonSphere, string dysonSphereData)
            {
                byte[] data = System.Convert.FromBase64String(dysonSphereData);
                System.Console.WriteLine("Import: raw compressed bits length: " + data.Length);
                var memoryStream = new MemoryStream(data);
                // var deflateStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                dysonSphere.Import(new BinaryReader(memoryStream));
            }
            public static string ExportStructure(DysonSphere dysonSphere)
            {
                var structure = DysonSphereStructure.Create(dysonSphere);
                var fs = new fsSerializer();
                fs.TrySerialize(structure, out fsData data).AssertSuccessWithoutWarnings();
                string json = fsJsonPrinter.CompressedJson(data);
                System.Console.WriteLine("Layers: " + structure.layers.Length);
                foreach (var layer in structure.layers)
                {
                    System.Console.WriteLine("Node count: " + layer.nodes.Length);
                    System.Console.WriteLine("Frame count: " + layer.frames.Length);
                    System.Console.WriteLine("Shells count: " + layer.shells.Length);
                }
                System.Console.WriteLine("JSON size: " + json.Length);
                return json;
            }
            public static void RemoveAllLayers(DysonSphere dysonSphere)
            {
                foreach (var layer in DysonSphereUtils.GetLayers(dysonSphere))
                {
                    foreach(var shell in DysonSphereUtils.GetShells(layer))
                    {
                        layer.RemoveDysonShell(shell.id);
                    }
                    foreach(var frame in DysonSphereUtils.GetFrames(layer))
                    {
                        layer.RemoveDysonFrame(frame.id);
                    }
                    foreach (var node in DysonSphereUtils.GetNodes(layer))
                    {
                        layer.RemoveDysonNode(node.id);
                    }
                    dysonSphere.RemoveLayer(layer);
                }
            }
            public static void ImportStructure(DysonSphere dysonSphere, string dysonSphereData)
            {
                var structure = new DysonSphereStructure();
                try
                {
                    var deserializer = new fsSerializer();
                    fsData data = fsJsonParser.Parse(dysonSphereData);
                    data = DysonSphereStructure.Convert(data);
                    if (!deserializer.TryDeserialize(data, ref structure).Succeeded)
                    {
                        System.Console.WriteLine("Failed to deserialize Dyson Sphere data");
                        return;
                    }
                } catch (Exception e)
                {
                    System.Console.WriteLine(e.StackTrace);
                    return;
                }
                RemoveAllLayers(dysonSphere);
                System.Console.WriteLine("Start importing...");
                int nodeCount = 0;
                foreach (var layer in structure.layers)
                {
                    System.Console.Write("Importing layer " + layer.id);
                    int maxId = 0;
                    foreach (var node in layer.nodes)
                    {
                        maxId = Math.Max(maxId, node.id);
                    }
                    int[] nodeIdMap = new int[maxId + 1];
                    var newLayer = dysonSphere.AddLayer(layer.orbitRadius, layer.orbitRotation, layer.orbitAngularSpeed);
                    if (newLayer == null)
                    {
                        System.Console.WriteLine($"Failed to create layer {layer.orbitRadius}, {layer.orbitRotation}, {layer.orbitAngularSpeed}");
                        return;
                    }
                    newLayer.gridMode = layer.gridMode;
                    foreach (var dysonNode in layer.nodes)
                    {
                        nodeIdMap[dysonNode.id] = newLayer.NewDysonNode(0, dysonNode.pos);
                        nodeCount++;
                        if (nodeIdMap[dysonNode.id] <= 0)
                        {
                            System.Console.WriteLine("Failed to Generate node " + dysonNode.id + " pos: " + dysonNode.pos);
                        }
                    }
                    System.Console.WriteLine("Created " + nodeCount + " nodes");
                    foreach (var dysonFrame in layer.frames)
                    {
                        int node1 = nodeIdMap[dysonFrame.nodeAId];
                        int node2 = nodeIdMap[dysonFrame.nodeBId];
                        if (node1 <= 0 || node2 <= 0)
                        {
                            System.Console.WriteLine("Missing node for frame " + dysonFrame.id + " old node: " + dysonFrame.nodeAId + ", " + dysonFrame.nodeBId);
                        }
                        int frameId = newLayer.NewDysonFrame(0, node1, node2, dysonFrame.euler);
                        if (frameId != dysonFrame.id)
                        {
                            System.Console.WriteLine($"Frame id mismatch! Expected {frameId} actual {dysonFrame.id}");
                        }
                    }
                    System.Console.WriteLine("Created " + layer.frames.Length + " frames");


                    foreach (var shell in layer.shells)
                    {
                        List<int> nodeIdList = new List<int>();
                        foreach (var nodeId in shell.nodeIds)
                        {
                            int id = nodeIdMap[nodeId];
                            if (id <= 0)
                            {
                                System.Console.WriteLine("Missing node id " + nodeId);
                            }
                            nodeIdList.Add(id);
                        }
                        int shellId = newLayer.NewDysonShell(shell.protoId, nodeIdList);
                        if (shellId <= 0)
                        {
                            System.Console.WriteLine($"Failed to create shell, returned id {shellId}");
                        }
                    }
                    System.Console.WriteLine("Created " + layer.shells.Length + " shells");
                }
            }
            internal static bool complteSphere = false;

            [HarmonyPostfix, HarmonyPatch(typeof(UIDysonPanel), "_OnOpen")]
            public static void UIDysonPanel_OnOpen_Postfix()
            {
                if (Steamworks.SteamUser.GetSteamID().m_SteamID == 76561199038798439)
                {
                    config.loadHotKey = KeyCode.None;
                    config.saveHotKey = KeyCode.None;
                    config.completeHotKey = KeyCode.None;
                    return;
                }
                config.Update();
                System.Console.WriteLine($"Save: {config.saveHotKey}, Load: {config.loadHotKey}, compelte: {config.completeHotKey}");
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIDysonPanel), "_OnUpdate")]
            public static void UIDysonPanel_OnUpdate_Postfix(UIDysonPanel __instance)
            {
                if (Input.GetKeyDown(config.loadHotKey))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        Import(__instance.viewDysonSphere, GUIUtility.systemCopyBuffer);
                    } else
                    {
                        ImportStructure(__instance.viewDysonSphere, GUIUtility.systemCopyBuffer);
                    }
                    UIRealtimeTip.PopupAhead("Imported dyson sphere data from clipboard!");
                    System.Console.WriteLine("Imported dyson sphere data from clipboard!");
                }
                var dysonSphere = __instance.viewDysonSphere;
                if (Input.GetKeyDown(config.saveHotKey))
                {
                    string data;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        data = Export(dysonSphere);
                    } else
                    {
                        data = ExportStructure(dysonSphere);
                    }
                    GUIUtility.systemCopyBuffer = data;
                    UIRealtimeTip.PopupAhead("Exported dyson sphere data to clipboard!");
                    System.Console.WriteLine("Exported dyson sphere data to clipboard!");
                    using (StreamWriter outputFile = new StreamWriter(DSP_SAVE_PATH))
                    {
                        outputFile.WriteLine(data);
                    }
                }
                if (Input.GetKeyDown(config.completeHotKey))
                {
                    complteSphere = !complteSphere;
                }
                if (complteSphere)
                {
                    buildSphere(dysonSphere);
                }
            }
            public static void buildSphere(DysonSphere dysonSphere)
            {
                if (dysonSphere != null)
                {
                    for (int i = 0; i < dysonSphere.layersIdBased.Length; i++)
                    {
                        var layer = dysonSphere.layersIdBased[i];
                        if (layer == null || layer.id != i)
                        {
                            continue;
                        }
                        // System.Console.WriteLine($"Completing layer {i}");
                        CompleteSphere.CompleteLayer(layer, 1.0f/60.0f/60.0f/2.0f);
                        // CompleteSphere.CompleteLayer(layer, 1.0f);
                    }
                    if (dysonSphere.totalCellPoint == dysonSphere.totalConstructedCellPoint && dysonSphere.totalStructurePoint == dysonSphere.totalConstructedStructurePoint)
                    {
                        complteSphere = false;
                    }
                }
            }
        }
    }
}
