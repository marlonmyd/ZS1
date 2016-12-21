using UnityEngine;
using System.Collections;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class GoogleAPIManager : MonoBehaviour {


  //leaderboard strings
  public GameObject Manager;
  public int HScore=0;
  public int score;
  public string leaderboard = "CgkI243Yh90ZEAIQDA";
  //achievement strings
  public bool UnlockWave3=false;
  public bool UnlockWave5=false;
  public bool UnlockWave7=false;
  public bool UnlockWave9=false;
  public bool UnlockWave11=false;
  public bool UnlockWave13=false;
  public bool UnlockWave15=false;
  public bool UnlockWave17=false;
  public bool UnlockWave19=false;
  public bool UnlockWave21=false;
  public string Wave3= "CgkI243Yh90ZEAIQAA";
  public string Wave5 = "CgkI243Yh90ZEAIQAQ";
  public string Wave7 = "CgkI243Yh90ZEAIQAg";
  public string Wave9 = "CgkI243Yh90ZEAIQAw";
  public string Wave11 = "CgkI243Yh90ZEAIQBA";
  public string Wave13= "CgkI243Yh90ZEAIQBQ";
  public string Wave15 = "CgkI243Yh90ZEAIQBg";
  public string Wave17 = "CgkI243Yh90ZEAIQBw";
  public string Wave19 = "CgkI243Yh90ZEAIQCA";
  public string Wave21 = "CgkI243Yh90ZEAIQCQ";
  public string incremental = "CgkI3tbsresCEAIQAw";
   public string messages="";
   public GameObject NormalMenu;
   public Text WaveTxt;
   
   
  
  
   public void Update(){
 //HScore=MxOptionMgr.GetInstance().GetBestScore();
 score=Manager.GetComponent<ScoreManager>().Score;
  if(score >=HScore){
  //PostScore();
  HScore=score;
  }
  if(HScore >=10){
	//UnlockEasyAchievement();
	//PostScore();
	}
  	
  
  
  } 
 public void LeaderBoardManager(){
    Social.localUser.Authenticate((bool success) =>
      {
        if (success)
        { 
		NormalMenu.active=false;
        }
        else
        {
		  
        }
      });
 
 }  
 void Load(){
	HScore=PlayerPrefs.GetInt("HScore");
	}
  void Start(){
  Load();
    PlayGamesPlatform.Activate();
	messages="google Play Services";
	

  }
  void Awake(){
  Load();
    PlayGamesPlatform.Activate();
	messages="google Play Services";
	

  }
  public void LogIn(){
   Social.localUser.Authenticate((bool success) =>
      {
        if (success)
        {  messages="You've successfully logged in";
          Debug.Log("You've successfully logged in");
		
        }
        else
        {messages="Login failed for some reason";
          Debug.Log("Login failed for some reason");
		  
        }
      });
  }
  public void UnlockWave3Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave3, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave3 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
    public void UnlockWave5Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave5, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave5 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
    public void UnlockWave7Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave7, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave7 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
    public void UnlockWave9Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave9, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave9 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
    public void UnlockWave11Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave11, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave11 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
      public void UnlockWave13Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave13, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave13 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
      public void UnlockWave15Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave15, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave15 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
      public void UnlockWave17Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave17, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave17 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
      public void UnlockWave19Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave19, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave19 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
      public void UnlockWave21Poser(){
  if (Social.localUser.authenticated)
      {
        Social.ReportProgress(Wave21, 100.0f, (bool success) =>
        {
          if (success)
          {UnlockWave21 =true;
            Debug.Log("You've successfully logged in");
          }
          else
          {
            Debug.Log("Login failed for some reason");
          }
        });
      }
  
  }
  public void IncrementalAchievement(){
    if (Social.localUser.authenticated)
      {
        ((PlayGamesPlatform)Social.Active).IncrementAchievement(incremental, 5, (bool success) =>
        {
         //The achievement unlocked successfully;
        });
      }
  
  }
  public void PostScore(){


     if (Social.localUser.authenticated)
      {
        Social.ReportScore(HScore, leaderboard, (bool success) =>
        {
          if (success)
          {
            ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(leaderboard);
          }
          else
          {
           Debug.Log("Login failed for some reason");
          }
        });
      }
  }
  
  public void ShowLeaderboard(){
  //PostScore();
     Social.localUser.Authenticate((bool success) =>
      {
        if (success)
        {  
		PostScore();
		//Social.ShowLeaderboardUI();
          Debug.Log("You've successfully logged in");
		
        }
        else
        {
          Debug.Log("Login failed for some reason");
		  
        }
      });
   
  }
  
  public void ShowSpecificLeaderboard(){
  ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(leaderboard);
  }

  public void ShowAchievments(){

		if(WaveTxt.text =="Wave 3"&&UnlockWave3 ==false){
	UnlockWave3Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 5"&&UnlockWave5 ==false){
		UnlockWave3Poser();
	UnlockWave5Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 7"&&UnlockWave7 ==false){
		UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	PostScore();
	}
	if(WaveTxt.text =="Wave 9"&&UnlockWave9 ==false){
	UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	UnlockWave9Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 11"&&UnlockWave11 ==false){
	UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	UnlockWave9Poser();
	UnlockWave11Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 13"&&UnlockWave13 ==false){
	UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	UnlockWave9Poser();
	UnlockWave13Poser();
	UnlockWave11Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 15"&&UnlockWave15 ==false){
	UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	UnlockWave9Poser();
	UnlockWave13Poser();
	UnlockWave11Poser();
	UnlockWave15Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 17"&&UnlockWave17 ==false){
	UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	UnlockWave9Poser();
	UnlockWave11Poser();
	UnlockWave13Poser();
	UnlockWave15Poser();
	UnlockWave17Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 19"&&UnlockWave19 ==false){
	UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	UnlockWave9Poser();
	UnlockWave11Poser();
	UnlockWave13Poser();
	UnlockWave15Poser();
	UnlockWave17Poser();
	UnlockWave19Poser();
	PostScore();
	}
		if(WaveTxt.text =="Wave 21"&&UnlockWave21 ==false){
	UnlockWave3Poser();
	UnlockWave5Poser();
	UnlockWave7Poser();
	UnlockWave9Poser();
	UnlockWave11Poser();
	UnlockWave13Poser();
	UnlockWave15Poser();
	UnlockWave17Poser();
	UnlockWave19Poser();
	UnlockWave21Poser();
	PostScore();
	}
  
  
    Social.localUser.Authenticate((bool success) =>
      {
        if (success)
        {  
		Social.ShowAchievementsUI();
          Debug.Log("You've successfully logged in");
		
        }
        else
        {
          Debug.Log("Login failed for some reason");
		  
        }
      });
   
  }
public void SignOut(){
 ((PlayGamesPlatform)Social.Active).SignOut();
}

}
