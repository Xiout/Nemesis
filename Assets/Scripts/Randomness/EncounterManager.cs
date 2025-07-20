using Board;
using Randomness;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Randomness
{
    public class EncounterManager
    {
        private static Tuple<IntruderTypeEnum, int>[] AllIntruderTokens ={
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Larva  , 1),
            Tuple.Create(IntruderTypeEnum.Creeper, 1),
            Tuple.Create(IntruderTypeEnum.Creeper, 1),
            Tuple.Create(IntruderTypeEnum.Creeper, 1),
            Tuple.Create(IntruderTypeEnum.Adult  , 2),
            Tuple.Create(IntruderTypeEnum.Adult  , 2),
            Tuple.Create(IntruderTypeEnum.Adult  , 2),
            Tuple.Create(IntruderTypeEnum.Adult  , 2),
            Tuple.Create(IntruderTypeEnum.Adult  , 3),
            Tuple.Create(IntruderTypeEnum.Adult  , 3),
            Tuple.Create(IntruderTypeEnum.Adult  , 3),
            Tuple.Create(IntruderTypeEnum.Adult  , 3),
            Tuple.Create(IntruderTypeEnum.Adult  , 3),
            Tuple.Create(IntruderTypeEnum.Adult  , 4),
            Tuple.Create(IntruderTypeEnum.Adult  , 4),
            Tuple.Create(IntruderTypeEnum.Adult  , 4),
            Tuple.Create(IntruderTypeEnum.Breeder, 3),
            Tuple.Create(IntruderTypeEnum.Breeder, 4),
            Tuple.Create(IntruderTypeEnum.Queen  , 4),
            Tuple.Create(IntruderTypeEnum.Blank  , 0),
        };

        private static List<Tuple<IntruderTypeEnum, int>> IntruderBag;
        private static List<Tuple<IntruderTypeEnum, int>> RemainingTokens;

        public static bool SetUpIntruderBag()
        {
            if (IntruderBag != null)
            {
                return false;
            }
            RemainingTokens = new List<Tuple<IntruderTypeEnum, int>>();
            RemainingTokens.AddRange(AllIntruderTokens);

            IntruderBag = new List<Tuple<IntruderTypeEnum, int>>();
            var tokens = RandomUtils.DrawWithoutReplacement(RemainingTokens.Where(x => x.Item1 == IntruderTypeEnum.Larva).ToList(), 4);
            for(int i=0; i< tokens.Count; ++i)
            {
                IntruderBag.Add(tokens[i]);
                RemainingTokens.Remove(tokens[i]);
            }

            var token = RandomUtils.DrawOnce(RemainingTokens.Where(x => x.Item1 == IntruderTypeEnum.Creeper).ToList());
            IntruderBag.Add(token);
            RemainingTokens.Remove(token);

            tokens = RandomUtils.DrawWithoutReplacement(RemainingTokens.Where(x => x.Item1 == IntruderTypeEnum.Adult).ToList(), Ship.GetInstance().Players.Count + 3);
            for (int i = 0; i < tokens.Count; ++i)
            {
                IntruderBag.Add(tokens[i]);
                RemainingTokens.Remove(tokens[i]);
            }

            int index = RemainingTokens.FindIndex(x => x.Item1 == IntruderTypeEnum.Queen);
            IntruderBag.Add(RemainingTokens[index]);
            RemainingTokens.RemoveAt(index);

            index = RemainingTokens.FindIndex(x => x.Item1 == IntruderTypeEnum.Blank);
            IntruderBag.Add(RemainingTokens[index]);
            RemainingTokens.RemoveAt(index);

            return true;
        }

        public static Tuple<IntruderTypeEnum, int> DrawEncounter()
        {
            if(IntruderBag == null)
            {
                SetUpIntruderBag();
            }

            int index = RandomUtils.DrawIndexOnce(IntruderBag);
            if(index == -1)
            {
                return null;
            }
            var Token = IntruderBag[index];
            RemainingTokens.Add(IntruderBag[index]);
            IntruderBag.RemoveAt(index);
            return Token;
        }

        public static IntruderTypeEnum PerformIntruderBagResolution()
        {
            if (IntruderBag == null)
            {
                SetUpIntruderBag();
            }

            return RandomUtils.DrawOnce(IntruderBag).Item1;
        }

        public static bool AddToIntruderBag(IntruderTypeEnum intruderType, int? surpriseAttackCount=null)
        {
            if (IntruderBag == null)
            {
                SetUpIntruderBag();
            }

            Tuple<IntruderTypeEnum, int> token = null;
            if(surpriseAttackCount != null)
            {
                token = RemainingTokens.Find(x => x.Item1 == intruderType && x.Item2 == surpriseAttackCount);
                if(token != null)
                {
                    IntruderBag.Add(token);
                    RemainingTokens.Remove(token);
                    return true;
                }

                Debug.LogWarning($"Could not find an intruder token {intruderType} ({surpriseAttackCount}) to place back in the bag");
            }

            token = RandomUtils.DrawOnce(RemainingTokens.Where(x => x.Item1 == intruderType).ToList());
            if (token == null)
            {
                Debug.LogWarning($"Could not find any intruder token {intruderType} to place back in the bag");
                return false;
            }

            IntruderBag.Add(token);
            RemainingTokens.Remove(token);
            return true;
        }
    
        public static bool IsBagEmpty()
        {
            if(IntruderBag == null){
                SetUpIntruderBag();
            }

            return IntruderBag.Count == 0;
        }
    }
}
