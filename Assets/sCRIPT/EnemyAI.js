var MaxHP=100.0;
var HP=100.0;
var Explosion:GameObject;
var HPUI:UI.Image;
var FireTime=3.0;
var FireTimeToFire=3.0;
var Bullet:GameObject;
var BulletSpawner:Transform;
var Ani:Animator;
var Manager:GameObject;
var Attacking=false;
var AttackCoolDown=5.0;
var Alive=true;
var Agent:NavMeshAgent;
var WaveLevel=1;

function Save(){
	
	PlayerPrefs.SetInt("WaveLevel", WaveLevel);
	
}
function Load(){
	
	WaveLevel=PlayerPrefs.GetInt("WaveLevel");
	
}
function Start () {
Load();
FireTime=Random.Range(1,5);
}

function Update () {
if(WaveLevel ==1){
Agent.speed =0.1;
}
if(WaveLevel ==2){
Agent.speed =0.2;
}
if(WaveLevel ==3){
Agent.speed =0.3;
}
if(WaveLevel ==4){
Agent.speed =0.4;
}
if(WaveLevel ==5){
Agent.speed =0.5;
}
if(WaveLevel ==6){
Agent.speed =0.6;
}
if(WaveLevel ==7){
Agent.speed =0.7;
}
if(WaveLevel ==8){
Agent.speed =0.8;
}
if(WaveLevel ==9){
Agent.speed =0.9;
}
if(WaveLevel ==10){
Agent.speed =1.0;
}
if(WaveLevel ==11){
Agent.speed =1.1;
}
if(WaveLevel ==12){
Agent.speed =1.2;
}
if(WaveLevel ==13){
Agent.speed =1.3;
}
if(WaveLevel ==14){
Agent.speed =1.4;
}
if(WaveLevel ==15){
Agent.speed =1.5;
}
if(WaveLevel ==16){
Agent.speed =1.6;
}
if(WaveLevel ==17){
Agent.speed =1.7;
}
if(WaveLevel ==18){
Agent.speed =1.8;
}
if(WaveLevel ==19){
Agent.speed =1.9;
}
if(WaveLevel ==20){
Agent.speed =2.0;
}


	Manager=GameObject.Find("Manager");
	
	
	AttackCoolDown -=Time.deltaTime;
	
	if(AttackCoolDown <=0){
		Attacking=true;
		AttackCoolDown = Random.Range(3,5);
		
		
	}
	if(Attacking){
		
		Attacking=false;
	}
	if(Alive){
	if(HP <=0){
	//Destroy(gameObject);
Die();
Alive=false;

	}

	
	
}
	FireTime -=Time.deltaTime;
	
if(FireTime <=0){
	
	Instantiate(Bullet, BulletSpawner.position,BulletSpawner.rotation);	
	FireTime=Random.Range(1,5);
	
}
	
	
	


HPUI.fillAmount= HP/MaxHP;
}
function Die(){
	gameObject.GetComponent(CapsuleCollider).height=0.1;
	Instantiate(Explosion, transform.position,transform.rotation);
	Manager.GetComponent(GameManager).Score +=1;
	Manager.GetComponent(GameManager).ZombieKilled +=1;
	gameObject.GetComponent(NavMeshAgent).enabled =false;
	gameObject.tag = "Untagged";
		Ani.SetBool("die", true);
		
		yield WaitForSeconds(5.0);
	Destroy(gameObject);
	

	
}
function OnTriggerEnter (other : Collider) {
	if(other.tag == "bullet"){
		Instantiate(Explosion, transform.position,transform.rotation);	
		HP-=50;
		Destroy(other.gameObject);
	}
	//Destroy(gameObject);
	
		
	}