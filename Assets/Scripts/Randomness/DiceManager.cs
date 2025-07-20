using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Randomness
{
    public class DiceManager
    {
        private static NoiseResultEnum[] NoiseDice 
        { 
            get
            {
                return new NoiseResultEnum[] {
                    NoiseResultEnum.Number1,
                    NoiseResultEnum.Number1,
                    NoiseResultEnum.Number2,
                    NoiseResultEnum.Number2,
                    NoiseResultEnum.Number3,
                    NoiseResultEnum.Number3,
                    NoiseResultEnum.Number4,
                    NoiseResultEnum.Number4,
                    NoiseResultEnum.Danger,
                    NoiseResultEnum.Silent,
                };
            }
        }

        private static CombatRollEnum[] CombatRollDice
        {
            get
            {
                return new CombatRollEnum[] {
                    CombatRollEnum.Blank,
                    CombatRollEnum.Shot,
                    CombatRollEnum.DoubleShot,
                    CombatRollEnum.Creeper,
                    CombatRollEnum.Creeper,
                    CombatRollEnum.Adult,
                };
            }
        }

        public static NoiseResultEnum RollNoiseDice()
        {
            return NoiseDice[Random.Range(0, NoiseDice.Length)];
        }

        public static CombatRollEnum RollCombatDice()
        {
            return CombatRollDice[Random.Range(0, CombatRollDice.Length)];
        }
    }
}
