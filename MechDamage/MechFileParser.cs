using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using YamlDotNet.Serialization;

namespace MechDamage
{
    public static class MechFileParser
    {
        public static Dictionary<String, HashSet<ChannelType>> DefaultChannelRequirements
            { get; } = 
            new Dictionary<String, HashSet<ChannelType>>()
            {
                {"Actuator", new HashSet<ChannelType>() {ChannelType.DATA, ChannelType.POWER}}
            };
        
        public static Core ParseFile(string filepath)
        {
            string contents = System.IO.File.ReadAllText(filepath);
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<Dictionary<String, object>>(contents);
            
            return ConfigToCore(result);
        }

        private static Core ConfigToCore(Dictionary<String, object> config)
        {
            var t = (Dictionary<object, object>)config["Configuration"];
            var coreConfig = (Dictionary<object, object>)t["Core"];
            var result = coreConfig.ToStructuralComponent<Core>();
            return result;
        }

        public static List<T> ToComponentList<T>(this List<object> systemList) where T : StructuralComponent, new()
        {
            var results = systemList.Select(x =>
                ((Dictionary<object, object>) x).ToStructuralComponent<T>());
            return results.ToList();
        }

        private static ushort ToUshort(this object text) => ushort.Parse(text.ToString());
        
        private static ChannelSpecifier ToChannels(
            this Dictionary<object, object> ChannelConfig)
        {
            var transport = ChannelConfig.GetValueOrDefault("Transport", "0").ToUshort();
            var data = ChannelConfig.GetValueOrDefault("Data", "0").ToUshort();
            var power = ChannelConfig.GetValueOrDefault("Power", "0").ToUshort();
            return new ChannelSpecifier(power, data, transport);
        }

        public static MountedSystem.ResourceType ToResource(
            this Dictionary<object, object> componentConfig)
        {
            
            var typeText = componentConfig.GetValueOrDefault("Type", "").ToString();
            var designationText = componentConfig.GetValueOrDefault("Designation", "")
                .ToString();
            var amountText = componentConfig.GetValueOrDefault("Amount", "0").ToString();
            
            return new MountedSystem.ResourceType(typeText, designationText, int.Parse(amountText));
        }

        public static MountedSystem.ResourceRequirements
            ToRequirements(this object requirementsList, String systemType="")
        {
            MountedSystem.ResourceRequirements defaultRequirements =
                DefaultChannelRequirements.GetValueOrDefault(systemType, new HashSet<ChannelType>());
            
            
            switch (requirementsList)
            {
                case String s:
                    return defaultRequirements | new HashSet<ChannelType>() {s};
                case Dictionary<object, object> d:
                    return defaultRequirements | ResourceRequirementsFromDict(d);
                case List<object> l:
                    return defaultRequirements | ResourceRequirementsFromList(l);
                
            }

            return default(MountedSystem.ResourceRequirements);
        }
        
        public static MountedSystem.ResourceRequirements ResourceRequirementsFromDict(
            Dictionary<object, object> requirementsDict)
        {
            MountedSystem.ResourceRequirements result = new HashSet<ChannelType>();
            foreach (var key in requirementsDict.Keys)
            {
                var tmp = new MountedSystem.ResourceType(key.ToString(),
                    requirementsDict[key].ToString());
                result |= tmp;
            }
            return result;
        }

        public static MountedSystem.ResourceRequirements
            ResourceRequirementsFromList(List<object> requirementsList)
        {
            MountedSystem.ResourceRequirements result = new HashSet<ChannelType>();
            foreach (var item in requirementsList)
            {
                switch (item)
                {
                    case String s:
                        result |= new HashSet<ChannelType>() {s};
                        continue;
                    case Dictionary<object, object> d:
                        result |= ResourceRequirementsFromDict(d);
                        continue;
                }
            }
            return result;
        }
        
        public static (HashSet<ChannelType> channels, MountedSystem.ResourceType resource) 
            ToRequirements(this List<object> requirementsList, String systemType)
        {
            var channels = new HashSet<ChannelType>();
            MountedSystem.ResourceType resource = default(MountedSystem.ResourceType);
            foreach (var item in requirementsList)
            {
                switch (item)
                {
                    case String s: channels.Add(s);
                        continue;
                    case Dictionary<object, object> dict:
                        resource = dict.ToResource();
                        continue;
                }
            }
            return (channels=channels, resource=resource);
        }
        
        public static MountedSystem ToMountedSystem(
            this Dictionary<object, object> componentConfig,
            StructuralComponent hostComponent=default(StructuralComponent))
        {
            var result = componentConfig.ToStructuralComponent<MountedSystem>();
            var typeText = componentConfig.GetValueOrDefault("Type", "").ToString();
            var resourceDict = (Dictionary<object, object>)componentConfig.GetValueOrDefault("Resource", 
                new Dictionary<object, object>());
            var requirementsRaw = componentConfig.GetValueOrDefault("Requires", 
                new List<object>());
            var requirements = requirementsRaw.ToRequirements(typeText);
            result.SystemType = typeText;
            result.HostComponent = hostComponent;
            result.Resource = resourceDict.ToResource();
            result.Requirements = requirements;
            return result;
        }

        public static T ToStructuralComponent<T>(
            this Dictionary<object, object> componentConfig) where T: StructuralComponent, new()
        {
            var componentName = componentConfig.GetValueOrDefault("Name", "").ToString();
            var structure = componentConfig.GetValueOrDefault("Structure", "1").ToUshort();
            var armor = componentConfig.GetValueOrDefault("Armor", "0").ToUshort();
            var abbreviation = componentConfig.GetValueOrDefault("abbr", "").ToString();
            var channelsDict =
                (Dictionary<object, object>) componentConfig.GetValueOrDefault("Channels",
                    new Dictionary<object, object>());
            var channels = channelsDict.ToChannels();

            var result = new T()
            {
                Name = componentName,
                Abbreviation = abbreviation,
                Structure = structure,
                Armor = armor,
                Channels = channels
            };
            return result;
        }
    }
}