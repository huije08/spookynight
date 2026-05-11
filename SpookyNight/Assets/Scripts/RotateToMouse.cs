using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour
{

    [SerializeField] private float CamXAxisSpeed = 5;
    [SerializeField] private float CamYAxisSpeed = 3;

    private float limitMinX = -80;
    private float limitMaxX = 50;

    private float eluerAngleX;
    private float eluerAngleY;

    public void UpdateRotate(float mouseX, float mouseY)
    {
        eluerAngleX -= mouseY * CamXAxisSpeed;
        eluerAngleY += mouseX * CamYAxisSpeed;

        eluerAngleX = ClampAngle(eluerAngleX, limitMinX, limitMaxX);
        transform.rotation = Quaternion.Euler(eluerAngleX, eluerAngleY, 0);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle<-360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }

        return Mathf.Clamp(angle, min, max);
    }
}
