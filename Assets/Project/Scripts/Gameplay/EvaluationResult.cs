using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    public class EvaluationResult
    {
        public string missionId, missionTitle;
        public GameMode mode;
        public Difficulty difficulty;

        public bool bootable, timedOut, isNewRecord;
        public int compatibilityScore, requirementsScore, budgetScore, timeScore, penalty;
        public int budget, spent, buildPrice;
        public float elapsedSeconds;

        public string headline;
        public List<string> detailLines = new List<string>();

        public List<string> unmetRequirements = new List<string>();
        public List<string> issues = new List<string>();

        public int Total => Mathf.Max(0, compatibilityScore + requirementsScore + budgetScore + timeScore - penalty);
    }
}
