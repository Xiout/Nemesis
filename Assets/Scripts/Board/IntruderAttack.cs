using Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Board
{
    public class IntruderAttack
    {
        //public string Name;
        //public List<IntruderTypeEnum> PerformingIntruders;
        public int IntruderLifePoint;
        public bool DoesRetreat;

        public readonly static IntruderAttack[] AllIntruderAttacks =
        {
            new IntruderAttack {IntruderLifePoint = 5, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 5, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 4, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 4, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 4, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 4, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 4, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 3, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 2, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 2, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 2, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = 2, DoesRetreat = false},
            new IntruderAttack {IntruderLifePoint = -1, DoesRetreat = true},
            new IntruderAttack {IntruderLifePoint = -1, DoesRetreat = true},
            new IntruderAttack {IntruderLifePoint = -1, DoesRetreat = true},
        };
    }
}
