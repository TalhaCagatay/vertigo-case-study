using System;
using System;
using UnityEngine;

namespace _Game.CardWheel.Data
{
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
