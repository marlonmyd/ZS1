var bulletSpeed=5;
var rb: Rigidbody;

var TimeToDestroy=10.0;

function Start() {
	rb = GetComponent.<Rigidbody>();
}


function Update () {
 //Rigidbody bulletClone = (Rigidbody) Instantiate(bullet, transform.position, transform.rotation);
 rb.velocity =  transform.TransformDirection (Vector3.forward * bulletSpeed);
 
 TimeToDestroy -=Time.deltaTime;

if(TimeToDestroy <=0){
	
	Destroy(gameObject);
}
}

function OnTriggerEnter (other : Collider) {
	if(other.tag == "Enemy"){
		
		//Destroy(other.gameObject);
		//Destroy(gameObject);
	}
	//Destroy(gameObject);
	
		
	}