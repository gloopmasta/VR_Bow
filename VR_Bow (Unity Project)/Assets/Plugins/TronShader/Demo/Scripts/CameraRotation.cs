using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRotation : MonoBehaviour
{    
    float camPosX;
    float camPosY;
    float camPosZ;
    float camRotationX;
    float camRotationY;
    float camRotationZ;
    float turnSpeed = 1;
    Vector3 offset = new Vector3(0,0,0);
    Vector3 abovePlayer = new Vector3(25,25,25);
    
    public Transform player;
    private void Start()
    {
        camPosX = transform.position.x;
        camPosY = transform.position.y;
        camPosZ = transform.position.z;
        camRotationX = transform.rotation.x;
        camRotationY = transform.rotation.y;
        camRotationZ = transform.rotation.z;
        offset = new Vector3(player.position.x + camPosX, player.position.y + camPosY, player.position.z + camPosZ);
        transform.rotation = Quaternion.Euler(camRotationX, camRotationY, camRotationZ);
    }
    
    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(!IsPointerOverUIObject())
            {
                abovePlayer = new Vector3(player.position.x, player.position.y + 1, player.position.z);
                offset = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * turnSpeed, Vector3.down) * Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * turnSpeed, Vector3.right) * offset;
                transform.position = player.position + offset;
                transform.LookAt(abovePlayer);
            }
        }
    }


    // unity ispointerovergameobject seems to have an issue with unity versione 2019.3. I used this function instead i found on stackoverflow
    // https://stackoverflow.com/questions/57010713/unity-ispointerovergameobject-issue/58545725#58545725
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.layer == 5) //5 = UI layer
            {
                return true;
            }
        }

        return false;
    }
}
