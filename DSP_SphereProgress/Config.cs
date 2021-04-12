using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;

namespace DysonSphereSave
{
    public class SpherePgoressConfig
    {
        private ConfigEntry<KeyCode> saveKeyCode;
        private ConfigEntry<KeyCode> loadKeyCode;
        private ConfigEntry<KeyCode> completeKeyCode;
        public KeyCode saveHotKey;
        public KeyCode loadHotKey;
        public KeyCode completeHotKey;
        public SpherePgoressConfig(ConfigFile Config)
        {
            saveKeyCode = Config.Bind<KeyCode>("DysonSphereSave", "saveKeyCode", KeyCode.S, "Hot key to save a dyson sphere");
            loadKeyCode = Config.Bind<KeyCode>("DysonSphereSave", "loadKeyCode", KeyCode.S, "Hot key to load a dyson sphere");
            completeKeyCode = Config.Bind<KeyCode>("DysonSphereSave", "loadKeyCode", KeyCode.S, "Hot key to complete a dyson sphere");
        }
        public void Update()
        {
            saveHotKey = saveKeyCode.Value;
            loadHotKey = loadKeyCode.Value;
            completeHotKey = completeKeyCode.Value;
        }
    }
}
