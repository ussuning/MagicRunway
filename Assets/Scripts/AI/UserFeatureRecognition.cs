using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ImageJson
{
    public string img = "";
}

public class UserFeatureRecognition : MonoBehaviour
{

    public static UserFeatureRecognition Instance;

    /* Common */
    /***********************************************************************************************************************************************************************************/

    public enum ImageType
    {
        PNG,
        JPG,
    };

    //[Header("Saving Parameters")]
    //public string UserImageFolder = "FaceImages";
    //public ImageType SaveType = ImageType.PNG;

    public bool PredictGender = true;

    //private string userImageFolderPath;

    /* Http */
    /*******************************************************************************************************************************************************/
    public string RequestURL = "http://192.168.2.99:5001/upload";


    /* Kinect */
    /***************************************************************************************************************************************************************************/

    [Header("Kinect Image Data")]
    public Vector2 UserImageSize = new Vector2(0.2f, 0.2f);

    private KinectManager kinectManager;


    /* Mono */
    /***********************************************************************************************************************************************************************************/

    void Awake()
    {
        //userImageFolderPath = GetFolderPath(UserImageFolder);
        Instance = this;
    }

    void Start()
    {
        //ValidateFolder(userImageFolderPath);
        kinectManager = KinectManager.Instance;
    }

    //void ValidateFolder(string folderPath)
    //{
    //    if (!Directory.Exists(folderPath))
    //    {
    //        Directory.CreateDirectory(folderPath);
    //    }
    //}


    /* Common */
    /***********************************************************************************************************************************************************************************/

    /* Public */

    public void ClassifyUserFeatures(long userId)
    {
        Debug.Log(string.Format("[UserFeatureRecognition] ClassifyUserFeatures(long userId = {0})", userId));

        //string userImagePath = GetUserImagePath(userImageFolderPath, userId, SaveType);
        //Debug.Log(string.Format("[UserFeatureRecognition] ClassifyUserFeatures(): userImagePath = {0}", userImagePath));

        Texture2D userTex = GetUserColorTexture(userId);
        Debug.Log(string.Format("[UserFeatureRecognition] ClassifyUserFeatures(): SUCESSFULLY created user texture for user {0}", userId));

        //SaveImage(userTex, userImagePath, SaveType);
        //Debug.Log(string.Format("[UserFeatureRecognition] ClassifyUserFeatures(): SUCESSFULLY saved user {0} image to {1}", userId, userImagePath));

        if (PredictGender)
            StartCoroutine(ClassifyUser(userTex.EncodeToPNG()));
    }
    
    
    /* Private */

    //private void SaveImage(Texture2D imageTex, string savePath, ImageType t)
    //{
    //    Debug.Log(string.Format("[UserFeatureRecognition] SaveImage(Texture2D imageTex, string savePath = {0}, ImageType t = {1}):", savePath, t.ToString()));

    //    byte[] imageBytes;
    //    if (t == ImageType.JPG)
    //    {
    //        Debug.Log(string.Format("[UserFeatureRecognition] SaveImage(): Encoding tex to JPG"));
    //        imageBytes = imageTex.EncodeToJPG();
    //    }
    //    else
    //    {
    //        Debug.Log(string.Format("[UserFeatureRecognition] SaveImage(): Encoding tex to PNG"));
    //        imageBytes = imageTex.EncodeToPNG();
    //    }

    //    Debug.Log(string.Format("[UserFeatureRecognition] SaveImage(): Saving bytes to {0}", savePath));
    //    File.WriteAllBytes(savePath, imageBytes);
    //}

    //private string GetUserImagePath(string folderPath, long userId, ImageType t)
    //{
    //    Debug.Log(string.Format("[UserFeatureRecognition] GetUserImagePath(string folderPath = {0}, long userId = {1}, ImageType t = {2})", folderPath, userId, t.ToString()));

    //    int userIdx = kinectManager.GetUserIndexById(userId);
    //    if (t == ImageType.JPG)
    //    {
    //        return folderPath + "user" + userIdx + "_" + userId.ToString() + ".jpg";
    //    }
    //    else
    //    {
    //        return folderPath + "user" + userIdx + "_" + userId.ToString() + ".png";
    //    }
    //}

    //private string GetFolderPath(string folderName)
    //{
    //    return Application.dataPath + "/" + folderName + "/";
    //}

    /* Kinect */
    /***************************************************************************************************************************************************************************/

    private Texture2D GetUserColorTexture(long userId)
    {
        Debug.Log(string.Format("[UserFeatureRecognition] GetUserColorTexture(long userId = {0})", userId));

        Rect userRect = GetUserRect(userId, UserImageSize);
        Texture2D kinectColorImg = kinectManager.GetUsersClrTex2D();
        Texture2D userColorTex = CropUserTexture(kinectColorImg, userRect);

        return userColorTex;
    }

