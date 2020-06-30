using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using MechDamage;
using NUnit.Framework;
using YamlDotNet.Serialization;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            
            var core = MechDamage.MechFileParser.ParseFile("TestAssets/TestMech.yml");
            var t = new MountedSystem()
            {
                SystemType = "test"
            };
            
            
            Assert.Pass();
        }
        
        [Test]
        public void TestParsingSimpleGenerator()
        {
            string contents = System.IO.File.ReadAllText("TestAssets/TestGenerator.yml");
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<Dictionary<object, object>>(contents);

            var actual = result.ToMountedSystem();

            Assert.AreEqual("Generator 1", actual.Name);
            Assert.AreEqual("Generator", actual.SystemType);
            Assert.AreEqual(null, actual.HostComponent);
            Assert.AreEqual("GEN1", actual.Abbreviation);
        }
        
        [Test]
        public void TestParsingAmmo()
        {
            string contents = System.IO.File.ReadAllText("TestAssets/TestAmmo.yml");
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<Dictionary<object, object>>(contents);

            var actual = result.ToMountedSystem();

            Assert.AreEqual("Machine Gun Ammo", actual.Name);
            
            var expected = new MountedSystem.ResourceType("Ammo", "5.56mm", 1000);
            Assert.AreEqual(expected, actual.Resource);
        }
        
        [Test]
        public void TestParsingMachineGun()
        {
            string contents = System.IO.File.ReadAllText("TestAssets/TestMachineGun.yml");
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<Dictionary<object, object>>(contents);

            var actual = result.ToMountedSystem();

            Assert.AreEqual("5.56 Light Machine Gun", actual.Name);
            Assert.AreEqual("Weapon", actual.SystemType);
            Assert.IsTrue(actual.Requirements.Channels.Contains("Data"));

            var expected = new MountedSystem.ResourceType("Ammo", "5.56mm", 1000);
            Assert.AreEqual(expected, actual.Resource);
            Assert.IsTrue(expected.SatisfiesRequirement(actual.Requirements));
        }

        [Test]
        public void BitVectorConstructionTest()
        {
            Assert.AreEqual(1, Utils.CreateVector(1, 1));
            Assert.AreEqual(3, Utils.CreateVector(2, 2));
            Assert.AreEqual(56, Utils.CreateVector(3, 6));
            Assert.AreEqual(60, Utils.CreateVector(4, 6));

            var vecA = Utils.CreateVector(4, 10);
            var vecB = Utils.CreateVector(3);
            var vecC = vecA | vecB;
            Assert.AreEqual(7, vecC.Cardinality());
        }
    }
}