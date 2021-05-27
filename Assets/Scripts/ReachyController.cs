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
        public GameObject gameObject;
    }

    [System.Serializable]
    public struct SerializableMotor
    {
        public string name;
        public float present_position;
    }

    [System.Serializable]
    public struct SerializableSensor
    {
        public string name;
        public float sensor_state;
    }

    [System.Serializable]
    public struct SerializableState
    {
        public List<SerializableMotor> motors;
        public string left_eye;
        public string right_eye;
        public float left_force_sensor;
        public float right_force_sensor;
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
        // public ForceSensor leftGripperForceSensor, rightGripperForceSensor;
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

        public void HandleCommand(Dictionary<string, float> commands)
        {
            foreach(KeyValuePair<string, float> kvp in commands )
            {
                SetMotorTargetPosition(kvp.Key, kvp.Value);
            }
        }

        public List<SerializableMotor> GetCurrentMotorsState(Dictionary<string, JointField> request)
        {
            List<SerializableMotor> motorsList = new List<SerializableMotor>();
            foreach(KeyValuePair<string, JointField> kvp in request )
            {
                Motor m = name2motor[kvp.Key];
                float position = m.presentPosition - name2motor[kvp.Key].offset;
                if(!name2motor[kvp.Key].isDirect)
                {
                    position *= -1;
                }
                motorsList.Add(new SerializableMotor() { name=m.name,  present_position=position});
            }
            return motorsList;
        }

        public List<SerializableSensor> GetCurrentSensorsState(string[] request)
        {
            List<SerializableSensor> sensorsList = new List<SerializableSensor>();
            // foreach(var sensor in request)
            // {
            //     Sensor s;
                
            //     float state = s.gameObject.GetComponent<ForceSensor>().currentForce;
            //     sensorsList.Add(new SerializableSensor() { name=s.name,  sensor_state=state});
            // }
            return sensorsList;
        }

        public SerializableState GetCurrentState()
        {
            
            SerializableState currentState = new SerializableState() { 
                motors = new List<SerializableMotor>(), 
                left_eye=leftEyeFrame,
                right_eye=rightEyeFrame,
                // left_force_sensor=leftGripperForceSensor.currentForce,
                // right_force_sensor=rightGripperForceSensor.currentForce,
            };

            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];
                currentState.motors.Add(new SerializableMotor() { name=m.name,  present_position=m.presentPosition});
            }

            return currentState;
        }
    }
}