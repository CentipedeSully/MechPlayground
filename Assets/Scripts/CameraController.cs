using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    //Declarations
    [Header("References")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Camera _playerCamera;

    [Header("Camera Settings")]
    [SerializeField] private bool _isCamControlEnabled = false;
    [SerializeField] private float _turnSpeed = 50f; 
    [SerializeField] private float _pitchSpeed = 50f;
    [SerializeField] private float _minPitch = -60;
    [SerializeField] private float _maxPitch = 60;
    [SerializeField] private bool _invertY = true;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector3 _relativeForwardDirection;
    [SerializeField] private Vector3 _relativeStrafeDirection;

    [Header("Debug")]
    [SerializeField] private bool _areControlsConnected = false;
    [SerializeField] private Vector2 _detectedMoveInput;
    [SerializeField] private Vector2 _detectedCameraInput;


    private InputAction _moveAction;
    private InputAction _cameraAction;
    private string _movementActionName = "Movement";
    private string _cameraActionName = "Camera";
    private float _pitchDistance;



    //Monobehaviours
    private void Awake()
    {
        ConnectToInputAsset();
    }

    private void Start()
    {
        EnableCameraAfterDelay(.2f);
    }

    private void Update()
    {
        ReadInputs();

        if (_isCamControlEnabled)
        {
            RotateCamera();
            MoveCamera();
        }
            
    }



    //Internals
    private void ConnectToInputAsset()
    {
        if (_playerInput != null)
        {
            //connect the actions this way to easily watch for changes with them each frame
            _moveAction = _playerInput.actions.FindAction(_movementActionName);
            _cameraAction = _playerInput.actions.FindAction(_cameraActionName);

            //validate the connection. Raise a flag if there's an error
            if (_moveAction != null && _cameraAction != null)
                _areControlsConnected = true;
            else
            {
                if (_cameraAction == null)
                    Debug.LogError($"Action '{_cameraAction}' wasn't found among the input asset's actions");
                if (_moveAction == null)
                    Debug.LogError($"Action '{_movementActionName}' wasn't found among the input asset's actions");

                _areControlsConnected = false;
            }
        }
    }

    private void ReadInputs()
    {
        if (_areControlsConnected)
        {
            //read the current movement input status
            _detectedMoveInput = _moveAction.ReadValue<Vector2>();

            //read the current camera input status
            _detectedCameraInput = _cameraAction.ReadValue<Vector2>();

            //invert y
            if (_invertY)
                _detectedCameraInput.y *= -1;

        }
    }

    private void RotateCamera()
    {
        if (_detectedCameraInput.x != 0)
        {
            Vector3 rotationOffset = Vector3.up * _detectedCameraInput.x * _turnSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotationOffset);
        }

        if (_detectedCameraInput.y != 0)
        {
            //fun fact:
            //Positive pitch positions our face more down,
            //and negative pitch positions our face more up

            if ((_pitchDistance < _maxPitch && _detectedCameraInput.y > 0) ||   //Are we attempting to pivot DOWN while remaining in range
                (_pitchDistance > _minPitch && _detectedCameraInput.y < 0))    //Are we attempting to pivot UP while remaining in range
            {
                //update the pitch's current distance from the origin of where we started (0)
                _pitchDistance += _detectedCameraInput.y * _pitchSpeed * Time.deltaTime;

                //apply the camera's relevant displacement
                Vector3 rotationOffset = Vector3.right * _detectedCameraInput.y * _pitchSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotationOffset);
            }

        }

    }

    private void EnableCameraAfterDelay(float delay)
    {
        Invoke(nameof(EnableCamera), delay);
    }
    private void EnableCamera()
    {
        _isCamControlEnabled = true;
    }

    private void MoveCamera()
    {
        //move forwards/backwards
        if (_detectedMoveInput.y != 0)
        {
            //Calculate our desired forwards direction, relative to the camera
            _relativeForwardDirection = _playerCamera.transform.TransformDirection(Vector3.forward);

            //ignore the height dimension
            _relativeForwardDirection.y = 0;
            _relativeForwardDirection = _relativeForwardDirection.normalized;

            transform.position += _detectedMoveInput.y * _moveSpeed * Time.deltaTime * _relativeForwardDirection;
        }

        if (_detectedMoveInput.x != 0)
        {
            //Calculate our desired Right direction, relative to the camera (to capture the strafing axis)
            _relativeStrafeDirection = _playerCamera.transform.TransformDirection(Vector3.right);

            //ignore the height dimension
            _relativeStrafeDirection.y = 0;
            _relativeStrafeDirection = _relativeStrafeDirection.normalized;

            transform.position +=  _detectedMoveInput.x * _moveSpeed * Time.deltaTime * _relativeStrafeDirection;
        }

    }



    //Externals
    public void SetCamerControl(bool enableCamera)
    {
        _isCamControlEnabled = enableCamera;
    }




}
