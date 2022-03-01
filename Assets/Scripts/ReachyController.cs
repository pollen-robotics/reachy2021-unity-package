﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

using Grpc.Core;

using Reachy.Sdk.Joint;
using Reachy.Sdk.Camera;
using Reachy.Sdk.Fan;

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
    public class Fan
    {
        public string name;
        public bool state;
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
    public struct SerializableFan
    {
        public string name;
        public bool fan_state;
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
        public Fan[] fans;
        public Sensor[] sensors;
        public UnityEngine.Camera leftEye, rightEye;
        public GameObject head;

        private Dictionary<string, Motor> name2motor;
        private Dictionary<string, Sensor> name2sensor;
        private Dictionary<string, Fan> name2fan;
        private string leftEyeFrame, rightEyeFrame;

        const int resWidth = 320;
        const int resHeight = 240;

        Texture2D texture;

        UnityEngine.Quaternion baseHeadRot;
        UnityEngine.Quaternion targetHeadRot;
        Vector3 headOrientation;
        float headRotDuration;

        private ZoomLevel zoomLevelLeft;
        private ZoomLevel zoomLevelRight;
        private float[] zoomArray = new float[]{1.0f, 40.0f, 70.0f, 100.0f};

        void Awake()
        {
            name2motor = new Dictionary<string, Motor>();

            name2sensor = new Dictionary<string, Sensor>();

            name2fan = new Dictionary<string, Fan>();

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

            for (int i = 0; i < fans.Length; i++)
            {
                Fan f = fans[i];
                name2fan[f.name] = f;
            }

            leftEye.targetTexture = new RenderTexture(resWidth, resHeight, 0);
            rightEye.targetTexture = new RenderTexture(resWidth, resHeight, 0);
            zoomLevelLeft = new ZoomLevel{ Level = ZoomLevelPossibilities.Out };
            zoomLevelRight = new ZoomLevel{ Level = ZoomLevelPossibilities.Out };
            texture = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            headOrientation = new Vector3(0, 0, 0);
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
                else
                {
                    m.presentPosition = m.targetPosition;
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
        }

        void SetFanState(string fanName, bool targetState)
        {
            if(fanName != "neck_fan")
            {
                name2fan[fanName].state = targetState;
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

                if(motorName == "neck_roll")
                {
                    headOrientation[0] = kvp.Value;
                }
                if(motorName == "neck_pitch")
                {
                    headOrientation[1] = kvp.Value;
                }
                if(motorName == "neck_yaw")
                {
                    headOrientation[2] = kvp.Value;
                }
            }
           
            UnityEngine.Quaternion euler_request = UnityEngine.Quaternion.Euler(headOrientation[1], headOrientation[0], -headOrientation[2]);
            HandleHeadOrientation(euler_request);
        }

        public void HandleFanCommand(Dictionary<FanId, bool> commands)
        {
            foreach(KeyValuePair<FanId, bool> kvp in commands )
            {
                string fanName;
                switch(kvp.Key.IdCase)
                {
                    case FanId.IdOneofCase.Name:
                        fanName = kvp.Key.Name;
                        break;
                    case FanId.IdOneofCase.Uid:
                        fanName = fans[kvp.Key.Uid].name;
                        break;
                    default:
                        fanName = kvp.Key.Name;
                        break;
                }
                SetFanState(fanName, kvp.Value);
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

        public List<SerializableFan> GetCurrentFansState(Google.Protobuf.Collections.RepeatedField<Reachy.Sdk.Fan.FanId> request)
        {
            List<Fan> fanRequest = new List<Fan>();

            foreach(var fan in request)
            {
                switch(fan.IdCase)
                {
                    case FanId.IdOneofCase.Name:
                        fanRequest.Add(name2fan[fan.Name]);
                        break;
                    case FanId.IdOneofCase.Uid:
                        fanRequest.Add(fans[fan.Uid]);
                        break;
                }
            }

            List<SerializableFan> fansList = new List<SerializableFan>();

            foreach(var fan in fanRequest)
            {
                bool state = fan.state;
                fansList.Add(new SerializableFan() { name=fan.name,  fan_state=state});
            }

            return fansList;
        }

        public SerializableView GetCurrentView()
        {
            
            SerializableView currentView = new SerializableView() { 
                left_eye=leftEyeFrame,
                right_eye=rightEyeFrame,
            };

            return currentView;
        }

        public void HandleHeadOrientation(UnityEngine.Quaternion q)
        {
            targetHeadRot = q;
        }

        void UpdateHeadOrientation()
        {
            head.transform.localRotation = targetHeadRot;
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