#pragma strict
var VR=true;
var AR=false;

function Awake () {
		DontDestroyOnLoad (transform.gameObject);
	}
function Start () {

}

function Update () {
if(Input.GetKeyDown(KeyCode.Escape)){
	
	if(VR){
		AR=true;
		VR=false;
		Application.LoadLevel("cam");
	}
	else{
		VR=true;
		AR=false;
		Application.LoadLevel("wave1");
	}
	
	
}
}