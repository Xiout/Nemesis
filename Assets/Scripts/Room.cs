using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
	//Properties relative to the room as a piece of the board
	public bool IsExplored {get;set;}
	public bool IsOnFire {get;set;}
	public bool IsBroken {get;set;}
	public int NumberItems {get;set;}
	
	//Properties elative to the type of room
	public GameManager.RoomName roomName {get;private set;}
	public string roomColor {get;private set;}
	public bool hasComputer {get;private set;}
	
    // Start is called before the first frame update
    private void Start()
    {
        IsExplored = false;
		IsOnFire = false;
		IsBroken = false;
		NumberItems = 0;
    }
	
	private void Explore(){
	
	}

    // Update is called once per frame
    private void Update()
    {
        
    }
	

}
