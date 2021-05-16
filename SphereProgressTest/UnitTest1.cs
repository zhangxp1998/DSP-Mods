using NUnit.Framework;
using DysonSphereSave;
using FullSerializer;
using System.IO;
using System.Collections.Generic;

namespace SphereProgressTest
{
    public class Tests
    {
        
        [Test]
        public void Test1()
        {
            string small_data = File.ReadAllText("small_sphere.json");
            DysonSphereStructure structure = new DysonSphereStructure();
            var deserializer = new fsSerializer();
            fsData data = fsJsonParser.Parse(small_data);
            data = DysonSphereStructure.Convert(data);
            System.Console.WriteLine(data);
            var res = deserializer.TryDeserialize(data, ref structure);
            Assert.IsTrue(res.Succeeded);
        }

        [Test]
        public void Test2()
        {
            string small_data = File.ReadAllText("text_sphere.json");
            DysonSphereStructure structure = new DysonSphereStructure();
            var deserializer = new fsSerializer();
            fsData data = fsJsonParser.Parse(small_data);
            var res = deserializer.TryDeserialize(data, ref structure);
            Assert.IsTrue(res.Succeeded);
        }
    }
}