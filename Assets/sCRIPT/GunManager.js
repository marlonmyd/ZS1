var Bullet:GameObject;
var BulletSound:AudioSource;
var BulletSpawner:Transform;
var Basyo:GameObject;
var BasyoSpawner:Transform;

 var speed = 20;
 
 
function Start () {

}

function Update () {
   // Put this in your update function
     if (Input.GetButtonDown("Fire1")||Input.GetMouseButtonDown(0)) {
	BulletSound.GetComponent.<AudioSource>().Play();
   Instantiate(Bullet, BulletSpawner.transform.position, BulletSpawner.transform.rotation);
     }


}
