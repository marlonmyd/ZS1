var scrollSpeed : float = 0.5;
var rend: Renderer;
var horscroll=true;
var verscroll=false;
var offset : float;
function Start() {
	rend = GetComponent.<Renderer>();
}


function Update () {
	
	if(horscroll){
	offset = Time.time * scrollSpeed;
	
	rend.material.SetTextureOffset("_MainTex", Vector2(offset,offset));
	}
		if(verscroll){
	offset  = Time.time * scrollSpeed;
	
	rend.material.SetTextureOffset("_MainTex", Vector2(0,offset));
	}
}