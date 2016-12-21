var LevelToLoad="";
var WaitTime=2.0;

function Start () {
		// Load the level named "MyBigLevel".
		var async : AsyncOperation = Application.LoadLevelAsync (LevelToLoad);
		yield async;
		//Debug.Log ("Loading complete");
	
}

function Update () {

}