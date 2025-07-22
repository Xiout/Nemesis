using Board;
using System.Collections.Generic;

namespace Assets.Scripts.Board
{
    public class IntruderAttack
    {
        public string Name;
        public List<IntruderTypeEnum> PerformingIntruders;
        public int IntruderLifePoint;
        public bool DoesRetreat;
        public bool Contamination;
        public int LightWoundCount;
        public int SeriousWoundCount;
        public int? SeriousWoundDeathCondition;
        public bool DoesTargetCurrentPlayerOnly;

        public readonly static IntruderAttack[] AllIntruderAttacks =
        {
            new("Slime", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper, IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 5, false, contamination: true),
            new("Claw Attack", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, -1, true,   contamination: true, lightWound: 2),
            new("Claw Attack", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen},  3, false,  contamination: true, lightWound: 2),
            new("Claw Attack", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen},  4, false,  contamination: true, lightWound: 2),
            new("Claw Attack", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen},  5, false,  contamination: true, lightWound: 2),
            new("Bite", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, -1, true, seriousWound: 1, deathCondition: 2),
            new("Bite", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 4, false, seriousWound: 1, deathCondition: 2),
            new("Bite", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 6, false, seriousWound: 1, deathCondition: 2),
            new("Bite", new List<IntruderTypeEnum> { IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 4, false, seriousWound: 1, deathCondition: 2),
            new("Scratch", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper, IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 5, false, contamination: true, lightWound: 1),
            new("Scratch", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper, IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 2, false, contamination: true, lightWound: 1),
            new("Scratch", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper, IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 3, false, contamination: true, lightWound: 1),
            new("Scratch", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper, IntruderTypeEnum.Adult, IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 6, false, contamination: true, lightWound: 1),
            new("Frenzy", new List<IntruderTypeEnum> { IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 3, false, seriousWound: 1, deathCondition: 2, currentOnly: true),
            new("Frenzy", new List<IntruderTypeEnum> { IntruderTypeEnum.Breeder, IntruderTypeEnum.Queen}, 4, false, seriousWound: 1, deathCondition: 2, currentOnly: true),
            new("Tail Attack", new List<IntruderTypeEnum> {IntruderTypeEnum.Queen}, 5, false, seriousWound: 1, deathCondition: 1),
            new("Tail Attack", new List<IntruderTypeEnum> {IntruderTypeEnum.Queen}, 2, false, seriousWound: 1, deathCondition: 1),
            new("Summoning", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper, IntruderTypeEnum.Queen}, 3, false), //TODO implement
            new("Transformation", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper}, 4, false), //TODO implement
            new("Transformation", new List<IntruderTypeEnum> { IntruderTypeEnum.Creeper}, 5, false), //TODO implement
        };

        private IntruderAttack(string name, List<IntruderTypeEnum> intruders, int life, bool retreat, 
            bool contamination = false, int lightWound = 0, int seriousWound = 0, int? deathCondition = null, bool currentOnly = true)
        {
            Name = name;
            PerformingIntruders = intruders;
            IntruderLifePoint = life;
            DoesRetreat = retreat;
            Contamination = contamination;
            LightWoundCount = lightWound;
            SeriousWoundCount = seriousWound;
            SeriousWoundDeathCondition = deathCondition;
            DoesTargetCurrentPlayerOnly = currentOnly;
        }
    }
}
