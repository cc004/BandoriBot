using BandoriBot.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace BandoriBot.Config
{
    public abstract class Configuration
    {
        private static List<Configuration> instances = new List<Configuration>();

        public static void Register<T>() where T : Configuration, new()
        {
            configs.Add(typeof(T), new T());
        }

        public static void Register<T>(T t) where T : Configuration
        {
            configs.Add(t.GetType(), t);
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
            Utils.Log(LoggerLevel.Info, $"{GetType().Name} successfully saved");
        }
        public void Load()
        {
            using (FileStream fs = new FileStream(Path.Combine("", Name), FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
                LoadFrom(br);
            Utils.Log(LoggerLevel.Info, $"{GetType().Name} successfully loaded");
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
            foreach (var config in configs)
            {
                try
                {
                    config.Value.Load();
                }
                catch (Exception e)
                {
                    if (!(e is FileNotFoundException))
                    Utils.Log(LoggerLevel.Error, e.ToString());
                    config.Value.LoadDefault();
                }
            }
        }
    }
}
