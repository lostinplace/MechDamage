using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Resources;

namespace MechDamage
{
    public struct ChannelType
    {
        public static readonly ChannelType POWER = "Power";
        public static readonly ChannelType TRANSPORT = "Transport";
        public static readonly ChannelType DATA = "Data";

        public String Value { get; private set; }

        private ChannelType(String value)
        {
            Value = value;
        }

        public static implicit operator ChannelType(String value) => new ChannelType(value);
    }
    
    public enum ChannelState
    {
        ACTIVE,
        INACTIVE,
        DESTROYED
    }

    public enum HardpointType
    {
        UPSTREAM,
        DOWNSTREAM
    }

    public struct ChannelSpecifier
    {
        public readonly ushort Power;
        public readonly ushort Data;
        public readonly ushort Transport;

        public ChannelSpecifier(ushort power, ushort data, ushort transport)
        {
            Power = power;
            Data = data;
            Transport = transport;
        }
    }

    public class StructuralComponent
    {
        public Dictionary<HardpointType, StructuralComponent> Hardpoints 
            { get; } = new Dictionary<HardpointType, StructuralComponent>();
        public List<MountedSystem> MountedSystem { get; } = new List<MountedSystem>();
        public String Name { set; get; }
        public String Abbreviation { get; set; }
        public ushort Armor { get; set; }

        private ChannelSpecifier _channels = default(ChannelSpecifier);
        public ChannelSpecifier Channels
        {
            get => _channels;
            set
            {
                var channelLength = value.Power + value.Data +  value.Transport;
                ChannelStatus = Utils.CreateVector(channelLength);
                PowerMask = Utils.CreateVector(value.Power, channelLength);
                DataMask = Utils.CreateVector(value.Power, channelLength-value.Power);
                TransportMask = Utils.CreateVector(value.Transport);
                _channels = value;
            }
        }

        private ushort _structure;

        public ushort Structure
        {
            get => _structure;
            set
            {
                StructureMask = Utils.CreateVector(value, value);
                _structure = value;
            }
        }

        public ulong StructureMask { get; set; }
        public ulong ChannelStatus { get; set; }
        public ulong PowerMask { get; private set; }
        public ulong DataMask { get; private set; }
        public ulong TransportMask { get; private set; }

        public StructuralComponent()
        {
            Structure = 1;
        }
        
    }

    public class MountedSystem : StructuralComponent
    {
        public struct ResourceRequirements
        {
            public readonly HashSet<ChannelType> Channels;
            public readonly HashSet<ResourceType> Resource;
            
            public ResourceRequirements(HashSet<ChannelType> channels, ResourceType resource)
            {
                Channels = channels;
                Resource = new HashSet<ResourceType>(){resource};
            }
            
            public ResourceRequirements(HashSet<ChannelType> channels, HashSet<ResourceType> resources)
            {
                Channels = channels;
                Resource = resources;
            }
            
            public static implicit operator ResourceRequirements(HashSet<ChannelType> channels) =>
                new ResourceRequirements(channels, new HashSet<ResourceType>());
            public static implicit operator ResourceRequirements(ResourceType resource) =>
                new ResourceRequirements(new HashSet<ChannelType>(), resource);
            public static implicit operator ResourceRequirements(HashSet<ResourceType> resources) =>
                new ResourceRequirements(new HashSet<ChannelType>(), resources);

            public static ResourceRequirements operator |(ResourceRequirements a, ResourceRequirements b)
            {
                var channels = a.Channels.Union(b.Channels).ToHashSet();
                var resources = a.Resource.Union(b.Resource).ToHashSet();
                return new ResourceRequirements(channels, resources);
            }
            
            
        }
        
        public struct ResourceType
        {
            public readonly string Type;
            public readonly string Designation;
            public readonly int Amount;

            public ResourceType(string type, string designation="", int amount=0)
            {
                Type = type;
                Designation = designation;
                Amount = amount;
            }
        }
        
        public String SystemType { get; set; }
        public ResourceType Resource { get; set; }
        public ResourceRequirements Requirements { get; set; }
        public StructuralComponent HostComponent { get; set; }
    }

    public class Core : StructuralComponent
    {
        
    }

    public static class Utils
    {
        private static readonly int[] S = {1, 2, 4, 8, 16}; // Magic Binary Numbers
        private static readonly ulong[] B = {0x55555555, 0x33333333, 0x0F0F0F0F, 0x00FF00FF, 0x0000FFFF};
        
        /// <summary>
        /// cribbed from https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
        /// </summary>
        public static int Cardinality(this ulong v)
        {
            ulong c; // store the total here
            c = v - ((v >> 1) & B[0]);
            c = ((c >> S[1]) & B[1]) + (c & B[1]);
            c = ((c >> S[2]) + c) & B[2];
            c = ((c >> S[3]) + c) & B[3];
            c = ((c >> S[4]) + c) & B[4];
            return (int)c;
        }
        
        public static ulong CreateVector(int numberOfSetBits, int length=0)
        {
            
            if (length == 0) length = numberOfSetBits;
            if (length > 64) throw new ArgumentOutOfRangeException("length must be greater than 0 and less than 64");
            var rightPadding = length - numberOfSetBits;
            var baseVec = (ulong)Math.Pow(2, numberOfSetBits) - 1;
            return baseVec << rightPadding;
        }

        public static bool SatisfiesRequirement(this MountedSystem.ResourceType resource,
            MountedSystem.ResourceRequirements requirements)
        {
            var result = true;
            foreach (var requirement in requirements.Resource)
            {
                result &= (requirement.Type == "" || requirement.Type == resource.Type);
                result &= (requirement.Designation == "" || requirement.Designation == resource.Designation);
            }

            return result;
        }
    }
}