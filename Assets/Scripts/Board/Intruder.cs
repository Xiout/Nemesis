using Board;
using Board.Corridors;
using Board.Rooms;
using Randomness;
using System.Collections.Generic;
using UnityEngine;

public class Intruder : MonoBehaviour
{
    public IntruderTypeEnum IntruderType;
    private int _injuryCounter;
    internal Room CurrentRoom;
    internal int SurpriseAttackCount;

    internal int? PosInRoomIndex;

    private Material _defaultMaterial;

    private void Awake()
    {
        _defaultMaterial = GetComponent<MeshRenderer>().material;
    }

    void Start()
    {
        _injuryCounter = 0;
    }

    internal bool IsInCombat()
    {
        return CurrentRoom.Players.Count > 0;
    }

    public void DealDamage(int damage)
    {
        _injuryCounter += damage;
    }

    public bool ResolveDeathOrRetreat()
    {
        Debug.Log("Injury :"+_injuryCounter);

        if (IntruderType == IntruderTypeEnum.Larva)
        {
            if(_injuryCounter >= 1)
            {
                PerformIntruderDeath();
                return true;
            }
        }

        var intruderCard = EventAndIntruderAttackManager.DrawIntruderAttackCard();
        if (intruderCard.DoesRetreat)
        {
            Debug.Log("Retreat");
            MoveIntruderWithCard();
            return false;
        }

        if(IntruderType != IntruderTypeEnum.Queen && IntruderType != IntruderTypeEnum.Breeder) 
        {
            Debug.Log("Life Points " + intruderCard.IntruderLifePoint);

            if (_injuryCounter >= intruderCard.IntruderLifePoint)
            {
                PerformIntruderDeath();
                return true;
            }

            return false;
        }

        var intruderCard2 = EventAndIntruderAttackManager.DrawIntruderAttackCard();
        if (intruderCard2.DoesRetreat)
        {
            Debug.Log("Retreat");
            MoveIntruderWithCard();
            return false;
        }

        Debug.Log("Life Points " + (intruderCard.IntruderLifePoint + intruderCard2.IntruderLifePoint));

        if (_injuryCounter >= intruderCard.IntruderLifePoint + intruderCard2.IntruderLifePoint)
        {
            PerformIntruderDeath();
            return true;
        }

        return false;
    }

    public void MoveIntruderWithCard()
    {
        var eventCard = EventAndIntruderAttackManager.DrawEventCard();
        if (eventCard != null)
        {
            Debug.Log($"Move Intruder {name} from {CurrentRoom} to direction {eventCard.MovementDirection}");

            if(CurrentRoom.Corridors.TryGetValue(eventCard.MovementDirection, out var corridor))
            {
                if((corridor as TechnicalCorridor) != null)
                {
                    Debug.Log("Move To Technical Corridor");
                    EncounterManager.AddToIntruderBag(IntruderType, SurpriseAttackCount);
                    CurrentRoom.RemoveIntruderFromRoom(this);
                    Ship.GetInstance().Intruders.Remove(this);
                    GameObject.Destroy(gameObject);
                }
                else
                {
                    var regularCorridor = (corridor as RegularCorridor);
                    if(regularCorridor == null)
                    {
                        Debug.LogWarning($"Corridor {corridor.name} as an invalid type");
                    }

                    CurrentRoom.RemoveIntruderFromRoom(this);
                    CurrentRoom = regularCorridor.Room2;
                    CurrentRoom.PlaceIntruderInRoom(this);
                }
            }
            else
            {
                Debug.LogWarning($"No Corridor found for moving Intruder from {CurrentRoom.name} (direction {eventCard.MovementDirection})");
            }
        }
        else
        {
            Debug.LogWarning("Drawn Intruder Attack Card was null");
        }
    }

    public void PerformIntruderAttack(Player player)
    {
        if(IntruderType == IntruderTypeEnum.Larva)
        {
            player.IsInfected = true;
            player.TakeContaminationCard();
            CurrentRoom.RemoveIntruderFromRoom(this);

            Ship.GetInstance().Intruders.Remove(this);

            GameObject.Destroy(gameObject);
            return;
        }
        else
        {
            var card = EventAndIntruderAttackManager.DrawIntruderAttackCard();

            if (!card.PerformingIntruders.Contains(IntruderType))
            {
                Debug.Log("Missed");
                return;
            }

            if(card.SeriousWoundDeathCondition != null)
            {
                if(player.SeriousWoundCount() >= (int)card.SeriousWoundCount)
                {
                    Debug.Log($"Player {player.PlayerOrder} has {card.SeriousWoundCount} or more serious wound, He does not survive.");
                    player.Death();
                }
            }

            if (card.Contamination)
            {
                player.TakeContaminationCard();
            }

            player.TakeLightDamage(card.LightWoundCount);
            for(int i=0; i<card.SeriousWoundCount; ++i)
            {
                player.TakeSeriousDamage();
            }
        }
    }

    internal void ResetMaterial()
    {
        gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { _defaultMaterial });
    }

    internal void Visualize()
    {
        gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().AdjacentMaterial});
    }

    private void PerformIntruderDeath()
    {
        //Add intruder carcass
        CurrentRoom.RemoveIntruderFromRoom(this);
        Ship.GetInstance().Intruders.Remove(this);

        GameObject.Destroy(gameObject);
    }
}
