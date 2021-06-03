using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

using Grpc.Core;

using Reachy.Sdk.Joint;

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
        public Camera leftEye, rightEye;
        public Sensor[] sensors;
        public GameObject head;
        private Quaternion baseHeadRot;
        private Quaternion[] forwardOrbita;
        private int forwardRange;
        public float diskTopRot, diskMidRot, diskLowRot;

        private Dictionary<string, Motor> name2motor;
        private Dictionary<string, Sensor> name2sensor;
        private string leftEyeFrame, rightEyeFrame;

        const int resWidth = 320;
        const int resHeight = 240;

        Texture2D texture;

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
            texture = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            StartCoroutine("UpdateCameraData");

            baseHeadRot = head.transform.rotation;
            
            TextAsset forward_orbita = Resources.Load("forward-orbita") as TextAsset;
            byte[] bytes = forward_orbita.bytes;

            forwardOrbita = new Quaternion[bytes.Length / 16];
            int j = 0;
            for (int i=0; i<bytes.Length; i+=16)
            {
                float qx = System.BitConverter.ToSingle(bytes, i + 0);
                float qy = System.BitConverter.ToSingle(bytes, i + 4);
                float qz = System.BitConverter.ToSingle(bytes, i + 8);
                float qw = System.BitConverter.ToSingle(bytes, i + 12);

                forwardOrbita[j++] = new Quaternion(qx, qy, qz, qw);
            }
            forwardRange = (int)Mathf.Pow(forwardOrbita.Length, 1.0f / 3.0f);
        }

        void Update()
        {
            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];

                JointController joint = m.gameObject.GetComponent<JointController>();
                joint.RotateTo(m.targetPosition);

                m.presentPosition = joint.GetPresentPosition();
            }

            for (int i = 0; i < sensors.Length; i++)
            {
                Sensor s = sensors[i];

                ForceSensor fSensor = s.sensorObject.GetComponent<ForceSensor>();
                s.currentState = fSensor.currentForce;
            }

            UpdateHeadOrientation();
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

        string GetEyeRawTextureData(Camera camera)
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
                        position = m.presentPosition - name2motor[kvp.Key.Name].offset;
                        target_position = m.targetPosition - name2motor[kvp.Key.Name].offset;
                        if(!name2motor[kvp.Key.Name].isDirect)
                        {
                            position *= -1;
                        }
                        break;
                    case JointId.IdOneofCase.Uid:
                        m = motors[kvp.Key.Uid];
                        position = m.presentPosition - motors[kvp.Key.Uid].offset;
                        target_position = m.targetPosition - motors[kvp.Key.Uid].offset;
                        if(!motors[kvp.Key.Uid].isDirect)
                        {
                            position *= -1;
                            target_position *= -1;
                        }
                        break;
                    default:
                        m = name2motor[kvp.Key.Name];
                        position = m.presentPosition - name2motor[kvp.Key.Name].offset;
                        target_position = m.targetPosition - name2motor[kvp.Key.Name].offset;
                        if(!name2motor[kvp.Key.Name].isDirect)
                        {
                            position *= -1;
                            target_position *= -1;
                        }
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

        void UpdateHeadOrientation()
        {
            Quaternion q = forwardOrbitaActuator(diskTopRot, diskMidRot, diskLowRot);
            head.transform.rotation = q * baseHeadRot;
        }

        Quaternion forwardOrbitaActuator(float diskTopRot, float diskMidRot, float diskLowRot)
        {
            float q1 = (int)Mathf.Clamp(diskTopRot, -150, 30);
            float q2 = (int)Mathf.Clamp(diskMidRot, -150, 30);
            float q3 = (int)Mathf.Clamp(diskLowRot, -150, 30);

            int i = (int)((q1 + 150) / 180 * (forwardRange - 1));
            int j = (int)((q2 + 150) / 180 * (forwardRange - 1));
            int k = (int)((q3 + 150) / 180 * (forwardRange - 1));

            int offset = i * (forwardRange * forwardRange) + j * forwardRange + k;
            Quaternion q = forwardOrbita[offset];
            return Quaternion.Euler(q.eulerAngles.x, -q.eulerAngles.z, q.eulerAngles.y);
        }   
    }
}