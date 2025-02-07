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
    [SerializeField] private Transform _playerObject;
    [SerializeField] private Transform _modelObject;

    [Header("Camera Settings")]
    [SerializeField] private bool _isCamControlEnabled = false;
    [SerializeField] private float _turnSpeed = 50f; 
    [SerializeField] private float _pitchSpeed = 50f;
    [SerializeField] private float _minPitch = -60;
    [SerializeField] private float _maxPitch = 60;
    [SerializeField] private bool _invertY = true;

    [Header("Movement Settings")]
    [SerializeField] private float _bodyMoveSpeed;
    [SerializeField] private float _bodyTurnSpeed;
    [SerializeField] private float _turnDegreeTolerance = .05f;
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
            MoveBody();
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

    private void MoveBody()
    {
        if (_detectedMoveInput.magnitude != 0)
        {
            //Calculate our desired forwards direction, relative to the camera
            _relativeForwardDirection = _playerCamera.transform.TransformDirection(Vector3.forward);

            //rotate the body to face forwards
            RotateBodyForwardsTowardsTargetDirection(_relativeForwardDirection);
        }

        //move forwards/backwards
        if (_detectedMoveInput.y != 0)
        {
            //Calculate our desired forwards direction, relative to the camera
            _relativeForwardDirection = _playerCamera.transform.TransformDirection(Vector3.forward);

            //ignore the height dimension
            _relativeForwardDirection.y = 0;
            _relativeForwardDirection = _relativeForwardDirection.normalized;

            //move in the proper axis
            _playerObject.position += _detectedMoveInput.y * _bodyMoveSpeed * Time.deltaTime * _relativeForwardDirection;
        }

        if (_detectedMoveInput.x != 0)
        {
            //Calculate our desired Right direction, relative to the camera (to capture the strafing axis)
            _relativeStrafeDirection = _playerCamera.transform.TransformDirection(Vector3.right);

            //ignore the height dimension
            _relativeStrafeDirection.y = 0;
            _relativeStrafeDirection = _relativeStrafeDirection.normalized;

            //move in the proper axis
            _playerObject.position +=  _detectedMoveInput.x * _bodyMoveSpeed * Time.deltaTime * _relativeStrafeDirection;

        }

    }

    private void RotateBodyForwardsTowardsTargetDirection(Vector3 cameraForwards)
    {
        Vector3 currentBodyFowards = _modelObject.TransformDirection(Vector3.forward);

        float signedDifference = Vector3.SignedAngle(currentBodyFowards, cameraForwards, Vector3.up);
        //Debug.Log($"Difference from body forwards to camera forwards: {signedDifference}");

       
        if ( signedDifference > _turnDegreeTolerance)
        {
            Vector3 currentRotation = _modelObject.rotation.eulerAngles;
            float rotationalModifier = _bodyTurnSpeed * Time.deltaTime;
            Debug.Log($"Rotational Modifier: {rotationalModifier}");
            Vector3 newRotation = currentRotation + new Vector3(0, rotationalModifier, 0);
            _modelObject.rotation = Quaternion.Euler(newRotation);
        }

        else if (signedDifference < -_turnDegreeTolerance)
        {
            Vector3 currentRotation = _modelObject.rotation.eulerAngles;
            float rotationalModifier = -_bodyTurnSpeed * Time.deltaTime;
            Debug.Log($"Rotational Modifier: {rotationalModifier}");
            Vector3 newRotation = currentRotation + new Vector3(0,+ rotationalModifier, 0);
            _modelObject.rotation = Quaternion.Euler(newRotation);
        }

    }




    //Externals
    public void SetCamerControl(bool enableCamera)
    {
        _isCamControlEnabled = enableCamera;
    }




}
