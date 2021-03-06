using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerLocomotion : MonoBehaviour
{
    CameraHandler cameraHandler;
    PlayerManager playerManager;
    Transform cameraObject;
    InputHandler inputHandler;
    PlayerStats playerStats;
    public new Rigidbody rigidbody;
    public GameObject normalCamera;
    public Vector3 moveDirection, normalVector, targetPosition;

    [HideInInspector]
    public Transform myTransform;
    [HideInInspector]
    public PlayerAnimatorManager playerAnimatorManager;

    [Header("Ground & Air Detection Stats")]
    [SerializeField]
    float groundDetectionRayStartPoint = 0.6f;
    [SerializeField]
    float minimumDistanceNeededToBeginFall = 1.5f;
    [SerializeField]
    float groundDirectionRayDistance = 0.2f;
    LayerMask ignoreForGroundCheck;
    public float inAirTimer;

    [Header("Movement Stats")]
    [SerializeField]
    float movementSpeed = 5;
    [SerializeField]
    float walkingSpeed = 1;
    [SerializeField]
    float sprintSpeed = 7;
    [SerializeField]
    float rotationSpeed = 10;
    [SerializeField]
    float fallingSpeed = 250;
    [SerializeField]
    float jumpHeight = 3;
    [SerializeField]
    float gravityIntensity = -15;
    [SerializeField]
    float pushoffStrength = 10f;


    public CapsuleCollider characterCollider;
    public CapsuleCollider characterCollisionBlockerCollider;

    private void Awake()
    {
        cameraHandler = FindObjectOfType<CameraHandler>();
        cameraObject = Camera.main.transform;

    }

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        rigidbody = GetComponent<Rigidbody>();
        playerStats = GetComponent<PlayerStats>();
        inputHandler = GetComponent<InputHandler>();
        playerAnimatorManager = GetComponentInChildren<PlayerAnimatorManager>();
        myTransform = transform;
        playerAnimatorManager.Initialize();
        playerManager.isGrounded = true;
        ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        Physics.IgnoreCollision(characterCollider, characterCollisionBlockerCollider, true);


    }


    #region Movement

    private void HandleRotation(float delta)
    {
        if (inputHandler.lockOnFlag)
        {
            if (inputHandler.sprintFlag || inputHandler.rollFlag)
            {
                Vector3 targetDirection = Vector3.zero;
                targetDirection = cameraHandler.cameraTransform.forward * inputHandler.vertical;
                targetDirection += cameraHandler.cameraTransform.right * inputHandler.horizontal;
                targetDirection.Normalize();
                targetDirection.y = 0;

                if (targetDirection == Vector3.zero)
                {
                    targetDirection = transform.forward;
                }

                Quaternion tr = Quaternion.LookRotation(targetDirection);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * Time.deltaTime);

                transform.rotation = targetRotation;
            }
            else
            {
                Vector3 rotationDirection = moveDirection;

                if(cameraHandler.currentLockOnTarget != null)
                {
                    rotationDirection = cameraHandler.currentLockOnTarget.transform.position - transform.position;
                }
                rotationDirection.y = 0;
                rotationDirection.Normalize();
                Quaternion tr = Quaternion.LookRotation(rotationDirection);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * Time.deltaTime);
                transform.rotation = targetRotation;
            }
        }
        else
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = inputHandler.moveAmount;

            targetDir = cameraObject.forward * inputHandler.vertical;
            targetDir += cameraObject.right * inputHandler.horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward;

            float rs = rotationSpeed;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }
    }

    public void HandleMovement(float delta)
    {
        if (inputHandler.rollFlag)
            return;

        if (playerManager.isInteracting)
            return;

        moveDirection = cameraObject.forward * inputHandler.vertical;
        moveDirection += cameraObject.right * inputHandler.horizontal;
        moveDirection.Normalize();
        moveDirection.y = 0;

        float speed = movementSpeed;

        if (playerStats.currentStamina >0 && inputHandler.sprintFlag && inputHandler.moveAmount > 0.5)
        {
            speed = sprintSpeed;
            playerManager.isSprinting = true;
            moveDirection *= speed;
            playerStats.TakeStaminaDamage(0.25f);
        }
        else
        {
            if (inputHandler.moveAmount < 0.5)
            {
                moveDirection *= walkingSpeed;
                playerManager.isSprinting = false;
            }
            else
            {
                moveDirection *= speed;
                playerManager.isSprinting = false;
            }
        }

        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
        rigidbody.velocity = projectedVelocity;

        playerAnimatorManager.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting);

        if (playerAnimatorManager.canRotate)
        {
            HandleRotation(delta);
        }
    }

    public void HandleRollingAndSprinting(float delta)
    {
        if (playerAnimatorManager.anim.GetBool("isInteracting"))
            return;

        if (inputHandler.rollFlag)
        {
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;

            if (inputHandler.moveAmount > 0)
            {
                playerAnimatorManager.PlayTargetAnimation("Rolling", true);
                moveDirection.y = 0;
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                myTransform.rotation = rollRotation;
            }
            else
            {
                playerAnimatorManager.PlayTargetAnimation("Backstep", true);
            }
        }
    }

    public void HandleFalling(float delta, Vector3 moveDirection)
    {
        playerManager.isGrounded = false;
        RaycastHit hit;
        Vector3 origin = myTransform.position;
        origin.y += groundDetectionRayStartPoint;

        if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
        {
            moveDirection = Vector3.zero;
        }

        if (playerManager.isInAir)
        {
            //Adds a push when you fall so you dont get stuck on the ledge
            rigidbody.AddForce(-Vector3.up * fallingSpeed);
            rigidbody.AddForce(moveDirection * fallingSpeed / pushoffStrength);
        }

        Vector3 dir = moveDirection;
        dir.Normalize();
        origin = origin + dir * groundDirectionRayDistance;

        targetPosition = myTransform.position;

        //        Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);
        if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
        {
            normalVector = hit.normal;
            Vector3 tp = hit.point;
            playerManager.isGrounded = true;
            targetPosition.y = tp.y;

            if (playerManager.isInAir)
            {
                if (inAirTimer > 0.5f)
                {
                    //Debug.Log("You were in the air for " + inAirTimer);
                    playerAnimatorManager.PlayTargetAnimation("Land", true);
                    inAirTimer = 0;
                }
                else
                {
                    playerAnimatorManager.PlayTargetAnimation("Movement", false);
                    inAirTimer = 0;
                }

                playerManager.isInAir = false;
            }
        }
        else
        {
            if (playerManager.isGrounded)
            {
                playerManager.isGrounded = false;
            }

            if (playerManager.isInAir == false)
            {
                if (playerManager.isInteracting == false)
                {
                    playerAnimatorManager.PlayTargetAnimation("Falling", true);
                }

                Vector3 vel = rigidbody.velocity;
                vel.Normalize();
              //  rigidbody.velocity = vel * (movementSpeed / 2);
                playerManager.isInAir = true;
            }
        }

        if (playerManager.isGrounded)
        {
            if (playerManager.isInteracting || inputHandler.moveAmount > 0)
            {
                myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime / 0.1f);
            }
            else
            {
                myTransform.position = targetPosition;
            }
        }

    }

    public void HandleJumping()
    {
        if (playerManager.isGrounded)
        {
            playerAnimatorManager.anim.SetBool("isJumping", true);
            playerAnimatorManager.PlayTargetAnimation("Jump", false);

            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            rigidbody.velocity = playerVelocity;
        }

    }
    #endregion

}