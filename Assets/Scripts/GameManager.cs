using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public enum RoomName{EVACUATION_SECTION_A, EVACUATION_SECTION_B, NEST, GENERATOR, STORAGE, LABORATORY, SURGERY, FIRE_SYSTEM, EMERGENCY}
	
	public List<Room> ListOfRoomPositions;

	private RoomName[] _listOfRooms;
	
	#region GameManager
	private static GameManager _instance = null;
	public static GameManager Instance{
		get{
			if(_instance==null){
				_instance = new GameManager();
				//set all properties here
				_instance.initializeRoomsPositions();
			}
			return _instance;
		}
	}
	
	private void Awake(){
		_instance = this;
		
		if(_listOfRooms == null || _listOfRooms.Length == 0)
			initializeRoomsPositions();
	}
	#endregion
	
	private void initializeRoomsPositions(){
		List<RoomName> rooms = new List<RoomName>();
		
		for(int i=0;i<ListOfRoomPositions.Count; ++i){
			rooms.Add((RoomName)i);
		}
		
		rooms.Shuffle();
		_listOfRooms = rooms.ToArray();
	}
}
