﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using AUP;

public class ImagePickerDemo : MonoBehaviour {

	private SharePlugin sharePlugin;
	private ImagePickerPlugin imagePickerPlugin;

	public Text statusText;
	private string imagePath="";

	public RawImage rawImage;
	public Button shareButton;

	// Use this for initialization
	void Start (){
		sharePlugin = SharePlugin.GetInstance();
		sharePlugin.SetDebug(0);

		imagePickerPlugin = ImagePickerPlugin.GetInstance();
		imagePickerPlugin.SetDebug(0);
		imagePickerPlugin.Init();
		imagePickerPlugin.SetImagePickerCallbackListener(onGetImageComplete,onGetImageCancel,onGetImageFail);

		EnableDisableShareButton(false);
	}
	
	public void GetImage(){
		imagePickerPlugin.GetImage();
		EnableDisableShareButton(false);
	}	

	public void ShareImage(){
		if(!imagePath.Equals("",StringComparison.Ordinal)){
			sharePlugin.ShareImage("MyPictureSubject","MyPictureSubjectContent",imagePath);
			UpdateStatus("Sharing Picture");
		}else{
			Debug.Log("[CameraDemo] imagepath is empty");
			UpdateStatus("can't image path is empty");
		}
	}

	private void UpdateStatus(string status){
		if(statusText!=null){
			statusText.text = String.Format("Status: {0}",status);
		}
	}

	private void DelayLoadImage(){
		//loads texture
		rawImage.texture = AUP.Utils.LoadTexture(imagePath);
		
		UpdateStatus("load image complete");
		EnableDisableShareButton(true);
	}

	private void EnableDisableShareButton(bool val){
		shareButton.interactable = val;
	}
	
	private void LoadImageMessage(){
		UpdateStatus("Loading Image...");
	}

	private void onGetImageComplete(string imagePath){
		Dispatcher.GetInstance().InvokeAction(
			()=>{
				this.imagePath = imagePath;

				UpdateStatus("Capture ImageComp lete");
				
				Invoke("LoadImageMessage",0.3f);
				Invoke("DelayLoadImage",0.5f);

				Debug.Log("[CameraDemo] onGetImageComplete imagePath " + imagePath);
			}
		);
	}

	private void onGetImageCancel(){
		Dispatcher.GetInstance().InvokeAction(
			()=>{
				UpdateStatus("onGetImageCancel");
			}
		);
	}

	private void onGetImageFail(){
		Dispatcher.GetInstance().InvokeAction(
			()=>{
				UpdateStatus("onGetImageFail");
			}
		);
	}
}