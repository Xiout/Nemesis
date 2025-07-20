namespace Board
{
    internal class EventCard
    {
        //public String Name
        //public List<IntruderType> MovingIntruders
        public int MovementDirection;

        public readonly static EventCard[] AllEventCards =
       {
            new EventCard {MovementDirection = 1},
            new EventCard {MovementDirection = 1},
            new EventCard {MovementDirection = 1},
            new EventCard {MovementDirection = 1},
            new EventCard {MovementDirection = 1},
            new EventCard {MovementDirection = 2},
            new EventCard {MovementDirection = 2},
            new EventCard {MovementDirection = 2},
            new EventCard {MovementDirection = 2},
            new EventCard {MovementDirection = 2},
            new EventCard {MovementDirection = 3},
            new EventCard {MovementDirection = 3},
            new EventCard {MovementDirection = 3},
            new EventCard {MovementDirection = 3},
            new EventCard {MovementDirection = 3},
            new EventCard {MovementDirection = 4},
            new EventCard {MovementDirection = 4},
            new EventCard {MovementDirection = 4},
            new EventCard {MovementDirection = 4},
            new EventCard {MovementDirection = 4},
        };
    }
}
