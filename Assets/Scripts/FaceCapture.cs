using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FaceCapture : MonoBehaviour 
{
    private int playerIndex = 0;
	private float faceWidth = 0.4f;
    private float faceHeight = 0.4f;

    public RawImage rawImage;

	private Rect faceRect;
	private Texture2D colorTex, faceTex;
	private KinectManager kinectManager;
	private FacetrackingManager faceManager;

    private void OnEnable()
    {
        if (faceManager != null)
            faceManager.enabled = true;
    }

    private void OnDisable()
    {
        if (faceManager != null)
            faceManager.enabled = false;
    }

    public bool IsFaceRectValid()
	{
		return faceRect.width > 0 && faceRect.height > 0;
	}

	public Rect GetFaceRect()
	{
		return faceRect;
	}

	public Texture GetFaceTex()
	{
		return faceTex;
	}

	void Start () 
	{
		kinectManager = KinectManager.Instance;
		faceTex = new Texture2D(100, 100, TextureFormat.ARGB32, false);

        ////////BILL
    }

    void Update () 
	{
		if(faceManager == null)
            faceManager = FacetrackingManager.Instance;

        if (faceManager.enabled == false)
            return;

		if(!kinectManager || !kinectManager.IsInitialized())
			return;
		if(!faceManager || !faceManager.IsFaceTrackingInitialized())
			return;

        //long userId = kinectManager.GetUserIdByIndex(playerIndex);
        long userId = kinectManager.GetPrimaryUserID();
		
        if (userId == 0) 
		{
            ////////BILL

			return;
		}

		// use color camera image
		if (!colorTex)
            colorTex = kinectManager.GetUsersClrTex2D();

		faceRect = GetHeadJointFaceRect(userId);

		if (faceRect.width > 0 && faceRect.height > 0) 
		{
			int faceX = (int)faceRect.x;
			int faceY = (int)faceRect.y;
			int faceW = (int)faceRect.width;
			int faceH = (int)faceRect.height;

			if(faceX < 0) faceX = 0;
			if(faceY < 0) faceY = 0;

            if (colorTex) 
			{
				if((faceX + faceW) > colorTex.width) faceW = colorTex.width - faceX;
				if((faceY + faceH) > colorTex.height) faceH = colorTex.height - faceY;
			}

			if(faceTex.width != faceW || faceTex.height != faceH)
			{
				faceTex.Resize(faceW, faceH);
			}

			Color[] colorPixels = colorTex.GetPixels(faceX, faceY, faceW, faceH, 0);
			faceTex.SetPixels(colorPixels);
			faceTex.Apply();

            rawImage.texture = faceTex;
		} 
		else 
		{
            rawImage.texture = null;
        }
	}

	private Rect GetHeadJointFaceRect(long userId)
	{
		Rect faceJointRect = new Rect();

		if(kinectManager.IsJointTracked(userId, (int)KinectInterop.JointType.Head))
		{
			Vector3 posHeadRaw = kinectManager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.Head);
			
			if(posHeadRaw != Vector3.zero)
			{
				Vector2 posDepthHead = kinectManager.MapSpacePointToDepthCoords(posHeadRaw);
				ushort depthHead = kinectManager.GetDepthForPixel((int)posDepthHead.x, (int)posDepthHead.y);
				
				Vector3 sizeHalfFace = new Vector3(faceWidth / 2f, faceHeight / 2f, 0f);
				Vector3 posFaceRaw1 = posHeadRaw - sizeHalfFace;
				Vector3 posFaceRaw2 = posHeadRaw + sizeHalfFace;
				
				Vector2 posDepthFace1 = kinectManager.MapSpacePointToDepthCoords(posFaceRaw1);
				Vector2 posDepthFace2 = kinectManager.MapSpacePointToDepthCoords(posFaceRaw2);

				if(posDepthFace1 != Vector2.zero && posDepthFace2 != Vector2.zero && depthHead > 0)
				{
					Vector2 posColorFace1 = kinectManager.MapDepthPointToColorCoords(posDepthFace1, depthHead);
					Vector2 posColorFace2 = kinectManager.MapDepthPointToColorCoords(posDepthFace2, depthHead);
					
					if(!float.IsInfinity(posColorFace1.x) && !float.IsInfinity(posColorFace1.y) &&
					   !float.IsInfinity(posColorFace2.x) && !float.IsInfinity(posColorFace2.y))
					{
						faceJointRect.x = posColorFace1.x;
						faceJointRect.y = posColorFace2.y;
						faceJointRect.width = Mathf.Abs(posColorFace2.x - posColorFace1.x);
						faceJointRect.height = Mathf.Abs(posColorFace2.y - posColorFace1.y);
					}
				}
			}
		}

		return faceJointRect;
	}

}