    private Rect GetUserRect(long userId, Vector3 imageSize)
    {
        Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(long userId = {0}, Vector3 imageSize = {1})", userId, imageSize));

        Rect userRect = new Rect();
        if (kinectManager && kinectManager.IsInitialized())
        {
            if (kinectManager.IsJointTracked(userId, (int)KinectInterop.JointType.Head))
            {
                Vector3 userPosRaw = kinectManager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.Head);

                if (userPosRaw != Vector3.zero)
                {
                    Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): userPosRaw = {0}", userPosRaw));

                    Vector2 userPosDepth = kinectManager.MapSpacePointToDepthCoords(userPosRaw);
                    ushort depthUser = kinectManager.GetDepthForPixel((int)userPosDepth.x, (int)userPosDepth.y);
                    
                    Vector3 posUserRaw1 = userPosRaw - imageSize;
                    Vector3 posUserRaw2 = userPosRaw + imageSize;
                    Vector2 posDepthUser1 = kinectManager.MapSpacePointToDepthCoords(posUserRaw1);
                    Vector2 posDepthUser2 = kinectManager.MapSpacePointToDepthCoords(posUserRaw2);

                    if (depthUser > 0 && posDepthUser1 != Vector2.zero && posDepthUser2 != Vector2.zero)
                    {
                        Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): userPosDepth = {0}, depthUser = {1}", userPosRaw, depthUser));
                        Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): posUserRaw1 = {0}, posUserRaw2 = {1},   posDepthUser1 = {2}, posDepthUser2 = {3}", posUserRaw1, posUserRaw2, posDepthUser1, posDepthUser2));

                        Vector2 posColorUser1 = kinectManager.MapDepthPointToColorCoords(posDepthUser1, depthUser);
                        Vector2 posColorUser2 = kinectManager.MapDepthPointToColorCoords(posDepthUser2, depthUser);

                        if (!float.IsInfinity(posColorUser1.x) && !float.IsInfinity(posColorUser1.y) &&
                           !float.IsInfinity(posColorUser2.x) && !float.IsInfinity(posColorUser2.y))
                        {
                            Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): posColorUser1 = {0}, posColorUser2 = {1}", posColorUser1, posColorUser2));

                            userRect.x = posColorUser1.x;
                            userRect.y = posColorUser2.y;
                            userRect.width = Mathf.Abs(posColorUser2.x - posColorUser1.x);
                            userRect.height = Mathf.Abs(posColorUser2.y - posColorUser1.y);
                        }
                        else
                        {
                            Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): User {0} has INVALID COLOR position", userId));
                        }
                    }
                    else
                    {
                        Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): User {0} has INVALID DEPTH position", userId));
                    }
                }
                else
                {
                    Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): User {0} has INVALID RAW position", userId));
                }
            }
            else
            {
                Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): User {0} is NOT tracked", userId));
            }
        }
        else
        {
            Debug.Log(string.Format("[UserFeatureRecognition] GetUserRect(): Kinect is not initiated"));
        }

        return userRect;
    }

    private Texture2D CropUserTexture(Texture2D fullImage, Rect userRect)
    {
        Debug.Log(string.Format("[UserFeatureRecognition] CropUserTexture(Texture2D fullImage, Rect userRect = Width{0} Height{1}", userRect.width, userRect.height));

        if (userRect.width > 0 && userRect.height > 0)
        {
            Debug.Log(string.Format("[UserFeatureRecognition] CropUserTexture(): userRect width = {0}, height = {1}", userRect.width, userRect.height));

            if (fullImage)
            {
                int userX = (int)userRect.x;
                int userY = (int)userRect.y;
                int userW = (int)userRect.width;
                int userH = (int)userRect.height;

                if (userX < 0) userX = 0;
                if (userY < 0) userY = 0;
                if ((userX + userW) > fullImage.width) userW = fullImage.width - userX;
                if ((userY + userH) > fullImage.height) userH = fullImage.height - userY;

                Texture2D userTex = new Texture2D(userW, userH);
                Color[] userPixels = fullImage.GetPixels(userX, userY, userW, userH, 0);
                Color[] userPixels_flipped = new Color[userPixels.Length];
                for(int i=0; i<userPixels.Length; i++)
                {
                    userPixels_flipped[userPixels.Length - 1 - i] = userPixels[i]; 
                }
                userTex.SetPixels(userPixels_flipped);
                userTex.Apply();

                return userTex;
            }
            else
            {
                Debug.Log(string.Format("[UserFeatureRecognition] CropUserTexture(): Cannot get Kinect color tex"));
            }
        }
        else
        {
            Debug.Log(string.Format("[UserFeatureRecognition] CropUserTexture(): INVALID user rect size"));
        }

        return null;
    }


    /* Unity Web Request */
    /****************************************************************************************************************************************************************************************/

    IEnumerator ClassifyUser(byte [] img)
    {
        using (UnityWebRequest www = UnityWebRequest.Put(RequestURL, GenerateImageJsonString(img)))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Dictionary<string, string> res = www.GetResponseHeaders();
                foreach (string header in res.Keys)
                {
                    Debug.Log(string.Format("{0}: {1}", header, res[header]));
                }
                Debug.Log(www.downloadHandler.text);
                Debug.Break();
            }
        }
    }

    string GenerateImageJsonString (byte [] img)
    {
        ImageJson ij = new ImageJson();
        ij.img = System.Convert.ToBase64String(img);
        string jsonStr = JsonUtility.ToJson(ij);
        return jsonStr;
    }
}
