var ScarePics:Sprite[];
var ScareAudio:AudioClip[];
var Scaretime=60.0;
var ScarePicsInt=0;
var ScarePicLeft:GameObject;
var ScarePicRight:GameObject;

function Start () {
Scaretime =50;
ScarePicsInt= Random.Range(0,6);
}
function ShowScarePic(){
GetComponent.<AudioSource>().Play();
Handheld.Vibrate ();
//GetComponent.<GUITexture>().enabled=true;
ScarePicLeft.GetComponent.<UI.Image>().enabled=true;
ScarePicRight.GetComponent.<UI.Image>().enabled=true;
yield WaitForSeconds(3.0);
ScarePicLeft.GetComponent.<UI.Image>().enabled=false;
ScarePicRight.GetComponent.<UI.Image>().enabled=false;
//GetComponent.<GUITexture>().enabled=false;
ScarePicsInt= Random.Range(0,8);
}
function Update () {
Scaretime -=Time.deltaTime;

if(Scaretime <=0){
	Scaretime = Random.Range(20,60);
	
ShowScarePic();
}

if(ScarePicsInt ==0){
	ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[0];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[0];
	//GetComponent.<GUITexture>().texture =ScarePics[0];
	GetComponent.<AudioSource>().clip=ScareAudio[0];
	

}
if(ScarePicsInt ==1){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[1];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[1];
	//GetComponent.<GUITexture>().texture =ScarePics[1];
GetComponent.<AudioSource>().clip=ScareAudio[1];
	
}
if(ScarePicsInt ==2){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[2];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[2];
	//GetComponent.<GUITexture>().texture =ScarePics[2];
GetComponent.<AudioSource>().clip=ScareAudio[2];
	
}
if(ScarePicsInt ==3){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[3];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[3];
	//GetComponent.<GUITexture>().texture =ScarePics[3];
	GetComponent.<AudioSource>().clip=ScareAudio[3];
	

}
if(ScarePicsInt ==4){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[4];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[4];
	//GetComponent.<GUITexture>().texture =ScarePics[4];
	GetComponent.<AudioSource>().clip=ScareAudio[4];
	

}
if(ScarePicsInt ==5){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[5];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[5];
	//GetComponent.<GUITexture>().texture =ScarePics[5];
	GetComponent.<AudioSource>().clip=ScareAudio[5];
}
if(ScarePicsInt ==6){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[6];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[6];
	//GetComponent.<GUITexture>().texture =ScarePics[5];
	GetComponent.<AudioSource>().clip=ScareAudio[0];
}
if(ScarePicsInt ==7){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[7];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[7];
	//GetComponent.<GUITexture>().texture =ScarePics[5];
	GetComponent.<AudioSource>().clip=ScareAudio[1];
}
if(ScarePicsInt ==8){
ScarePicLeft.GetComponent.<UI.Image>().sprite=ScarePics[8];
	ScarePicRight.GetComponent.<UI.Image>().sprite=ScarePics[8];
	//GetComponent.<GUITexture>().texture =ScarePics[5];
	GetComponent.<AudioSource>().clip=ScareAudio[2];
}
}