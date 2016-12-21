#pragma strict

private var devices : WebCamDevice[];

private var deviceName : String;

private var wct : WebCamTexture;

private var resultString : String;

private var update: boolean;

private var data : Color32[];

 

function Start() {

    yield Application.RequestUserAuthorization (UserAuthorization.WebCam | UserAuthorization.Microphone);

    if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone)) {

       devices = WebCamTexture.devices;

       deviceName = devices[0].name;
		//wct = new WebCamTexture(deviceName, 1200, 1080, 15);
       wct = new WebCamTexture(deviceName, 640, 360, 15);

       GetComponent.<Renderer>().material.mainTexture = wct;

       wct.Play();

       resultString = "no problems";

    } else {

       resultString = "no permission!";

    }

    data = new Color32[wct.width * wct.height];

}

 

function Update() {

    if (wct) {

        if (wct.didUpdateThisFrame) {

            update = true;

            wct.GetPixels32 (data);

        } else {

            update = false;

        }

    }

}

 

function OnGUI() {

    for (var i = 0; i < devices.length; i++) {

       //GUI.Box(Rect(100, 100+(i*25), 200, 25),"NAME: "+devices[i].name);

       //GUI.Box(Rect(300, 100+(i*25), 200, 25),"FRONT FACING? "+devices[i].isFrontFacing);

    }

    //GUI.Box(Rect(100, 100+(i*25), 200, 25),"OPENED? "+resultString);

   // GUI.Box(Rect(300, 100+(i*25), 200, 25),"PLAYING? "+wct.isPlaying);

   // GUI.Box(Rect(100, 125+(i*25), 200, 25),"UPDATED? "+update);

}