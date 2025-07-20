using Assets.Scripts.Board;
using Board;
using System.Collections.Generic;
using System.Linq;

namespace Randomness
{
    internal class EventAndIntruderAttackManager
    {
        private static List<IntruderAttack> IntruderAttackDeck = null;
        private static List<IntruderAttack> IntruderAttackDiscard = null;

        private static List<EventCard> EventDeck = null;
        private static List<EventCard> EventDiscard = null;

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

            return card;
        }
    }
}
