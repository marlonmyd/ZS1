using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdsExample : MonoBehaviour
{

public float ShowAdTime=30.0f;
public float ShowAdTimeFloat=30.0f;
public string LevelToLoad="wave1";


public void Start(){
ShowAd();
}
public void Awake(){
ShowAd();
}

public void Update(){
ShowAdTime -=Time.deltaTime;

if(ShowAdTime <=0.0f){
ShowAd();
ShowAdTime = ShowAdTimeFloat;
}
}
  public void ShowAd()
  {
    if (Advertisement.IsReady())
    {
      Advertisement.Show();
	  Application.LoadLevel(LevelToLoad);
    }
  }
}