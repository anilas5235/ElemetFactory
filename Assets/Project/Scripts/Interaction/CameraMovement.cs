using Project.Scripts.General;
using UnityEngine;

namespace Project.Scripts.Interaction
{
    public class CameraMovement : Singleton<CameraMovement>
    {
        public Camera Camera { get; private set; }

        [Header("Parameters")]
        [SerializeField] private float moveSpeed =5;
        [SerializeField] private float heightenedSpeed = 10;

        [SerializeField] private float zoomSpeed =2;
        [SerializeField] private float minCamSize =2;
        [SerializeField] private float maxCamSize =20;

        protected override void Awake()
        {
            base.Awake();
            Camera = GetComponent<Camera>();
        }

        private void Update()
        {
            Camera.orthographicSize += -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            if (Camera.orthographicSize < minCamSize) Camera.orthographicSize = minCamSize;
            else if (Camera.orthographicSize > maxCamSize) Camera.orthographicSize = maxCamSize;

            float speed = Input.GetKey(KeyCode.LeftShift) ? heightenedSpeed : moveSpeed;
            transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) *
                                  (Time.deltaTime * speed * Camera.orthographicSize/maxCamSize);
        }
    }
}
