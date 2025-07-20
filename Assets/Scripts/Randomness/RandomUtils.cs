using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Randomness
{
    public static class RandomUtils
    {
        public static List<T> DrawWithoutReplacement<T>(List<T> source, int numberOfDraws)
        {
            if (numberOfDraws > source.Count)
                Debug.LogWarning($"Not enough elements for drawing {numberOfDraws} times");

            List<T> copy = new List<T>(source); // copy to preserve original list
            List<T> result = new List<T>();

            while (numberOfDraws>0)
            {
                if (numberOfDraws > copy.Count)
                {
                    break;
                }
                int index = Random.Range(0, copy.Count);
                result.Add(copy[index]);
                copy.RemoveAt(index);

                --numberOfDraws;
            }

            return result;
        }

        public static List<int> DrawIndexWithoutReplacement<T>(List<T> source, int numberOfDraws)
        {
            if (numberOfDraws > source.Count)
                Debug.LogWarning($"Not enough elements for drawing {numberOfDraws} times");

            List<T> copy = new List<T>(source); // copy to preserve original list
            List<int> result = new List<int>();

            while (numberOfDraws > 0)
            {
                if (numberOfDraws > copy.Count)
                {
                    break;
                }
                int index = Random.Range(0, copy.Count);
                result.Add(index);
                copy.RemoveAt(index);

                --numberOfDraws;
            }

            return result;
        }

        public static T DrawOnce<T>(List<T> source)
        {
            if (source.Count < 1)
            {
                Debug.LogWarning($"Not enough elements for drawing 1 time");
                return default(T);
            }

            int index = Random.Range(0, source.Count);
            return source[index];
        }

        public static int DrawIndexOnce<T>(List<T> source)
        {
            if (source.Count < 1)
            {
                Debug.LogWarning($"Not enough elements for drawing 1 time");
                return -1;
            }

            int index = Random.Range(0, source.Count);
            return index;
        }

        public static List<T> Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                int j = Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }
    }
}
