using System;
using System.Collections.Generic;

namespace MestreDoPC
{
    [Serializable]
    public class RecordEntry
    {
        public string mode;
        public string difficulty;
        public string missionId;
        public int score;
        public float timeSeconds;
        public int budgetSpent;
        public string date;
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public List<string> completedMissionIds = new List<string>();
        public List<RecordEntry> records = new List<RecordEntry>();
    }
}
