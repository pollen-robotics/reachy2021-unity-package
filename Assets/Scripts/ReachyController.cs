﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

using Grpc.Core;

using Reachy.Sdk.Joint;
using Reachy.Sdk.Camera;

namespace Reachy
{
    [System.Serializable]
    public class Motor
    {
        public string name;
        public int uid;
        public GameObject gameObject;
        public float targetPosition;
        public float presentPosition;
        public float offset;
        public bool isDirect;
    }

    [System.Serializable]
    public class Sensor
    {
        public string name;
        public GameObject sensorObject;
        public float currentState;
    }

    [System.Serializable]
    public struct SerializableMotor
    {
        public string name;
        public int uid;
        public float present_position;
        public float goal_position;
    }

    [System.Serializable]
    public struct SerializableSensor
    {
        public string name;
        public float sensor_state;
    }

    [System.Serializable]
    public struct SerializableView
    {
        public string left_eye;
        public string right_eye;
    }

    [System.Serializable]
    public struct MotorCommand
    {
        public string name;
        public float goal_position;
    }

    [System.Serializable]
    public struct SerializableCommands
    {
        public List<MotorCommand> motors;
    }

    public class ReachyController : MonoBehaviour
    {
        public Motor[] motors;
        public UnityEngine.Camera leftEye, rightEye;
        public Sensor[] sensors;
        public GameObject head;

        float _timeElapsed;

        private Dictionary<string, Motor> name2motor;
        private Dictionary<string, Sensor> name2sensor;
        private string leftEyeFrame, rightEyeFrame;

        const int resWidth = 320;
        const int resHeight = 240;

        Texture2D texture;

        Quaternion baseHeadRot;
        Quaternion targetHeadRot;
        float headRotDuration;
        bool needUpdateHeadRot;

        private ZoomLevel zoomLevelLeft;
        private ZoomLevel zoomLevelRight;
        private float[] zoomArray = new float[]{1.0f, 40.0f, 70.0f, 100.0f};

        void Awake()
        {
            name2motor = new Dictionary<string, Motor>();

            name2sensor = new Dictionary<string, Sensor>();

            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];
                m.uid = i;
                name2motor[m.name] = m;
            }

            for (int i = 0; i < sensors.Length; i++)
            {
                Sensor s = sensors[i];
                name2sensor[s.name] = s;
            }

