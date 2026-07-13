using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    public float displayTime = 0.06f;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.enabled = false;

        if (lr.material == null)
            lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void Show(Vector3 start, Vector3 end, bool isCharged)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayLaser(start, end, isCharged));
    }

    IEnumerator DisplayLaser(Vector3 start, Vector3 end, bool isCharged)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        if (isCharged)
        {
            lr.startWidth = 0.25f;
            lr.endWidth   = 0.15f;
            lr.startColor = Color.red;
            lr.endColor   = new Color(1f, 0.3f, 0f);
        }
        else
        {
            lr.startWidth = 0.04f;
            lr.endWidth   = 0.02f;
            lr.startColor = Color.green;
            lr.endColor   = new Color(0f, 1f, 0.5f);
        }

        lr.enabled = true;
        yield return new WaitForSeconds(displayTime);
        lr.enabled = false;
    }
}
