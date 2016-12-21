var TimeToSpawn= 5.0;
var Zombie:GameObject[];
var ZombieInt=0;
var Enemies:GameObject[];
var LevelName="";
var Portal:GameObject;
var Player:GameObject;
var DistanceToPlayer=0.0;
var WaveLevel:UI.Text;
var i =0;
function Start () {

}
function FindPlayer(){
Player= FindClosestEnemy(); 
 DistanceToPlayer=  Vector3.Distance(Player.transform.position, transform.position);;
}
function FindClosestEnemy () : GameObject {
    // Find all game objects with tag Enemy
    var gos : GameObject[];
    gos = GameObject.FindGameObjectsWithTag("Player"); 
    var closest : GameObject; 
    var distance = Mathf.Infinity; 
    var position = transform.position; 
    // Iterate through them and find the closest one
    for (var go : GameObject in gos)  { 
        var diff = (go.transform.position - position);
        var curDistance = diff.sqrMagnitude; 
        if (curDistance < distance) { 
            closest = go; 
            distance = curDistance; 
        } 
    } 
    return closest;    
}
function WaveManager(){
	LevelName=Application.loadedLevelName;
	
	
	TimeToSpawn -=Time.deltaTime;




	for(i = 0; i < 90; i++)
	{
		//Debug.Log(i);
		if(WaveLevel.text== "Wave "+ (i+1)){
		//WaveTxt.GetComponent(UI.Text).text = "Wave "+ (i + 1);
		//Debug.Log(Score);
		
		if(TimeToSpawn <=0){
	
	if(Enemies.Length <(11+i)){
	 if(DistanceToPlayer >8){
	 Instantiate(Zombie[ZombieInt],transform.position, transform.rotation);
	 //Instantiate(Portal,transform.position, transform.rotation);
	 }
	}
 TimeToSpawn = Random.Range(3,8);
 ZombieInt = Random.Range(0,5);
}


		//Debug.Log("asdas" + i.ToString("F0"));
		//Debug.Log(20 * (i + 1));
//if(Score > (20 * (i + 1))){
			//	Debug.Log("wave "+ (i + 1));
				//Application.LoadLevel("wave"+ (i + 2));
			//}
	}
	}
	

	
	
}
function Update () {
	WaveManager();
	FindPlayer();
	Enemies =  GameObject.FindGameObjectsWithTag("Enemy");
	
	

}