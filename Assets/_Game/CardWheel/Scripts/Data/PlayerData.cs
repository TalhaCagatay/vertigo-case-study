using System;
using UnityEngine;

namespace _Game.CardWheel.Data
{
    /// <summary>
    /// This is not a place to save, a better approach would be having a saving system and passing PlayerData to it and save it.
    /// So the correct usage would be using PlayerData as a model class and having a separate saving system to save it both locally and to cloud.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public void Save(string id, int value)
        {
            PlayerPrefs.SetInt(id, value);
        }

        public int Load(string id, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(id, defaultValue);
        }
    }
}
