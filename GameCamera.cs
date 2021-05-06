using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private new Camera camera; // Add 'new' keyword to avoid 'warning' cause declaring a variable with the same name as a variable in a base class

    public float scrollMovement;
    public float scrollSize = 0;

    private float scrollStart;

    void Awake() 
    {
        camera = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        scrollStart = camera.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float scrollMovement = Input.GetAxisRaw("Mouse ScrollWheel"); // Axis raw, no smoothing applied

        float currentScroll = camera.transform.position.y;
        Vector3 direction;
        if (scrollMovement > 0 && currentScroll < scrollStart)
        {
            direction = Vector3.up;
        }
        else if (scrollMovement < 0 && currentScroll > (scrollStart - scrollSize)) // Scroll end
        {
            direction = Vector3.down;
        }
        else {
            // Do Nothing
            direction = Vector3.zero;
        }

        camera.transform.Translate((direction * this.scrollMovement) * Time.deltaTime);
    }
}
