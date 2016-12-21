var HP=100.0;
var HPUI:UI.Image;
var MaxHP=100.0;
var X=0.0;
var Y=0.0;
var Speed=5.0;
var MousePos:Vector2;
var XTxt:UI.Text;
var YTxt:UI.Text;


function Start () {

}

function Update () {
HPUI.fillAmount= HP/MaxHP;
MousePos=Input.mousePosition;


if(HP <0){
	
	Application.LoadLevel("GameOver");
}

X= Input.GetAxis("Mouse X") ;
Y = Input.GetAxis("Mouse Y") ;

if(Y > 0){
	
	//transform.position.z +=Speed*Time.deltaTime;
}
if(Y < 0){
	
	//transform.position.z -=Speed*Time.deltaTime;
}

if(X > 0){
	
	//transform.position.x +=Speed*Time.deltaTime;
}
if(X < 0){
	
	//transform.position.x -=Speed*Time.deltaTime;
}
}

function OnTriggerEnter (other : Collider) {
	if(other.tag == "Enemybullet"){
		
		HP-=50;
	}
	if(other.tag == "Enemy"){
		
		HP-=50;
		Handheld.Vibrate();
	}
	//Destroy(gameObject);
	
		
	}
	

function OnTriggerStay (other : Collider) {
	
	if(other.tag == "Enemy" && other.gameObject.GetComponent(EnemyAI).Attacking == true){
		
		//HP-=50;
	}
	//Destroy(gameObject);
	
		
	}