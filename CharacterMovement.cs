using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    #region Movement
    
    [Header("Movement settings")]
    [SerializeField] private float _rollTorque = 1000f;
    [SerializeField] private float _thrust = 100f;
    [SerializeField] private float _upThrust = 50f;
    [SerializeField] private float _strafeThrust = 50f;
    
    #endregion

    #region Boost

    [SerializeField]
    private float _currentBoostAmount;
    [SerializeField]
    private bool _boosting = false;
    
    [Header("Boost settings")]
    [SerializeField] private float _maxBoostAmount = 2f;
    [SerializeField] private float _boostDeprecationRate = 0.25f;
    [SerializeField] private float _boostRechargeRate = 0.5f;
    [SerializeField] private float _boostMultiplier = 5f;

    [SerializeField, Range(0.001f, 0.999f)]
    private float _thrustGlideReduction = 0.999f;

    [SerializeField, Range(0.001f, 0.999f)]
    private float _upDownGlideReduction = 0.111f;

    [SerializeField, Range(0.001f, 0.999f)]
    private float _leftRightGlideReduction = 0.111f;

    private float _glide = 0f;
    private float _verticalGlide = 0f;
    private float _horisontalGlide = 0f;
    
    #endregion

    [SerializeField] private  InputActionAsset _inputActionAsset = default;
    [SerializeField] private Camera _playerCamera = default;
    
    private Rigidbody _rigidbody = default;

    private float _thrust1D;
    private float _upDown1D;
    private float _strafe1D;
    private float _roll1D;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _playerCamera = Camera.main;
        _currentBoostAmount = _maxBoostAmount;

        _inputActionAsset.FindAction("Thrust").performed += OnThrust;
        _inputActionAsset.FindAction("Strafe").performed += OnStrafe;
        _inputActionAsset.FindAction("UpDown").performed += OnUpDown;
        _inputActionAsset.FindAction("Roll").performed += OnRoll;
        _inputActionAsset.FindAction("Boost").performed += OnBoost;
        _inputActionAsset.Enable();
    }

    private void OnDestroy()
    {
        _inputActionAsset.Disable();
        _inputActionAsset.FindAction("Thrust").started -= OnThrust;
        _inputActionAsset.FindAction("Strafe").started -= OnStrafe;
        _inputActionAsset.FindAction("UpDown").started -= OnUpDown;
        _inputActionAsset.FindAction("Roll").started -= OnRoll;
        _inputActionAsset.FindAction("Boost").started -= OnBoost;
    }

    private void FixedUpdate()
    {
        HandleBoosting();
        HandleMovement();
    }

    private void HandleBoosting()
    {
        if (_boosting && _currentBoostAmount > 0f)
        {
            _currentBoostAmount -= _boostDeprecationRate;
            if (_currentBoostAmount <= 0f)
            {
                _boosting = false;
            }
        }
        else
        {
            if (_currentBoostAmount < _maxBoostAmount)
            {
                _currentBoostAmount += _boostRechargeRate;
            }
        }
    }
    
    private void HandleMovement()
    {
        //Roll
        _rigidbody.AddTorque(-_playerCamera.transform.forward * _roll1D * _rollTorque * Time.deltaTime);

        //Trust
        if (_thrust1D > 0.1f || _thrust1D < -0.1f)
        {
            float currentThrust;

            if (_boosting)
            {
                currentThrust = _thrust * _boostMultiplier;
            }
            else
            {
                currentThrust = _thrust;
            }
            
            _rigidbody.AddForce(_playerCamera.transform.forward * _thrust1D * currentThrust * Time.deltaTime);
            _glide = _thrust;
        }
        else
        {
            _rigidbody.AddForce(_playerCamera.transform.forward * _glide * Time.deltaTime);
            _glide *= _thrustGlideReduction;
        }

        //Up/Down
        if (_upDown1D > 0.1f || _upDown1D < -0.1f)
        {
            _rigidbody.AddRelativeForce(Vector3.up * _upDown1D * _upThrust * Time.fixedDeltaTime);
            _verticalGlide *= _upDown1D * _upThrust;
        }
        else
        {
            _rigidbody.AddRelativeForce(Vector3.up * _verticalGlide * Time.fixedDeltaTime);
            _verticalGlide *= _upDownGlideReduction;
        }

        // Strafing
        if (_strafe1D > 0.1f || _strafe1D < -0.1f)
        {
            _rigidbody.AddForce(_playerCamera.transform.right * _strafe1D * _upThrust * Time.fixedDeltaTime);
            _horisontalGlide = _strafe1D * _strafeThrust;
        }
        else
        {
            _rigidbody.AddForce(_playerCamera.transform.right  * _horisontalGlide * Time.fixedDeltaTime);
            _horisontalGlide *= _leftRightGlideReduction;
        }
    }

    #region Input

    private void OnThrust(InputAction.CallbackContext context) => _thrust1D = context.ReadValue<float>();

    private void OnStrafe(InputAction.CallbackContext context) => _strafe1D = context.ReadValue<float>();

    private void OnUpDown(InputAction.CallbackContext context) => _upDown1D = context.ReadValue<float>();

    private void OnRoll(InputAction.CallbackContext context) => _roll1D = context.ReadValue<float>();

    private void OnBoost(InputAction.CallbackContext context) => _boosting = context.performed;

    #endregion

}
