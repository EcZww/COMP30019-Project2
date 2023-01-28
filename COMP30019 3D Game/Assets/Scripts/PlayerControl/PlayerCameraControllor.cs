using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCameraControllor : MonoBehaviour
{
    public new Camera camera;
    private Transform characterTransform;
    private Transform cameraTransform;
    private Rigidbody myRigidBody;
    private Vector3 offset;
    private Vector3 noCollisionCameraPosition;
    RaycastHit hit;

    public enum RotationAxes {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }
 
    public RotationAxes m_axes = RotationAxes.MouseXAndY;
    public float rotateSpeedX = 2f;
    public float rotateSpeedY = 1.5f;
 
    public float m_minimumY = -25f;
    public float m_maximumY = 45f;
 
    float m_rotationY = 0f;
    


    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = this.GetComponent<Rigidbody>();
        characterTransform = this.GetComponent<Transform>();
        offset = characterTransform.position;
        noCollisionCameraPosition = camera.transform.position;
    }


    void LateUpdate() {
        if (!InventoryManager.Instance.isOpen) CameraControl();
    }

    void CameraControl() {
        if (noCollisionCameraPosition != null) camera.transform.position = noCollisionCameraPosition;
        camera.transform.position += this.transform.position - offset;
        offset = this.transform.position;
        float mouseX = Input.GetAxis("Mouse X") * rotateSpeedX;
        camera.transform.RotateAround(this.transform.position, Vector3.up, mouseX);
        this.transform.RotateAround(this.transform.position, Vector3.up, mouseX);
        m_rotationY += Input.GetAxis ("Mouse Y") * rotateSpeedY;
        m_rotationY = Mathf.Clamp (m_rotationY, m_minimumY, m_maximumY);
        camera.transform.localEulerAngles = new Vector3 (-m_rotationY, camera.transform.localEulerAngles.y, 0);
        noCollisionCameraPosition = camera.transform.position;
        if (Physics.Linecast (this.transform.position, camera.transform.position, out hit)) {
            if (hit.collider.gameObject.CompareTag("Terrain")) {
                Vector3 direction = (hit.point - camera.transform.position).normalized;
                camera.transform.position = hit.point + direction * 0.5f;
            }
        }
    }
    
}
