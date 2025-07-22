using Assets.Scripts.Board;
using Board;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Randomness
{
    internal class EventAndIntruderAttackManager
    {
        private static List<IntruderAttack> IntruderAttackDeck = null;
        private static List<IntruderAttack> IntruderAttackDiscard = null;

        private static List<EventCard> EventDeck = null;
        private static List<EventCard> EventDiscard = null;

        private static List<bool> ContaminationCardDesk = null;

        private static List<SeriousWoundEnum> SeriousWoundDeck = new List<SeriousWoundEnum>(){
            SeriousWoundEnum.Bleeding, SeriousWoundEnum.Bleeding,SeriousWoundEnum.Bleeding, SeriousWoundEnum.Bleeding,
            SeriousWoundEnum.Body, SeriousWoundEnum.Body, SeriousWoundEnum.Body, SeriousWoundEnum.Body,
            SeriousWoundEnum.Arm, SeriousWoundEnum.Arm, SeriousWoundEnum.Arm, SeriousWoundEnum.Arm,
            SeriousWoundEnum.Leg, SeriousWoundEnum.Leg, SeriousWoundEnum.Leg, SeriousWoundEnum.Leg,
            SeriousWoundEnum.Hand, SeriousWoundEnum.Hand, SeriousWoundEnum.Hand, SeriousWoundEnum.Hand};

        public static bool SetUpEventDeck()
        {
            if (EventDeck != null)
            {
                return false;
            }

            EventDeck = new List<EventCard>();
            EventDeck.AddRange(EventCard.AllEventCards.ToList());
            EventDiscard = new List<EventCard>();
            return true;
        }

        public static EventCard DrawEventCard()
        {
            if (EventDeck == null)
            {
                SetUpEventDeck();
            }

            if (EventDeck.Count == 0)
            {
                EventDeck.AddRange(EventDiscard);
                EventDiscard.Clear();
            }

            var card = RandomUtils.DrawOnce(EventDeck);
            EventDiscard.Add(card);
            EventDeck.Remove(card);

            return card;
        }

        public static bool SetUpIntruderAttackDeck()
        {
            if (IntruderAttackDeck != null)
            {
                return false;
            }

            IntruderAttackDeck = new List<IntruderAttack>();
            IntruderAttackDeck.AddRange(IntruderAttack.AllIntruderAttacks.ToList());
            IntruderAttackDiscard = new List<IntruderAttack>();
            return true;
        }

        public static IntruderAttack DrawIntruderAttackCard()
        {
            if (IntruderAttackDeck == null)
            {
                SetUpIntruderAttackDeck();
            }

            if (IntruderAttackDeck.Count == 0)
            {
                IntruderAttackDeck.AddRange(IntruderAttackDiscard);
                IntruderAttackDiscard.Clear();
            }

            var card = RandomUtils.DrawOnce(IntruderAttackDeck);
            IntruderAttackDiscard.Add(card);
            IntruderAttackDeck.Remove(card);

            Debug.Log($"Draw Attack : {card.Name}");
            return card;
        }

        public static bool DrawContaminationCard()
        {
            if(ContaminationCardDesk == null)
            {
                ContaminationCardDesk = new List<bool>();
                for (int i=0; i<21; ++i) {
                    ContaminationCardDesk.Add(i < 7);
                }
            }

            if (ContaminationCardDesk.Count <= 0)
            {
                Debug.LogWarning("Not enough card in the contamination deck to draw on");
                return false;//Could be replaced by random bool
            }

            bool card = RandomUtils.DrawOnce(ContaminationCardDesk);
            ContaminationCardDesk.Remove(card);

            return card;
        }

        public static SeriousWoundEnum DrawSeriousWoundCard()
        {
            var card = RandomUtils.DrawOnce(SeriousWoundDeck);
            SeriousWoundDeck.Remove(card);

            return card;
        }
    }
}
