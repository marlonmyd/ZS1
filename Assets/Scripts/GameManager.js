public var Score:int=0;
var ScoreTxt:TextMesh;
var FPS=0;
var FPSTxt:UI.Text;
var WaveTxt:GameObject;
var PlayingObject:GameObject;
var ZombieSpawner:GameObject;
var LevelName="";
var DLight:GameObject;
var LevelToLoad="";
var ZombieToKillInt=20.0;
var ZombieKilled=0.0;
var WaveLevel=1;
var WaveLevelTxt:UI.Text;
var WaveLevelUI:UI.Image;
var Menu=false;
var MenuUI:GameObject;
var ObjectWithSound:AudioSource[];
var ScarePic:GameObject;
var ScarePicYes=true;
var Player:Transform;
var  ScoreManager:ScoreManager;
var Gun:GameObject;
var CheatActivatedInt= 20;
var CheatActivated=false;


public function ResetZRotaion(){
Player.localRotation.z =0.0;
}
public function SoundManager(){
ObjectWithSound= FindObjectsOfType(typeof(AudioSource));


}
public function Like(){
Application.OpenURL ("https://www.facebook.com/Zombie-Survival-204125020024169/?ref=br_rs");
}
public function Rate(){
Application.OpenURL ("https://play.google.com/store/apps/details?id=com.ZombieSurvival.GAWANIMYD");
}
public function PlayManager(){
Time.timeScale=1.0;
}
function Save(){
	PlayerPrefs.SetString("LevelToLoad", "LevelToLoad");
	PlayerPrefs.SetInt("ZombieToKillInt", ZombieToKillInt);
	PlayerPrefs.SetInt("WaveLevel", WaveLevel);
	PlayerPrefs.SetInt("Score", Score);
}
function Load(){
	LevelToLoad=PlayerPrefs.GetString("LevelToLoad");
	ZombieToKillInt=PlayerPrefs.GetInt("ZombieToKillInt");
	WaveLevel=PlayerPrefs.GetInt("WaveLevel");
	Score=PlayerPrefs.GetInt("Score");
}
function Start () {
Load();	
LevelToLoad=Application.loadedLevelName;
Screen.sleepTimeout = SleepTimeout.NeverSleep;
DLight.active=false;
ZombieSpawner.active=false;
PlayingObject.active=false;
yield WaitForSeconds(3.0);
WaveTxt.active =false;
ZombieSpawner.active=true;
PlayingObject.active=true;
Save();
}
public function Reset(){
 ZombieToKillInt=20;
	 WaveLevel=0;
	 Score=0;
}
function WaveManager(){
	  if (Input.GetButtonDown("Fire2")) {
	 // Reset();
 //Score +=10;
	  }

	LevelName=Application.loadedLevelName;
	
	for(var i = 0; i < 90; i++)
	{
		//Debug.Log(i);
		if(Application.loadedLevelName== "wave"+ (i+1)){
		//WaveTxt.GetComponent(UI.Text).text = "Wave "+ (i + 1);
		WaveTxt.GetComponent(TextMesh).text = "Wave "+ (i + 1);
		//Debug.Log(Score);
		Debug.Log("asdas" + i.ToString("F0"));
		Debug.Log(20 * (i + 1));
			if(Score > (20 * (i + 1))){
				Debug.Log("wave "+ (i + 1));
				Application.LoadLevel("wave"+ (i + 2));
			}
	}
	}
	

	
	
}
function WaveManager2(){

WaveLevelTxt.text= "Wave "+ WaveLevel.ToString("F0");
WaveLevelUI.fillAmount= ZombieKilled/ZombieToKillInt;

 if (Input.GetButtonDown("Fire2")) {
//Reset();
 //Score +=10;
 //ZombieKilled +=10;
 CheatActivatedInt -=1;
 if(CheatActivatedInt <=0){
 CheatActivated=true;
 
 }
 if(CheatActivated){
  //Score +=10;
 //ZombieKilled +=10;
 }
	  }
	  

	LevelName=Application.loadedLevelName;
	
	if(WaveLevel <=0){
	ZombieToKillInt=20.0;
	ZombieKilled=0.0;
	WaveLevel =1;
	Save();
	}
	WaveTxt.GetComponent(TextMesh).text = "Wave "+ WaveLevel.ToString("F0");
	
	
	
			if(ZombieKilled >=ZombieToKillInt){
			ZombieToKillInt +=  5;
			WaveLevel +=1;
				Save();
				Application.LoadLevel("wave1");
			}
	
	}

function Update () {
//WaveManager();
SoundManager();
WaveManager2();
FPS = 1/Time.deltaTime;	

FPSTxt.text = FPS.ToString("F0");

ScoreTxt.text = Score.ToString("F0");
if(Input.GetKeyDown(KeyCode.Escape)){
	if(Menu){
	Menu=false;
	MenuUI.active =false;
	Time.timeScale=1.0;
	Gun.active =true;
	}
	else{
	Menu=true;
	Time.timeScale=0.0;
	MenuUI.active =true;
	Gun.active =false;
	}
}
if(Menu){
//Time.timeScale=0.0;
//MenuUI.active =true;
}

ScoreManager.Score = Score;
}