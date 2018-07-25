using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MR;

public class ShoulderFixer {
    protected AvatarScaler avatarScaler;

    struct ConfidenceValue
    {
        public float maxConfidence;
        public float maxConfidenceValue;
        public float currConfidence;
        public float currConfidenceValue;

        public void Update(float confidence, float value)
        {
            currConfidence = confidence;
            currConfidenceValue = value;
            if (currConfidence > maxConfidence)
            {
                maxConfidence = currConfidence;
                maxConfidenceValue = currConfidenceValue;
            }
        }

        public float GetWeightedValue()
        {
            float totalConfidence = currConfidence + maxConfidence;
            float weightedValue =
                maxConfidence / totalConfidence * maxConfidenceValue +
                currConfidence / totalConfidence * currConfidenceValue;

            return weightedValue;
        }
    }

    ConfidenceValue BodyWidth;
    ConfidenceValue BodyHeight;
    ConfidenceValue LegsHeight;

    public Vector3 correctedShoulderLeft;
    public Vector3 correctedShoulderRight;
    public Vector3 origShoulderLeft;
    public Vector3 origShoulderRight;
    public Vector3 origElbowLeft;
    public Vector3 elbowOnPlane;
    public Vector3 origSpineShoulder;
    public Vector3 origNeck;
    public Vector3 shoulderTForward;
    public Vector3 shoulderTUp;
    public Vector3 shoulderTRight;
    public float elbowLeftDotShoulderUpPlane;
    public float elbowRightDotShoulderUpPlane;

    public ShoulderFixer(AvatarScaler avScaler)
    {
        avatarScaler = avScaler;
    }

    public void UpdateJointPositions(Vector3 hipLeft, Vector3 hipRight,
        Vector3 ankleLeft, Vector3 ankleRight,
        Vector3 kneeLeft, Vector3 kneeRight,
        Vector3 shoulderLeft, Vector3 shoulderRight, 
        Vector3 elbowLeft, Vector3 elbowRight, 
        Vector3 spineShoulder, Vector3 spineBase, Vector3 neck)
    {
        origElbowLeft = elbowLeft;
        origShoulderLeft = shoulderLeft;
        origShoulderRight = shoulderRight;
        origSpineShoulder = spineShoulder;
        origNeck = neck;
        shoulderTUp = (neck - spineShoulder).normalized;
        shoulderTForward = Vector3.Cross(shoulderLeft - shoulderRight, shoulderTUp).normalized;
        shoulderTRight = Vector3.Cross(shoulderTUp, shoulderTForward).normalized;

        //Debug.Log("shoulderUp = " + shoulderTUp);
        //Debug.Log("shoulderForward = " + shoulderTForward);

        Vector3 shoulderDelta = (shoulderLeft - shoulderRight);
        // The more horizontal with the camera, the more confident about the width we are.
        float shouldersDotCamera = Vector3.Dot(shoulderDelta.normalized, avatarScaler.foregroundCamera.transform.forward);
        float confidence = 1.0f - Mathf.Abs(shouldersDotCamera);
        float value = shoulderDelta.magnitude;
        BodyWidth.Update(confidence, value);

        // The higher that the user lifts their arms above her head while facing camera directly, the more confidence about the height we are.
        float spineDotCameraUp = Vector3.Dot((spineShoulder - spineBase).normalized, avatarScaler.foregroundCamera.transform.up);
        spineDotCameraUp = Mathf.Clamp(spineDotCameraUp, 0, float.MaxValue); // Clamp at zero (no negative values);
        confidence =
            (1.0f - Mathf.Abs(shouldersDotCamera)) * // shoulders squared with camera forward?
            (1.0f - spineDotCameraUp) * // spine aligned with CameraUp?
            Mathf.Clamp((elbowLeft - shoulderLeft).normalized.y, 0, float.MaxValue) * // Left elbow above shoulder
            Mathf.Clamp((elbowRight - shoulderRight).normalized.y, 0, float.MaxValue) // Right elbow above shoulder
            ;
        Vector3 posHipCenter = (hipLeft + hipRight) / 2f;
        Vector3 posShoulderCenter = (shoulderLeft + shoulderRight) / 2f;
        Vector3 posAnkleCenter = (ankleLeft + ankleRight) / 2f;
        value = (posShoulderCenter - posHipCenter).magnitude;
        BodyHeight.Update(confidence, value);

        //Debug.Log("spineBase=" + spineBase);
        //Debug.Log("posHipCenter=" + posHipCenter);
        //Debug.Log("posAnkleCenter=" + posAnkleCenter);

        // Use same confidence as for BodyHeight
        float leftLegLength = (hipLeft - kneeLeft).magnitude + (kneeLeft - ankleLeft).magnitude;
        float rightLegLength = (hipRight - kneeRight).magnitude + (kneeRight - ankleRight).magnitude;
        value = Mathf.Max(leftLegLength, rightLegLength);
        LegsHeight.Update(confidence, value);
        
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

        Vector3 shoulderToElbowLeft = elbowLeft - shoulderLeft;
        elbowOnPlane = Vector3.ProjectOnPlane(shoulderToElbowLeft, shoulderTUp) + shoulderLeft;
        elbowLeftDotShoulderUpPlane = Vector3.Dot(shoulderToElbowLeft.normalized, (elbowOnPlane - shoulderLeft).normalized);
        //Debug.Log("elbowLeftDotShoulderUpPlane " + elbowLeftDotShoulderUpPlane);
        Vector3 shoulderToElbowRight = elbowLeft - shoulderLeft;
        elbowOnPlane = Vector3.ProjectOnPlane(shoulderToElbowRight, shoulderTUp) + shoulderRight;
        elbowRightDotShoulderUpPlane = Vector3.Dot(shoulderToElbowRight.normalized, (elbowOnPlane - shoulderRight).normalized);
    }

    public float GetWeightedWidth()
    {
        return BodyWidth.GetWeightedValue();
    }

    public float GetMaxConfidenceBodyHeight()
    {
        // Get weighted width;
        //float totalConfidence = currHeightConfidence + maxHeightConfidence;
        //float weightedHeight =
        //    maxHeightConfidence / totalConfidence * maxConfidenceHeight +
        //    currHeightConfidence / totalConfidence * currConfidenceHeight;

        return BodyHeight.maxConfidenceValue;
    }

    public float GetMaxConfidenceLegsHeight()
    {
        return LegsHeight.maxConfidenceValue;
    }

    public float GetMaxLegsHeightOffset()
    {
        return (LegsHeight.maxConfidenceValue - LegsHeight.currConfidenceValue) * 1.0f;
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
