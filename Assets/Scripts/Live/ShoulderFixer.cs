using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MR;

public class ShoulderFixer {
    protected AvatarScaler avatarScaler;

    float maxConfidence = 0.0f;
    float maxConfidenceWidth = 0.0f;
    float currConfidence = 0.0f;
    float currConfidenceWidth = 0.0f;

    public Vector3 correctedShoulderLeft;
    public Vector3 correctedShoulderRight;
    public Vector3 origShoulderLeft;
    public Vector3 origShoulderRight;
    public Vector3 origSpineShoulder;
    public Vector3 origNeck;
    public Vector3 shoulderTForward;
    public Vector3 shoulderTUp;
    public Vector3 shoulderTRight;

    public ShoulderFixer(AvatarScaler avScaler)
    {
        avatarScaler = avScaler;
    }

    public void UpdateJointPositions(Vector3 shoulderLeft, Vector3 shoulderRight, Vector3 spineShoulder, Vector3 neck)
    {
        origShoulderLeft = shoulderLeft;
        origShoulderRight = shoulderRight;
        origSpineShoulder = spineShoulder;
        origNeck = neck;
        shoulderTUp = (neck - spineShoulder).normalized;
        shoulderTForward = Vector3.Cross(shoulderLeft - shoulderRight, shoulderTUp).normalized;
        shoulderTRight = Vector3.Cross(shoulderTUp, shoulderTForward).normalized;

        Debug.Log("shoulderUp = " + shoulderTUp);
        Debug.Log("shoulderForward = " + shoulderTForward);

        Vector3 shoulderDelta = (shoulderLeft - shoulderRight);
        // The more horizontal with the camera, the more confident we are.
        float shouldersDotCamera = Vector3.Dot(shoulderDelta.normalized, avatarScaler.foregroundCamera.transform.forward);
        currConfidence = 1.0f - Mathf.Abs(shouldersDotCamera);
        currConfidenceWidth = shoulderDelta.magnitude;

        if (currConfidence > maxConfidence)
        {
            maxConfidence = currConfidence;
            maxConfidenceWidth = currConfidenceWidth;
        }
        //Debug.Log("currConfidence = " + currConfidence);
        //Debug.Log("currConfidenceWidth = " + currConfidenceWidth);
        //Debug.Log("maxConfidence = " + maxConfidence);
        //Debug.Log("maxConfidenceWidth = " + maxConfidenceWidth);

        // Correct shoulders based on shouldersDotCamera, current implementation seems to overrotate the spine -HH
        Vector3 relativeShoulderLeft = shoulderLeft - spineShoulder;
        Vector3 relativeShoulderRight = shoulderRight - spineShoulder;
        relativeShoulderLeft.RotatePointAroundPivot(shoulderTUp, new Vector3(0, 30.0f * shouldersDotCamera, 0));
        relativeShoulderRight.RotatePointAroundPivot(shoulderTUp, new Vector3(0, -30.0f * shouldersDotCamera, 0));
        correctedShoulderLeft = shoulderLeft;// spineShoulder + relativeShoulderLeft;
        correctedShoulderRight = shoulderRight; // spineShoulder + relativeShoulderRight;
    }

    public float GetWeightedWidth()
    {
        // Get weighted width;
        float totalConfidence = currConfidence + maxConfidence;
        float weightedWidth = 
            maxConfidence / totalConfidence * maxConfidenceWidth +
            currConfidence / totalConfidence * currConfidenceWidth;

        return weightedWidth;
    }

    internal Vector3 GetCorrectedShoulderLeftPos()
    {
        throw new NotImplementedException();
    }

    internal Vector3 GetCorrectedShoulderRightPos()
    {
        throw new NotImplementedException();
    }
}
