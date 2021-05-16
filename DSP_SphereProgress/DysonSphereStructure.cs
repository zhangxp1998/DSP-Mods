using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DysonSphereSave
{
    [Serializable]
    public class DysonSphereStructure
    {
        // Convert old style IList<T> deserialized data to raw array deserialized data
        public static fsData Convert(fsData data)
        {
            if (data.IsDictionary)
            {
                var map = data.AsDictionary;
                if (map.ContainsKey("$content"))
                {
                    System.Console.WriteLine("Converted " + map["$type"]);
                    return Convert(map["$content"]);
                }
                foreach (var key in new List<string>(map.Keys))
                {
                    map[key] = Convert(map[key]);
                }
                return data;
            }
            else if (data.IsList)
            {
                var list = data.AsList;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = Convert(list[i]);
                }
                return data;
            }
            return data;
        }
        public DysonLayerStructure[] layers { get; set; }
        public float minOrbitRadius { get; set; }
        public float maxOrbitRadius { get; set; }
        public float dysonLumino { get; set; }
        public float dysonRadius { get; set; }
        public float radius { get; set; }
        public float physicsRadius { get; set; }
        public float systemRadius { get; set; }

        public static DysonSphereStructure Create(DysonSphere obj)
        {
            var structure = new DysonSphereStructure();
            var layers = new List<DysonLayerStructure>();
            for (int i = 0; i < obj.layersIdBased.Length; i ++)
            {
                var layer = obj.layersIdBased[i];
                if (layer == null || layer.id != i)
                {
                    System.Console.WriteLine("layer = " + layer);
                    if (layer != null)
                    {
                        System.Console.WriteLine("layer.id = " + layer.id);
                    }
                    continue;
                }
                layers.Add(DysonLayerStructure.Create(layer));
            }
            structure.layers = layers.ToArray();
            structure.maxOrbitRadius = obj.maxOrbitRadius;
            structure.minOrbitRadius = obj.minOrbitRadius;
            structure.dysonLumino = obj.starData.dysonLumino;
            structure.radius = obj.starData.radius;
            structure.dysonRadius = obj.starData.dysonRadius;
            structure.systemRadius = obj.starData.systemRadius;
            structure.physicsRadius = obj.starData.physicsRadius;
            return structure;
        }

    }
    [Serializable]
    public class DysonLayerStructure
    {
        public float orbitRadius { get; set; }
        public Quaternion orbitRotation { get; set; }
        public float orbitAngularSpeed { get; set; }
        public int gridMode { get; set; }

        public int id { get; set; }
        public DysonNodeStructure[] nodes;
        public DysonFrameStructure[] frames;
        public DysonShellStructure[] shells;

        public static DysonLayerStructure Create(DysonSphereLayer obj)
        {
            DysonLayerStructure structure = new DysonLayerStructure();
            structure.orbitRadius = obj.orbitRadius;
            structure.orbitAngularSpeed = obj.orbitAngularSpeed;
            structure.orbitRotation = obj.orbitRotation;
            structure.gridMode = obj.gridMode;
            var nodes = new List<DysonNodeStructure>();
            for (int i = 0; i < obj.nodeCursor; i ++)
            {
                var node = obj.nodePool[i];
                if (node == null || node.id != i)
                {
                    continue;
                }
                nodes.Add(DysonNodeStructure.Create(node));
            }
            structure.nodes = nodes.ToArray();
            var frames = new List<DysonFrameStructure>();
            for (int i = 0; i < obj.frameCursor; i++)
            {
                var frame = obj.framePool[i];
                if (frame == null || frame.id != i)
                {
                    continue;
                }
                frames.Add(DysonFrameStructure.Create(frame));
            }
            structure.frames = frames.ToArray();
            var shells = new List<DysonShellStructure>();
            for (int i = 0; i < obj.shellCursor; i++)
            {
                var shell = obj.shellPool[i];
                if (shell == null || shell.id != i)
                {
                    continue;
                }
                shells.Add(DysonShellStructure.Create(shell));
            }
            structure.shells = shells.ToArray();
            structure.id = obj.id;
            return structure;
        }
    }
    [Serializable]
    public class DysonNodeStructure
    {
        public Vector3 pos { get; set; }
        public int id { get; set; }
        public int protoId { get; set; }
        public static DysonNodeStructure Create(DysonNode node)
        {
            DysonNodeStructure structure = new DysonNodeStructure();
            structure.pos = node.pos;
            structure.id = node.id;
            structure.protoId = node.protoId;
            return structure;
        }
    }
    [Serializable]
    public class DysonFrameStructure
    {
        public int nodeAId { get; set; }
        public int nodeBId { get; set; }
        public bool euler { get; set; }

        public int id { get; set; }
        public int protoId { get; set; }
        public static DysonFrameStructure Create(DysonFrame obj)
        {
            DysonFrameStructure structure = new DysonFrameStructure();
            structure.nodeAId = obj.nodeA.id;
            structure.nodeBId = obj.nodeB.id;
            structure.euler = obj.euler;

            structure.id = obj.id;
            structure.protoId = obj.protoId;
            return structure;
        }
    }
    [Serializable]
    public class DysonShellStructure
    {
        public int id { get; set; }
        public int protoId { get; set; }

        public int[] nodeIds { get; set; }
        public static DysonShellStructure Create(DysonShell obj)
        {
            DysonShellStructure structure = new DysonShellStructure();

            structure.nodeIds = obj.nodes.ConvertAll(n => n.id).ToArray();

            structure.id = obj.id;
            structure.protoId = obj.protoId;
            return structure;
        }
    }
}
