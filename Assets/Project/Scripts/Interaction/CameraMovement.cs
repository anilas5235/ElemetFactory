using UnityEngine;

namespace Project.Scripts.Interaction
{
    public class CameraMovement : MonoBehaviour
    {
        private Camera _camera;

        [Header("Parameters")]
        [SerializeField] private float moveSpeed =5;
        [SerializeField] private float heightenedSpeed = 10;

        [SerializeField] private float zoomSpeed =2;
        [SerializeField] private float minCamSize =2;
        [SerializeField] private float maCamSize =20;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            _camera.orthographicSize += -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            if (_camera.orthographicSize < minCamSize) _camera.orthographicSize = minCamSize;
            else if (_camera.orthographicSize > maCamSize) _camera.orthographicSize = maCamSize;

            float speed = Input.GetKey(KeyCode.LeftShift) ? heightenedSpeed : moveSpeed;
            transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) *
                                  (Time.deltaTime * speed * _camera.orthographicSize/maCamSize);
        }
    }
}
