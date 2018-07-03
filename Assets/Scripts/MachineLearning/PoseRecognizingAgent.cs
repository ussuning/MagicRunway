using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseRecognizingAgent : Agent {

    public GameObject[] particles ;

    private KinectManager manager;
    private long KinectUserId;

    private int estPoseIdx;
    private float posingTimeEllapsed = 0f;
    private int prevAnimationIdx = -1;

    void Start()
    {
        if (manager == null)
            manager = KinectManager.Instance;
    }

    void Update ()
    {
        posingTimeEllapsed += Time.deltaTime;
    }

    void LateUpdate()
    {
        if(estPoseIdx > 0 && posingTimeEllapsed > SystemConfigs.PosingTime)
        {
            posingTimeEllapsed = 0f;
            if (particles.Length > 0)
            {
                int randAnimIdx = -1;
                do
                {
                    randAnimIdx = Random.Range(0, particles.Length);
                } while (randAnimIdx == prevAnimationIdx && particles.Length > 1);

                Vector3 ParticlePosistion = Vector3.zero;
                if (KinectUserId > 0)
                    ParticlePosistion = manager.GetUserPosition(KinectUserId);
                GameObject ps = Instantiate(particles[randAnimIdx], ParticlePosistion, Quaternion.identity);
                //particles[randAnimIdx].Play();
                prevAnimationIdx = randAnimIdx;
            } 
        }
    }

    public override void InitializeAgent()
    {
        manager = KinectManager.Instance;
    }

    public override void CollectObservations()
    {
        KinectUserId = manager.GetPrimaryUserID(); //Delete later
        if (manager.IsUserInKinectView(KinectUserId))
        {
            if (SystemConfigs.CollectUserRotation)
            {
                Vector3 userRot = manager.GetUserOrientation(KinectUserId, false).eulerAngles;
                userRot = NormalizeAngles(userRot);
                AddVectorObs(userRot);
            }
            if (SystemConfigs.CollectJointDirectionData)
            {
                for (int i = 0; i < SystemConfigs.DetectedJoints.Length; i++)
                {
                    if (manager.IsJointTracked(KinectUserId, (int)SystemConfigs.DetectedJoints[i]))
                    {
                        int JointIdx = (int)SystemConfigs.DetectedJoints[i];
                        Vector3 JointDirection = manager.GetJointDirection(KinectUserId, JointIdx);
                        AddVectorObs(JointDirection);
                    }
                }
            }
            if (SystemConfigs.CollectJointOrientationData)
            {
                for (int i = 0; i < SystemConfigs.DetectedJoints.Length; i++)
                {
                    if (manager.IsJointTracked(KinectUserId, (int)SystemConfigs.DetectedJoints[i]))
                    {
                        int JointIdx = (int)SystemConfigs.DetectedJoints[i];
                        Vector3 JointOrientation = manager.GetJointOrientation(KinectUserId, JointIdx).eulerAngles;
                        JointOrientation = NormalizeAngles(JointOrientation);
                        AddVectorObs(JointOrientation);
                    }
                }
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int newPoseIdx = Mathf.RoundToInt(vectorAction[0]);
        if(estPoseIdx != newPoseIdx)
        {
            estPoseIdx = newPoseIdx;
            if (estPoseIdx < 0)
                estPoseIdx = 0;

            posingTimeEllapsed = 0f;    
        }
        Debug.Log("estPoseIdx: " + estPoseIdx);
    }

    public override void AgentOnDone()
    {
        //Destroy(gameObject);
    }

    public override void AgentReset()
    {
        estPoseIdx = 0;
        posingTimeEllapsed = 0f;
    }

    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
