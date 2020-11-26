﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

[SelectionBase]
public class Dial : MonoBehaviour
{
    public float maxDistance = 0.05f;
    private Quaternion originalRotation;
    private Quaternion startPinchRotationA = Quaternion.identity;
    private Quaternion startPinchRotationB = Quaternion.identity;

    [SerializeField]
    private PinchDetector _pinchDetectorA;
    public PinchDetector PinchDetectorA
    {
        get
        {
            return _pinchDetectorA;
        }
        set
        {
            _pinchDetectorA = value;
        }
    }

    [SerializeField]
    private PinchDetector _pinchDetectorB;
    public PinchDetector PinchDetectorB
    {
        get
        {
            return _pinchDetectorB;
        }
        set
        {
            _pinchDetectorB = value;
        }
    }

    private Transform control;
    // Start is called before the first frame update
    void Start()
    {
        if (_pinchDetectorA == null) _pinchDetectorA = Game.Instance.pinchDetectorA;
        if (_pinchDetectorB == null) _pinchDetectorB = Game.Instance.pinchDetectorB;

        control = transform.Find("Control");
        originalRotation = control.rotation;

        CreatePositions();
    }

    private List<Vector3> positionsList;
    public float sensitivity;
    public float dialValue = 0;

    public int positions = 3;
    public float radius = 1.0f;
    public float arcWidth = 360f;
    void CreatePositions()
    {
        positionsList = new List<Vector3>();

        float x;
        float y = 0f;
        float z;

        float angle = 0f;

        for (int i = 0; i < positions; i++)
        {
            z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            positionsList.Add(new Vector3(x, y, z));

            angle += (arcWidth / positions);
        }
    }

    private void OnDrawGizmos()
    {
        if (rotationHeld)
        {
            for (int i = 0; i < positionsList.Count; i++)
            {
                //Debug.Log(positionsList[i]);
                Vector3 pos = transform.TransformPoint(positionsList[i]);
                Debug.DrawLine(transform.position, pos, Color.green);
            }

            int active = ClosestPosition(control.right);

            Utils.DrawCircle(transform.TransformPoint(positionsList[active]), Vector3.up, 0.02f, Color.red);

        }
    }

    private void drawSpheres()
    {
        foreach (var item in VisualizePosition.spheres)
        {
            Destroy(item);
        }
        VisualizePosition.spheres.Clear();
        if (rotationHeld)
        {
            for (int i = 0; i < positionsList.Count; i++)
            {
                //Debug.Log(positionsList[i]);
                Vector3 pos = transform.TransformPoint(positionsList[i]);
                VisualizePosition.Create(this.gameObject, pos, 0.005f);
            }

            int active = ClosestPosition(control.right);
            VisualizePosition.Create(this.gameObject, transform.TransformPoint(positionsList[active]), 0.02f);
        }
    }

    private int ClosestPosition(Vector3 direction)
    {
        var maxDot = -Mathf.Infinity;
        int index = 0;

        for (int i = 0; i < positionsList.Count; i++)
        {
            var t = Vector3.Dot(direction, positionsList[i]);
            if (t > maxDot)
            {
                index = i;
                maxDot = t;
            }
        }
        return index;
    }

    private bool rotationHeld = false;
    // Update is called once per frame
    void Update()
    {
        drawSpheres();

        float PinchADist = float.PositiveInfinity;
        float PinchBDist = float.PositiveInfinity;

        bool didUpdate = false;
        if (_pinchDetectorA != null)
        {
            didUpdate |= _pinchDetectorA.DidChangeFromLastFrame;
            PinchADist = (_pinchDetectorA.transform.position - transform.position).magnitude;
        }
        if (_pinchDetectorB != null)
        {
            didUpdate |= _pinchDetectorB.DidChangeFromLastFrame;
            PinchBDist = (_pinchDetectorB.transform.position - transform.position).magnitude;
        }

        if (_pinchDetectorA != null && _pinchDetectorA.IsActive && PinchADist < maxDistance)
        {
            if (!rotationHeld)
            {
                rotationHeld = true;
                startPinchRotationA = _pinchDetectorA.Rotation;
                return;
            }
            RotateDial(_pinchDetectorA, startPinchRotationA);
        }
        else if (_pinchDetectorB != null && _pinchDetectorB.IsActive && PinchBDist < maxDistance)
        {
            if (!rotationHeld)
            {
                rotationHeld = true;
                startPinchRotationB = _pinchDetectorB.Rotation;
                return;
            }
            RotateDial(_pinchDetectorB, startPinchRotationB);
        }
        else
        {
            rotationHeld = false;
            originalRotation = control.rotation;
        }
    }

  
    private void RotateDial(PinchDetector singlePinch, Quaternion startPinchRotation)
    {
        Vector3 p = singlePinch.Rotation * Vector3.right;
        p.y = 0;
        Quaternion currentRotation = Quaternion.LookRotation(p);

        Vector3 l = startPinchRotation * Vector3.right;
        l.y = 0;
        Quaternion startRotation = Quaternion.LookRotation(l);

        Quaternion difference = currentRotation * Quaternion.Inverse(startRotation);

        Quaternion target = originalRotation * difference;

        control.rotation = Quaternion.Slerp(control.rotation, target, Time.deltaTime * 20f);
        //transform.LookAt(p);
    }
}