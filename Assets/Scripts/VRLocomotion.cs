using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class VRLocomotion : MonoBehaviour
{
    [Header("Teleport")]
    public Transform xrRig;
    public string handness = "Right";

    [Header("Curved Line")]
    public float curveHeight = 1.5f;
    public int lineResolution = 20;

    [Header("Smooth Reticle")]
    public Transform teleportReticle;
    public float smoothnessValue = 0.2f;

    [Header("Fade Screen")]
    public RawImage blackScreen;

    // Internal Vars
    private LineRenderer lr;
    public bool teleportLock;

    // Start is called before the first frame update
    void Start()
    {
        InitializeLineRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRaycast();
        Rotate();
    }

    private void InitializeLineRenderer()
    {
        // Get Line Renderer
        lr = GetComponent<LineRenderer>();

        // Turn it off
        lr.enabled = false;

        // Set the total number of points in the line
        lr.positionCount = lineResolution;

    }

    private void HandleRaycast()
    {
        // Create RayCast
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo = new RaycastHit();

        // If hit the raycast returns true
        if (Physics.Raycast(ray, out hitInfo))
        {
            // Turn on Line Renderer and Reticle if we hit somthing
            lr.enabled = true;
            teleportReticle.gameObject.SetActive(true);

            // Check to see if hit valid target
            bool validTarget = hitInfo.collider.tag == "Ground";

            // Set color to blue if valid target, gray otherwise
            Color lrColor = validTarget ? Color.cyan : Color.gray;

            lr.startColor = lrColor;
            lr.endColor = lrColor;

            // Set the Start and desired position for the line renderer
            Vector3 startPoint = transform.position;
            Vector3 desiredPoint = hitInfo.point;


            // Smooth out the end position
            Vector3 vecToDesired = desiredPoint - teleportReticle.position;
            Vector3 smootherVecToDesired = (vecToDesired / smoothnessValue) * Time.deltaTime;
            Vector3 endPoint = teleportReticle.position + smootherVecToDesired;

            // Position the teleport reticle to the smooth end point
            teleportReticle.position = endPoint;

            // Find the mid point for the curved line
            Vector3 vecFromStartToEnd = endPoint - startPoint;
            Vector3 halfVecFromStartToEnd = vecFromStartToEnd / 2f;

            Vector3 midPoint = startPoint + halfVecFromStartToEnd;
            midPoint.y += curveHeight;

            // Set all the curved line positions
            for (int i = 0; i < lineResolution; i++)
            {
                float t = i / (float)lineResolution;
                Vector3 startToMid = Vector3.Lerp(startPoint, midPoint, t);
                Vector3 midToEnd = Vector3.Lerp(midPoint, endPoint, t);
                Vector3 curvePos = Vector3.Lerp(startToMid, midToEnd, t);

                lr.SetPosition(i, curvePos);
            }


            // Old Straight line set positions
            //lr.SetPosition(0, transform.position);
            //lr.SetPosition(1, hitInfo.point);

            // Check for input if is a valid target
            if (teleportLock == false && validTarget == true && Input.GetButtonDown(handness + "_Trigger"))
            {
                // Use coroutine to teleport user
                StartCoroutine(FadeTeleport(hitInfo.point));
                //xrRig.position = hitInfo.point;
            }
        }
        // Code comes here if the raycast did not hit anythihng
        else
        {
            lr.enabled = false;
            teleportReticle.gameObject.SetActive(false);
        }
    }

    private void Rotate()
    {
        // Check if button is clicked
        if (Input.GetButtonDown(handness + "_StickClick"))
        {
            // Determine rotation direction
            float rot = Input.GetAxis(handness + "_Joystick") > 0 ? 30 : -30;

            // Rotate user
            xrRig.transform.Rotate(0, rot, 0);
        }
    }

    private IEnumerator FadeTeleport(Vector3 pos)
    {
        // Make sure teleport can't be called again
        teleportLock = true;

        // Reset Time counter variable
        float currentTime = 0;

        // Loop until counter is done
        while (currentTime < 1)
        {
            // Fade out screen
            blackScreen.color = Color.Lerp(Color.clear, Color.black, currentTime);
            
            // Wait one frame
            yield return null;
           
            // Increment Timer
            currentTime += Time.deltaTime;
        }

        // Set full black screen
        blackScreen.color = Color.black;
        
        // Move user
        xrRig.transform.position = pos;
       // xrRig.LookAt(someCoolObjectsTransform, Vector3.up);
        
        // Wait one second
        yield return new WaitForSeconds(1);
       
        // Reset timer again
        currentTime = 0;
        
        // Loop until timer is done again
        while (currentTime < 1)
        {
            // Fade in screen
            blackScreen.color = Color.Lerp(Color.black, Color.clear, currentTime);

            // Wait one frame
            yield return null;
            
            // Increment Timer
            currentTime += Time.deltaTime;
        }
        
        // Allow teleporting again
        teleportLock = false;
    }
}
