using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravityMultiplier;
    [SerializeField] private float jumpButtonGracePeriod;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float jumpHorizontalSpeed;
    [SerializeField] private float speed;


    private Animator _animator;
    private CharacterController _characterController;
    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    private bool isJumping;
    private bool isGrounded;


    void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        originalStepOffset = _characterController.stepOffset;
    }


    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            inputMagnitude /= 2;
        }

        _animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);

        //float speed = inputMagnitude * maximumSpeed;
        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) *
                            movementDirection;
        movementDirection.Normalize();
        float gravity = Physics.gravity.y * gravityMultiplier;
        if (isJumping && ySpeed>0 && Input.GetButton("Jump")==false)
        {
            gravity *= 2;
        }
        ySpeed += gravity * Time.deltaTime;
        if (_characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpButtonPressedTime = Time.time;
        }

        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            _characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;
            _animator.SetBool("isGrounded", true);
            isGrounded = true;
            _animator.SetBool("isJumping", false);
            isJumping = false;
            _animator.SetBool("isFalling", false);

            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = Mathf.Sqrt(jumpHeight*-3*gravity);
                _animator.SetBool("isJumping", true);
                isJumping = true;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
        }
        else
        {
            _characterController.stepOffset = 0;
            _animator.SetBool("isGrounded", false);
            isGrounded = false;
            if ((isJumping && ySpeed < 0) || ySpeed < -2)
            {
                _animator.SetBool("isFalling", true);
            }
        }


        if (movementDirection != Vector3.zero)
        {
            _animator.SetBool("isMoving", true);

            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }

        if (isGrounded == false)
        {
            Vector3 velocity = movementDirection * (inputMagnitude * jumpHorizontalSpeed);
            velocity.y = ySpeed;
            _characterController.Move(velocity * Time.deltaTime);
        }
    }

    private void OnAnimatorMove()
    {
        if (isGrounded)
        {
            Vector3 velocity = _animator.deltaPosition;
            velocity.y = ySpeed * Time.deltaTime;

            _characterController.Move(velocity);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}