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

    public bool MoveIntruderWithCard()
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
                    return true;
                }
                else
                {
                    var regularCorridor = (corridor as RegularCorridor);
                    if(regularCorridor == null)
                    {
                        Debug.LogWarning($"Corridor {corridor.name} as an invalid type");
                    }

                    if(regularCorridor.Door == DoorEnum.Closed)
                    {
                        regularCorridor.BreakDoor();
                        return false;
                    }

                    CurrentRoom.RemoveIntruderFromRoom(this);
                    CurrentRoom = regularCorridor.Room2;
                    CurrentRoom.PlaceIntruderInRoom(this);
                    return true;
                }
            }
            else
            {
                Debug.LogWarning($"No Corridor found for moving Intruder from {CurrentRoom.name} (direction {eventCard.MovementDirection})");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("Drawn Intruder Attack Card was null");
            return false;
        }
    }

    internal void ResetMaterial()
    {
        gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { _defaultMaterial });
    }

    internal void Visualize()
    {
        gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().SelectableMaterial});
    }

    private void PerformIntruderDeath()
    {
        //Add intruder carcass
        CurrentRoom.Intruders.Remove(this);
        Ship.GetInstance().Intruders.Remove(this);

        GameObject.Destroy(gameObject);
    }
}
