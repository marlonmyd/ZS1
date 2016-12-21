var ScarePics:Texture2D[];
var ScareAudio:AudioClip[];
var Scaretime=60.0;
var ScarePicsInt=0;


function Start () {
Scaretime =50;
ScarePicsInt= Random.Range(0,6);
}
function ShowScarePic(){
GetComponent.<AudioSource>().Play();
Handheld.Vibrate ();
GetComponent.<GUITexture>().enabled=true;
yield WaitForSeconds(3.0);
GetComponent.<GUITexture>().enabled=false;
ScarePicsInt= Random.Range(0,6);
}
function Update () {
Scaretime -=Time.deltaTime;

if(Scaretime <=0){
	Scaretime = Random.Range(20,60);
	
ShowScarePic();
}

if(ScarePicsInt ==0){
	GetComponent.<GUITexture>().texture =ScarePics[0];
	GetComponent.<AudioSource>().clip=ScareAudio[0];
	

}
if(ScarePicsInt ==1){
	GetComponent.<GUITexture>().texture =ScarePics[1];
GetComponent.<AudioSource>().clip=ScareAudio[1];
	
}
if(ScarePicsInt ==2){
	GetComponent.<GUITexture>().texture =ScarePics[2];
GetComponent.<AudioSource>().clip=ScareAudio[2];
	
}
if(ScarePicsInt ==3){
	GetComponent.<GUITexture>().texture =ScarePics[3];
	GetComponent.<AudioSource>().clip=ScareAudio[3];
	

}
if(ScarePicsInt ==4){
	GetComponent.<GUITexture>().texture =ScarePics[4];
	GetComponent.<AudioSource>().clip=ScareAudio[4];
	

}
if(ScarePicsInt ==5){
	GetComponent.<GUITexture>().texture =ScarePics[5];
	GetComponent.<AudioSource>().clip=ScareAudio[5];
	

}
}