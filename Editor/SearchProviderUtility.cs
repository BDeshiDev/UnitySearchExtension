using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

namespace Editor
{
    public static class SearchProviderUtility
    {
        //since unity fuzzy match wants a list.
        static List<int> matchesCache = new();

        public static bool FuzzyMatch(string source, string target, out int score)
        {
            //#TODO need a better fuzzy match. this is bad for short matches and doesn't consider space splitting      
            return LevenshteinFuzzyMatch(source, target, out score);
            //unity has a  fuzzy match function but I don't like the output
            // return UnityFuzzyMatch(source, target, out score);
        }

        public static bool UnityFuzzyMatch(string source, string target, out int score)
        {
            long longScore = 0;
            matchesCache.Clear();
            if (FuzzySearch.FuzzyMatch(target, source, ref longScore, matchesCache))
            {
                score = (int)longScore;
                return true;
            }

            score = 6969;
            return false;
        }

        private static bool LevenshteinFuzzyMatch(string source, string target, out int score)
        {
            var levenshteinDistance = LevenshteinDistance(source, target);
            score = levenshteinDistance;
            return levenshteinDistance <= (Mathf.Min(source.Length, target.Length) / 2) || 
                   //include as entry if it contains target in someform. hack for small matches
                   target.Length > 4 && source.Contains(target);
        }


        public static int LevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return string.IsNullOrEmpty(b) ? 0 : b.Length;
            if (string.IsNullOrEmpty(b)) return a.Length;

            var costs = new int[b.Length + 1];
            for (int i = 0; i <= a.Length; i++)
            {
                int lastValue = i;
                for (int j = 0; j <= b.Length; j++)
                {
                    if (i == 0)
                        costs[j] = j;
                    else if (j > 0)
                    {
                        int newValue = costs[j - 1];
                        if (a[i - 1] != b[j - 1])
                            newValue = Mathf.Min(Mathf.Min(newValue, lastValue), costs[j]) + 1;
                        costs[j - 1] = lastValue;
                        lastValue = newValue;
                    }
                }

                if (i > 0)
                    costs[b.Length] = lastValue;
            }

            return costs[b.Length];
        }
    }
}