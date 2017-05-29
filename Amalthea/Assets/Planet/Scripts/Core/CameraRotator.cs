using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LemonSpawn
{

    public class CameraRotator
    {
        private Vector3 mouseAccel = new Vector3();
        public Vector3 focusPoint = Vector3.zero;
        private Vector3 focusPointCur = Vector3.zero;
        public float scrollWheel, scrollWheelAccel;
        private Vector3 euler;

        private Camera mainCamera;

        public Camera Camera()
        {
            return mainCamera;
        }

        public CameraRotator(Camera cam)
        {
            mainCamera = cam;
        }


        public void ForceCamera(CameraRotator cr)
        {
            mainCamera.transform.rotation = cr.Camera().transform.rotation;
        }

        public void UpdateCamera()
        {
            float s = 1.0f;
            float theta = 0.0f;
            float phi = 0.0f;

            //            if (Input.GetMouseButton(1))
            {
                theta = s * Input.GetAxis("Horizontal");
                phi = s * Input.GetAxis("Vertical") * -1.0f;
            }
            mouseAccel += new Vector3(theta, phi, 0);
            /*            focusPointCur += (focusPoint - focusPointCur) * 0.05f;
                        focusPointCurStar += (selectedSystem.position - focusPointCurStar) * 0.05f;
                        */
            float t = 0.1f;
            focusPointCur = (focusPoint * t + focusPointCur * (1 - t));

            //focusPointCurStar = (selectedSystem.position * t + focusPointCurStar * (1 - t));

            mouseAccel *= 0.65f;

            euler += mouseAccel * 10f;
            //            Debug.Log(theta);
            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.up, mouseAccel.x);


            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) > 0.99)
                if (mouseAccel.y < 0)
                    mouseAccel.y = 0;
            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) < -0.99)
                if (mouseAccel.y > 0)
                    mouseAccel.y = 0;


            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.right, mouseAccel.y);

            mainCamera.transform.LookAt(focusPointCur);

            scrollWheelAccel = Input.GetAxis("Mouse ScrollWheel") * 0.5f * -1;
            scrollWheel = scrollWheel * 0.9f + scrollWheelAccel * 0.1f;
            if (Mathf.Abs(scrollWheel) < 0.001)
                scrollWheel = 0;

        }





    }

}