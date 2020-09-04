using BandoriBot.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace BandoriBot.Config
{
    public abstract class Configuration
    {
        private static List<Configuration> instances = new List<Configuration>();

        public static void Register<T>(T t) where T : Configuration
        {
            instances.Add(t);
        }

        private static Dictionary<Type, Configuration> configs = new Dictionary<Type, Configuration>();

        public static T GetConfig<T>() where T : Configuration
        {
            return configs[typeof(T)] as T;
        }

        public abstract string Name { get; }
        public abstract void SaveTo(BinaryWriter bw);
        public abstract void LoadFrom(BinaryReader br);
        public abstract void LoadDefault();

        public void Save()
        {
            using (FileStream fs = new FileStream(Path.Combine("", Name), FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
                SaveTo(bw);
        }
        public void Load()
        {
            using (FileStream fs = new FileStream(Path.Combine("", Name), FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
                LoadFrom(br);
        }
        public static void SaveAll()
        {
            foreach (Configuration config in instances)
            {
                config.Save();
            }
        }

        public static void Save<T>() where T : Configuration
        {
            GetConfig<T>().Save();
        }

        public static void LoadAll()
        {
            foreach (Configuration config in instances)
            {
                try
                {
                    config.Load();
                    Utils.Log(LoggerLevel.Info, $"{config.GetType().Name} successfully loaded");
                }
                catch (Exception e)
                {
                    Utils.Log(LoggerLevel.Error, e.ToString());
                    config.LoadDefault();
                }
                configs.Add(config.GetType(), config);
            }
        }
    }
}
