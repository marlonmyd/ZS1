var TimeToDestroy=1.0;

function Start () {

}

function Update () {
TimeToDestroy -=Time.deltaTime;

if(TimeToDestroy <=0){
	
	Destroy(gameObject);
}
}