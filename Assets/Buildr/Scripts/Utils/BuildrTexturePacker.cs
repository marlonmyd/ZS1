// BuildR
// Available on the Unity3D Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This function creates textures for the packed model
//It will tile textures for as long as needed to ensure we use one large texture for the model.
public class BuildrTexturePacker : MonoBehaviour 
{
	
	private static int MAX_SINGLE_TEXTURE_SIZE = 4096;
	private static int PADDING = 0;
	private static int PACKED_SIZE = 4096;
	
	public static void Pack(BuildrData data)
	{
		data.texturesNotPacked.Clear();
		data.texturesPacked.Clear();
		
		int numberOfTextures = data.textures.Count;
		List<Texture2D> useTextures = new List<Texture2D>(numberOfTextures);
		List<int> texturePacked = new List<int>();
		for(int t=0; t<numberOfTextures; t++)
		{
			BuildrTexture texture = data.textures[t];
			
			//Debug.Log(texture.name+" "+texture.tiled);
			if(texture.tiled)
			{
				int textureWidth = texture.texture.width;
				int textureHeight = texture.texture.height;
				int targetTextureWidth = Mathf.RoundToInt(textureWidth * texture.maxUVTile.x);
				int targetTextureHeight = Mathf.RoundToInt(textureHeight * texture.maxUVTile.y);
				float resizeToLargest = Mathf.Min((float)MAX_SINGLE_TEXTURE_SIZE/(float)targetTextureWidth, (float)MAX_SINGLE_TEXTURE_SIZE/(float)targetTextureHeight, 1);//ensure texture fits smallest size
				int useTextureWidth = Mathf.RoundToInt( resizeToLargest * targetTextureWidth );
				int useTextureHeight = Mathf.RoundToInt( resizeToLargest * targetTextureHeight );
				
				if(useTextureWidth==0||useTextureHeight==0)//texture no used on model
				{
					data.texturesNotPacked.Add(t);
					continue;
				}
				Texture2D useTexture = new Texture2D(useTextureWidth,useTextureHeight);
				
				Color32[] textureBlock = texture.texture.GetPixels32();
				Color32[] useBlock = new Color32[useTextureWidth*useTextureHeight];
				//paint the textures onto a new texture canvas
				for(int x=0; x<useTextureWidth; x++)
				{
					for(int y=0; y<useTextureHeight; y++)
					{
						int tx = Mathf.RoundToInt( (x/resizeToLargest)%(textureWidth-1));
						int ty = Mathf.RoundToInt( (y/resizeToLargest)%(textureHeight-1));
						
						int useIndex = x + y * useTextureWidth;
						int tIndex = tx + ty * textureWidth;
						useBlock[useIndex] = textureBlock[tIndex];
					}
				}
				useTexture.SetPixels32(useBlock);
				useTexture.Apply();
				useTexture.Compress(true);
				useTextures.Add(useTexture);
				texturePacked.Add(t);
				//Debug.Log(texture.name+" "+useTextureWidth+" "+useTextureHeight);
			}
			else
			{
				int useTextureWidth = texture.texture.width;
				int useTextureHeight = texture.texture.height;
				Texture2D useTexture = new Texture2D(useTextureWidth,useTextureHeight);
				useTexture.SetPixels32(texture.texture.GetPixels32());
				useTexture.Apply();
				useTexture.Compress(true);
				useTextures.Add(useTexture);
				texturePacked.Add(t);
				//Debug.Log(texture.name+" "+useTextureWidth+" "+useTextureHeight);
			}
		}
		
		Texture2D textureAtlas = new Texture2D(1,1);
		Rect[] outputRect = textureAtlas.PackTextures(useTextures.ToArray(),PADDING,PACKED_SIZE,false);
		
		//remove textures from memory
		if(data.textureAtlas!=null)
			Object.DestroyImmediate(data.textureAtlas);
		
		while(useTextures.Count > 0)
		{
			Object.DestroyImmediate(useTextures[0]);
			useTextures.RemoveAt(0);
		}
		
		data.textureAtlas = textureAtlas;
		data.texturesPacked = texturePacked;
		data.atlasCoords = outputRect;
		
		System.GC.Collect();
	}
}