            leftEye.targetTexture = new RenderTexture(resWidth, resHeight, 0);
            rightEye.targetTexture = new RenderTexture(resWidth, resHeight, 0);
            zoomLevelLeft = new ZoomLevel{ Level = ZoomLevelPossibilities.Out };
            zoomLevelRight = new ZoomLevel{ Level = ZoomLevelPossibilities.Out };
            needUpdateHeadRot = false;
            _timeElapsed = 0.0f;
            texture = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            baseHeadRot = head.transform.localRotation;
            StartCoroutine("UpdateCameraData");
        }

        void Update()
        {
            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];

                if(!m.name.StartsWith("neck"))
                {
                    JointController joint = m.gameObject.GetComponent<JointController>();
                    joint.RotateTo(m.targetPosition);

                    m.presentPosition = joint.GetPresentPosition();
                }
            }

            for (int i = 0; i < sensors.Length; i++)
            {
                Sensor s = sensors[i];

                ForceSensor fSensor = s.sensorObject.GetComponent<ForceSensor>();
                s.currentState = fSensor.currentForce;
            }

           UpdateHeadOrientation();
           UpdateCameraZoom();
        }

        IEnumerator UpdateCameraData()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                leftEyeFrame = GetEyeRawTextureData(leftEye);
                rightEyeFrame = GetEyeRawTextureData(rightEye);
            }
        }

        string GetEyeRawTextureData(UnityEngine.Camera camera)
        {
            RenderTexture.active = camera.targetTexture;
            texture.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            texture.Apply();

            return Convert.ToBase64String(texture.EncodeToJPG());
        }

        void SetMotorTargetPosition(string motorName, float targetPosition)
        {
            targetPosition += name2motor[motorName].offset;
            if(!name2motor[motorName].isDirect)
            {
                targetPosition *= -1;
            }
            name2motor[motorName].targetPosition = targetPosition;

            if(motorName == "neck_disk_top")
            {
                headRotDuration = name2motor[motorName].targetPosition;
                needUpdateHeadRot = true;
            }
        }

        public void HandleCommand(Dictionary<JointId, float> commands)
        {
            foreach(KeyValuePair<JointId, float> kvp in commands )
            {
                string motorName;
                switch(kvp.Key.IdCase)
                {
                    case JointId.IdOneofCase.Name:
                        motorName = kvp.Key.Name;
                        break;
                    case JointId.IdOneofCase.Uid:
                        motorName = motors[kvp.Key.Uid].name;
                        break;
                    default:
                        motorName = kvp.Key.Name;
                        break;
                }
                SetMotorTargetPosition(motorName, kvp.Value);
            }
        }

        public List<SerializableMotor> GetCurrentMotorsState(Dictionary<JointId, JointField> request)
        {
            List<SerializableMotor> motorsList = new List<SerializableMotor>();
            foreach(KeyValuePair<JointId, JointField> kvp in request )
            {
                Motor m;
                float position;
                float target_position;
                switch(kvp.Key.IdCase)
                {
                    case JointId.IdOneofCase.Name:
                        m = name2motor[kvp.Key.Name];
                        position = m.presentPosition;
                        target_position = m.targetPosition;
                        if(!name2motor[kvp.Key.Name].isDirect)
                        {
                            position *= -1;
                            target_position *= -1;
                        }
                        position -= name2motor[kvp.Key.Name].offset;
                        target_position -= name2motor[kvp.Key.Name].offset;
                        break;
                    case JointId.IdOneofCase.Uid:
                        m = motors[kvp.Key.Uid];
                        position = m.presentPosition;
                        target_position = m.targetPosition;
                        if(!motors[kvp.Key.Uid].isDirect)
                        {
                            position *= -1;
                            target_position *= -1;
                        }
                        position -= motors[kvp.Key.Uid].offset;
                        target_position -= motors[kvp.Key.Uid].offset;
                        break;
                    default:
                        m = name2motor[kvp.Key.Name];
                        position = m.presentPosition;
                        target_position = m.targetPosition;
                        if(!name2motor[kvp.Key.Name].isDirect)
                        {
                            position *= -1;
                            target_position *= -1;
                        }
                        position -= name2motor[kvp.Key.Name].offset;
                        target_position -= name2motor[kvp.Key.Name].offset;
                        break;
                }
                motorsList.Add(new SerializableMotor() { name=m.name, uid = m.uid, present_position=Mathf.Deg2Rad*position, goal_position=Mathf.Deg2Rad*target_position});
            }
            return motorsList;
        }

        public List<SerializableSensor> GetCurrentSensorsState(Google.Protobuf.Collections.RepeatedField<Reachy.Sdk.Joint.SensorId> request)
        {
            List<Sensor> sensorRequest = new List<Sensor>();

            foreach(var sensor in request)
            {
                switch(sensor.IdCase)
                {
                    case SensorId.IdOneofCase.Name:
                        sensorRequest.Add(name2sensor[sensor.Name]);
                        break;
                    case SensorId.IdOneofCase.Uid:
                        sensorRequest.Add(sensors[sensor.Uid]);
                        break;
                }
            }

            List<SerializableSensor> sensorsList = new List<SerializableSensor>();

            foreach(var sensor in sensorRequest)
            {
                float state = sensor.currentState;
                sensorsList.Add(new SerializableSensor() { name=sensor.name,  sensor_state=state});
            }

            return sensorsList;
        }

        public SerializableView GetCurrentView()
        {
            
            SerializableView currentView = new SerializableView() { 
                left_eye=leftEyeFrame,
                right_eye=rightEyeFrame,
            };

            return currentView;
        }

        public void HandleHeadOrientation(Quaternion q)
        {
            targetHeadRot = q;
        }

        void UpdateHeadOrientation()
        {
            if(needUpdateHeadRot)
            {
                _timeElapsed += Time.deltaTime;
                if(_timeElapsed >= headRotDuration)
                {
                    needUpdateHeadRot = false;
                    head.transform.localRotation = targetHeadRot;
                    baseHeadRot = targetHeadRot;

                    _timeElapsed = 0;
                    return;
                }

                float fTime = _timeElapsed / headRotDuration;
                head.transform.localRotation = Quaternion.Lerp(baseHeadRot, targetHeadRot, fTime);
            }
            else
            {
                head.transform.localRotation = baseHeadRot;
            }
        }

        public void HandleCameraZoom(CameraId id, ZoomLevelPossibilities zoomLevel)
        {
            if(id == CameraId.Left)
            {
                zoomLevelLeft.Level = zoomLevel;
            }
            else
            {
                zoomLevelRight.Level = zoomLevel;
            }
        }

        public ZoomLevel GetCameraZoom(CameraId id)
        {
             if(id == CameraId.Left)
            {
                return zoomLevelLeft;
            }
            else
            {
                return zoomLevelRight;
            }
        }

        void UpdateCameraZoom()
        {
            foreach(UnityEngine.Camera camera in UnityEngine.Camera.allCameras)
            {
                if(camera.name == "Right Camera")
                {
                    camera.fieldOfView = zoomArray[(int)zoomLevelRight.Level];
                }

                if(camera.name == "Left Camera")
                {
                    camera.fieldOfView = zoomArray[(int)zoomLevelLeft.Level];
                }
            }
        }
    }
}