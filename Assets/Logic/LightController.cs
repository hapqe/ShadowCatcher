using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField]
    float rotationMaximum = 80.0f;

    Quaternion startRotation;
    // Start is called before the first frame update
    void Start()
    {
        startRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        var mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        mousePos.x = mousePos.x / (Screen.width / 2.0f) - 1.0f;
        mousePos.y = mousePos.y / (Screen.height / 2.0f) - 1.0f;

        var max = rotationMaximum / 4.0f;
        var x = mousePos.y * max;
        x = Mathf.Clamp(x, -max, max);
        var y = mousePos.x * max;
        y = Mathf.Clamp(y, -max, max);
        transform.rotation = Quaternion.Euler(new Vector3(x, y, 0)) * startRotation;
    }
}
