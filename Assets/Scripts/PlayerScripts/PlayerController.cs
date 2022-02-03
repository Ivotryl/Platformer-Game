using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [Header("----- References -----", order = 0), Space]
    [SerializeField] private InputReader playerInputs = default;
    [SerializeField] private FrameInputs inputs;
    private struct FrameInputs {
        public float horizontal, vertical;
        public int horizontalRaw, verticalRaw;
    }
    [SerializeField] private LayerMask groundLayer;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Animator animator;
    [SerializeField] private ParticleSystem runningDustPS;
    [SerializeField] private ParticleSystem normalJumpDustPS;
    [SerializeField] private ParticleSystem multipleJumpDustPS;
    [SerializeField] private ParticleSystem wallJumpDustPS;
    [SerializeField] private ParticleSystem wallSlideDustPS;
    [SerializeField] private ParticleSystem landingDustPS;

    //[Header("----- Game Events -----", order = 0)]
  
    [Header("----- Ground Detection -----", order = 0)]
    private readonly Collider2D[] ground = new Collider2D[1];
    private readonly Collider2D[] leftWall = new Collider2D[1];
    private readonly Collider2D[] rightWall = new Collider2D[1];

    [Header("----- Inputs -----", order = 0), Space]
    [SerializeField] private Vector2 movementDirection;
    [SerializeField] private float lastHorizontalDirection = 1.0f;
    [SerializeField] private float jumpingInput;

    [Header("----- Parameters -----", order = 0), Space]
    [Header("----- Enablers -----", order = 1)]
    [SerializeField] private bool canWalk;
    [SerializeField] private bool canTurn;
    [SerializeField] private bool enableSmoothHorizontalMovement = true;
    [SerializeField] private bool enableInstantFall = true;
    [SerializeField] private bool enableVariableJump = true;
    [SerializeField] private bool enableDirectionalJump = true;
    [SerializeField] private bool enableMultipleJump = true;
    [SerializeField] private bool enableWallJump = true;
    [SerializeField] private bool enableWallSliding = true;
    [SerializeField] private bool enableDash = true;

    [Header("----- Horizontal Movement -----", order = 1)]
    [SerializeField, Range(0.0f, 30.0f)] private float moveSpeed;
    [SerializeField] private Vector2 maxSpeed;
    [SerializeField, Range(0.0f, 5.0f)] private float moveAcceleration;
    [SerializeField] private float dustCreationThreshold;
    [SerializeField] private float longJumpVelocityThreshold;
    [Space]
    [SerializeField, Range(0.0f, 100.0f)] private float currentGroundMovementLerpSpeed;
    [SerializeField, Range(0.0f, 100.0f)] private float groundBaseMovementLerpSpeed;
    [SerializeField, Range(0.0f, 100.0f)] private float groundStopMovementLerpSpeed;
    [SerializeField, Range(0.0f, 100.0f)] private float groundTurnMovementLerpSpeed;
    [Space]
    [SerializeField, Range(0.0f, 100.0f)] private float currentAirborneMovementLerpSpeed;
    [SerializeField, Range(0.0f, 100.0f)] private float airborneBaseMovementLerpSpeed;
    [SerializeField, Range(0.0f, 100.0f)] private float airborneStopMovementLerpSpeed;
    [SerializeField, Range(0.0f, 100.0f)] private float airborneTurnMovementLerpSpeed;
    [Space]
    [SerializeField, Range(0.0f, 100.0f)] private float multipleJumpMovementLerpSpeed;
    [SerializeField, Range(0.0f, 100.0f)] private float wallJumpMovementLerpSpeed;

    [Header("----- Vertical Movement -----", order = 1)]
    [SerializeField, Range(0.0f, 30.0f)] private float jumpForce;
    [SerializeField, Range(0.0f, 30.0f)] private float multipleJumpForce;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField, Range(0, 10)] private int extraJumpsMax, extraJumpsRemaining;
    [Space]
    [SerializeField, Range(0.0f, 10.0f)] private float fallGravityScale;
    [SerializeField, Range(0.0f, 10.0f)] private float ascendingGravityScale;
    [SerializeField, Range(0.0f, 10.0f)] private float lowJumpGravityScale;
    [SerializeField, Range(-2.0f, 2.0f)] private float jumpVelocityFalloff;
    [SerializeField, Range(0.0f, 1.0f)] private float coyoteJumpLength = 0.2f;
    [SerializeField, Range(0.0f, 1.0f)] private float coyoteJumpTimer;
    [SerializeField, Range(0.0f, 2.0f)] private float jumpDelayLength = 0.2f;
    [SerializeField, Range(0.0f, 2.0f)] private float jumpDelayTimer;
    [SerializeField] private bool hasPressedJumpButtonThisFrame;
    [SerializeField] private float wallJumpLock;
    [Space]
    [SerializeField] private float timeLastWallJumped;
    [Space]
    [SerializeField, Range(-3.0f, 0.0f)] private float wallSlideSpeed = -1.0f;

    /*[Header("----- Dash -----", order = 1)]
    [SerializeField, Range(0.0f, 20.0f)] private float dashSpeed;
    [SerializeField, Range(0.0f, 5.0f)] private float dashLength;
    [SerializeField] private float timeStartedDash;
    [SerializeField] private Vector2 dashDirection;
    public static event Action OnStartDashing, OnStopDashing;*/

    [Header("----- Ground Detection -----", order = 1)]
    [SerializeField, Range(-0.5f, 0.5f)] private float grounderHorizontalOffset;
    [SerializeField, Range(0.0f, 0.5f)] private float grounderVerticalOffset;
    [SerializeField, Range(0.0f, 0.5f)] private float grounderRadius;
    [SerializeField, Range(0.0f, 1.0f)] private float grounderWidth, grounderHeigth;
    [SerializeField, Range(0.0f, 0.5f)] private float wallCheckHorizontalOffset;
    [SerializeField, Range(0.0f, 0.5f)] private float wallCheckVerticalOffset;
    [SerializeField, Range(0.0f, 0.5f)] private float wallCheckRadius;

    [Header("----- States -----", order = 0), Space]
    [Header("----- Horizontal Movement -----", order = 1)]
    [SerializeField] private bool canGetHorizontalInput;
    [SerializeField] private bool isGettingHorizontalInput;
    [SerializeField] private bool hasHorizontalVelocity;
    [SerializeField] private bool isFacingRight;
    [SerializeField] private bool isFacingLeft;
    [Space]
    [SerializeField] private bool isTurningRight;
    [SerializeField] private bool isTurningLeft;
    [SerializeField] private bool isChangingDirections;
    [Space]
    [SerializeField] private bool isStatic;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isStrafing;
    [SerializeField] private bool isMovingRight;
    [SerializeField] private bool isMovingLeft;
    [Space]
    [SerializeField] private bool isAgainstLeftWall;
    [SerializeField] private bool isAgainstRightWall;
    [SerializeField] private bool isAgainstAnyWall;
    [SerializeField] private bool hitsWall;
    [SerializeField] private bool isPushingLeftWall;
    [SerializeField] private bool isPushingRightWall;
    [SerializeField] private bool isPushingAnyWall;

    [Header("----- Vertical Movement -----", order = 1)]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isAirborne;
    [SerializeField] private bool isAscending;
    [SerializeField] private bool isFalling;
    [SerializeField] private bool hitsGround;
    [Space]
    [SerializeField] private bool isWallSliding;
    [SerializeField] private bool isWallGrabbing = false;
    [SerializeField] private bool isWallClimbing;
    [Space]
    [SerializeField] private bool isPressingJumpButton;
    [SerializeField] private bool hasPressedJumpButton;
    [SerializeField] private bool hasReleasedJumpButton;
    [SerializeField] private bool canFirstJump;
    [SerializeField] private bool canFirstJumpInAir;
    [SerializeField] private bool isBetweenJumpDelay = false;
    [SerializeField] private bool hasFirstJumped;
    [SerializeField] private bool canMultipleJump;
    [SerializeField] private bool hasMultipleJumped;
    [SerializeField] private bool canWallJump;
    [SerializeField] private bool hasWallJumped;

    [Header("----- Other -----", order = 1)]
    [SerializeField] private bool canDash;
    [SerializeField] private bool hasDashed;
    [SerializeField] private bool isDashing;

    private void Awake() {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        //if (runningDustPS == null) runningDustPS = FindObjectOfType<ParticleSystem>();
        if (boxCollider = null) boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start() {
        canWalk = true;
        extraJumpsRemaining = extraJumpsMax;
        lowJumpGravityScale = fallGravityScale * 2.0f;

        isWallGrabbing = false;
    }

    private void OnEnable() {
        SubscribeToEvents();
    }

    private void OnDisable() {
        UnsubscribeToEvents();
    }

    private void SubscribeToEvents() {
        playerInputs.moveEvent += ReadMovementInputs;
        playerInputs.jumpStartEvent += ReadJumpInputOnPress;
        playerInputs.jumpStopEvent += ReadJumpInputOnRelease;
        //playerInputs.dodgeEvent += Dodge;
    }

    private void UnsubscribeToEvents() {
        playerInputs.moveEvent -= ReadMovementInputs;
        playerInputs.jumpStartEvent -= ReadJumpInputOnPress;
        playerInputs.jumpStopEvent -= ReadJumpInputOnRelease;
        //playerInputs.dodgeEvent -= Dodge;
    }

    private void Update() {
        UpdateAnimations();

        if (enableSmoothHorizontalMovement)
            SmoothHorizontalMovement();
        else {
            currentGroundMovementLerpSpeed = 100.0f;
            currentAirborneMovementLerpSpeed = 100.0f;
        }

        if (hasPressedJumpButton) {
            HandleJumping();
        }

        HandlePlayerStates();
        HandleGrounding();
    }

    private void FixedUpdate() {
        HandleMovement();
        HandleFalling();
        if (enableWallSliding) HandleWallSlide();
        //HandleWallGrab();
        //HandleDashing();
    }

#region Inputs
    private void ReadMovementInputs() {
        if (canGetHorizontalInput){
            inputs.horizontal = playerInputs.horizontalInput;
            inputs.horizontalRaw = Mathf.RoundToInt(inputs.horizontal);
        }
        inputs.vertical = playerInputs.verticalInput;
        inputs.verticalRaw = Mathf.RoundToInt(inputs.vertical);

        movementDirection = playerInputs.movementInputs;
    }

    private void ReadJumpInputOnPress() {
        jumpingInput = playerInputs.jumping;
        isPressingJumpButton = jumpingInput != 0.0f;

        hasReleasedJumpButton = false;
        hasPressedJumpButton = true;
        hasPressedJumpButtonThisFrame = playerInputs.gameInput.Gameplay.Jump.triggered;

        if (hasPressedJumpButton) {
            HandleJumping();
            hasPressedJumpButton = false;
        }
    }

    private void ReadJumpInputOnRelease() {
        jumpingInput = playerInputs.jumping;
        isPressingJumpButton = jumpingInput != 0.0f;

        hasPressedJumpButton = false;
        hasReleasedJumpButton = true;
    }
#endregion

#region Detection
    private void HandleGrounding() {
        // Ground Detection
        //hitsGround = Physics2D.OverlapCircleNonAlloc((Vector2)transform.position + new Vector2(grounderHorizontalOffset, grounderVerticalOffset), grounderRadius, ground, groundLayer) > 0 ||
        //            Physics2D.OverlapCircleNonAlloc((Vector2)transform.position + new Vector2(-grounderHorizontalOffset, grounderVerticalOffset), grounderRadius, ground, groundLayer) > 0;
        hitsGround = Physics2D.OverlapBoxNonAlloc((Vector2)transform.position + new Vector2(grounderHorizontalOffset, grounderVerticalOffset), new Vector2(grounderWidth, grounderHeigth), 0.0f, ground, groundLayer) > 0;

        if (!isGrounded && hitsGround) {
            isGrounded = true;
            hasDashed = false;

            ResetJumpingParameters();

            transform.SetParent(ground[0].transform);
            PlayParticle(landingDustPS);
        }
        else if (isGrounded && !hitsGround) {
            isGrounded = false;
            transform.SetParent(null);
        }

        // Wall Detection
        isAgainstRightWall = Physics2D.OverlapCircleNonAlloc((Vector2)transform.position + new Vector2(wallCheckHorizontalOffset, wallCheckVerticalOffset), wallCheckRadius, rightWall, groundLayer) > 0;
        isAgainstLeftWall = Physics2D.OverlapCircleNonAlloc((Vector2)transform.position + new Vector2(-wallCheckHorizontalOffset, wallCheckVerticalOffset), wallCheckRadius, leftWall, groundLayer) > 0;
        hitsWall = isAgainstRightWall || isAgainstLeftWall;

        if (!isAgainstAnyWall && hitsWall) {
            isAgainstAnyWall = true;
            //hasWallJumped = false;
            //hasMultipleJumped = false;
        }
        else if (isAgainstAnyWall && !hitsWall) {
            isAgainstAnyWall = false;
        }

        isPushingRightWall = isAgainstRightWall && isMovingRight;
        isPushingLeftWall = isAgainstLeftWall && isMovingLeft;
        isPushingAnyWall = isPushingLeftWall || isPushingRightWall;
    }

    private void HandlePlayerStates() {
        isGettingHorizontalInput = inputs.horizontalRaw != 0.0f ? true : false;
        lastHorizontalDirection = isGettingHorizontalInput ? inputs.horizontalRaw : lastHorizontalDirection;

        hasHorizontalVelocity = rb.velocity.x != 0.0f ? true : false;
        isStatic = isGrounded && !hasHorizontalVelocity;
        isWalking = isGrounded && hasHorizontalVelocity;
        isStrafing = isAirborne && hasHorizontalVelocity;

        isFacingRight = 0.0f <= lastHorizontalDirection ? true : false;
        isFacingLeft = !isFacingRight ? true : false;
        isMovingRight = 0.0f < inputs.horizontal && isFacingRight; // Is moving and facing right and its velocity is greater than 0
        isMovingLeft = inputs.horizontal < 0.0f && isFacingLeft; // Is moving and is facing left and its velocity is less than 0
        canTurn = isWallGrabbing || isDashing ? false : true;
        isTurningRight = canTurn && rb.velocity.x < 0.0f && isMovingRight; // Is already moving left, or facing left, but wants to move right, and can actually turn
        isTurningLeft = canTurn && 0.0f < rb.velocity.x && isMovingLeft; // Is already moving right, or facing right, but wants to move left, and can actually turn
        isChangingDirections = isTurningLeft || isTurningRight; // Is either turning left or turning right

        isAirborne = !isGrounded ? true : false;
        canMultipleJump = enableMultipleJump && !canFirstJump && extraJumpsRemaining != 0 ? true : false;
        canWallJump = enableWallJump && (isAgainstAnyWall && isAirborne) ? true : false;

        isFalling = (isAirborne || isWallSliding) && (rb.velocity.y < 0.0f);
        isAscending = isAirborne && (0.0f < rb.velocity.y);
        isWallClimbing = isWallGrabbing && (0.0f < rb.velocity.y);

        // Coyote Timer counter
        if (isGrounded) {
            coyoteJumpTimer = coyoteJumpLength;
        }
        else { //if (isAirborne)
            coyoteJumpTimer -= Time.deltaTime;
            coyoteJumpTimer = Mathf.Clamp(coyoteJumpTimer, 0.0f, coyoteJumpLength);

            if (0.0 < coyoteJumpTimer && !hasFirstJumped) {
                canFirstJumpInAir = true;
            }
            else if (hasFirstJumped || coyoteJumpTimer <= 0.0f || !canFirstJump) {
                coyoteJumpTimer = 0.0f;
                if (canFirstJump) canFirstJump = false;
                canFirstJumpInAir = false;
            }
        }

        // Jump Delay counter
        if (hasPressedJumpButtonThisFrame) {
            jumpDelayTimer = jumpDelayLength;
            hasPressedJumpButtonThisFrame = false;
        }
        else if (!hasPressedJumpButtonThisFrame) {
            jumpDelayTimer -= Time.deltaTime;
            jumpDelayTimer = Mathf.Clamp(jumpDelayTimer, 0.0f, jumpDelayLength);

            if (0.0f < jumpDelayTimer) {
                if ((hitsGround || isWallSliding) && isPressingJumpButton) {
                    isBetweenJumpDelay = true;
                    HandleJumping();
                }
            }
            else if (jumpDelayTimer <= 0.0f) {
                isBetweenJumpDelay = false;
            }
        }

        if (hasReleasedJumpButton) {
            if (!hasMultipleJumped)
                animator.ResetTrigger("Jump");
            if (hasMultipleJumped) {
                animator.ResetTrigger("MultipleJump");
                hasMultipleJumped = false;
            }
            if (hasWallJumped) {
                animator.ResetTrigger("WallJump");
                hasWallJumped = false;
            }
        }
    }
#endregion

#region Horizontal Movement
    private void HandleMovement() {
        //if (isDashing) return;
        
        var idealVelocity = new Vector2(Mathf.RoundToInt(inputs.horizontal) * moveSpeed, rb.velocity.y);
        if (isGrounded && !isPushingAnyWall) {
            rb.velocity = Vector2.MoveTowards(rb.velocity, idealVelocity, currentGroundMovementLerpSpeed * Time.fixedDeltaTime);
        }
        else if (isAirborne) {
            rb.velocity = Vector2.MoveTowards(rb.velocity, idealVelocity, currentAirborneMovementLerpSpeed * Time.fixedDeltaTime);
        }
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x), Mathf.Clamp(rb.velocity.y, -maxSpeed.y * rb.gravityScale, maxSpeed.y));
    }
    
    private void SmoothHorizontalMovement() {
        // This can be done using just X & Y input as they lerp to max values, but this gives greater control over velocity acceleration
        var acceleration = isGrounded ? moveAcceleration : moveAcceleration * 0.5f;

        if (isChangingDirections) {
            if (isWalking) {
                currentGroundMovementLerpSpeed = groundTurnMovementLerpSpeed;
                SetCurrentMovementLerpSpeed(currentGroundMovementLerpSpeed, groundTurnMovementLerpSpeed);
                if (isTurningRight) {
                    inputs.horizontal = Mathf.MoveTowards(inputs.horizontal, 1, acceleration * Time.deltaTime);
                    animator.ResetTrigger("IsTurningLeft");
                    animator.SetTrigger("IsTurningRight");
                }
                else if (isTurningLeft) {
                    inputs.horizontal = Mathf.MoveTowards(inputs.horizontal, -1, acceleration * Time.deltaTime);
                    animator.ResetTrigger("IsTurningRight");
                    animator.SetTrigger("IsTurningLeft");
                }         
                PlayParticle(runningDustPS);
            }
            else if (isAirborne) {
                if (hasMultipleJumped)
                    currentAirborneMovementLerpSpeed = multipleJumpMovementLerpSpeed;
                else if (hasWallJumped)
                    currentAirborneMovementLerpSpeed = wallJumpMovementLerpSpeed;
                else if (!hasWallJumped || !hasMultipleJumped)
                    currentAirborneMovementLerpSpeed = airborneTurnMovementLerpSpeed;
                currentAirborneMovementLerpSpeed = SetCurrentMovementLerpSpeed(currentAirborneMovementLerpSpeed, airborneBaseMovementLerpSpeed);
            }

            if ((isTurningLeft && rb.velocity.x < 0.0f) || (isTurningRight && 0.0f < rb.velocity.x)) {
                isTurningRight = false;
                isTurningLeft = false;
                isChangingDirections = false;
            }
        }
        else if (!isChangingDirections) {
            animator.ResetTrigger("IsTurningRight");
            animator.ResetTrigger("IsTurningLeft");
            if (isGrounded) {
                if (isGettingHorizontalInput) {   
                    if (hasHorizontalVelocity && Mathf.Abs(rb.velocity.x) < dustCreationThreshold)
                        PlayParticle(runningDustPS);
                }
                else if (!isGettingHorizontalInput) {
                    inputs.horizontal = Mathf.MoveTowards(inputs.horizontal, 0, acceleration * 2f * Time.deltaTime);
                    currentGroundMovementLerpSpeed = groundStopMovementLerpSpeed;

                    if (hasHorizontalVelocity && Mathf.Abs(rb.velocity.x) > dustCreationThreshold)
                        PlayParticle(runningDustPS);
                }
                currentGroundMovementLerpSpeed = SetCurrentMovementLerpSpeed(currentGroundMovementLerpSpeed, groundBaseMovementLerpSpeed);
            }
            else if (isAirborne) {
                if (!isGettingHorizontalInput) {
                    currentAirborneMovementLerpSpeed = airborneStopMovementLerpSpeed;
                }
                currentAirborneMovementLerpSpeed = SetCurrentMovementLerpSpeed(currentAirborneMovementLerpSpeed, airborneBaseMovementLerpSpeed);
            }
        }

        float SetCurrentMovementLerpSpeed(float lerpTarget, float lerpAmount) {
            var lerpResult = Mathf.MoveTowards(lerpTarget, lerpAmount, 100.0f * Time.deltaTime);;

            return lerpResult;
        }
    }
#endregion

#region Vertical Movement
    private void HandleJumping() {
        //if (isDashing) return;

        if ((canWallJump && (isAirborne || isWallGrabbing))) {
            ExecuteWallJump(new Vector2(isAgainstLeftWall ? wallJumpForce.x: -wallJumpForce.x, enableDirectionalJump && movementDirection.y < 0.0f ? -(wallJumpForce.y / 3.0f) : wallJumpForce.y));
        }
        else if ((canFirstJump || canFirstJumpInAir) || isBetweenJumpDelay) {
            if (0.0f <= movementDirection.y || (movementDirection.y < 0.0f && !isGettingHorizontalInput))
                ExecuteNormalJump(new Vector2(rb.velocity.x, isGettingHorizontalInput ? jumpForce * 0.85f : jumpForce));
            else if (enableDirectionalJump && movementDirection.y < 0.0f && isGettingHorizontalInput) {
                ExecuteNormalJump(new Vector2(longJumpVelocityThreshold < Mathf.Abs(rb.velocity.x) ? jumpForce * inputs.horizontalRaw * 1.5f : rb.velocity.x, jumpForce * 0.70f));
            }
        }
        else if (canMultipleJump && isAirborne) {
            ExecuteMultipleJump(isGettingHorizontalInput ? new Vector2(multipleJumpForce * inputs.horizontalRaw * 1.2f, multipleJumpForce * 0.8f) : new Vector2(rb.velocity.x, multipleJumpForce));
        }
    }

    private void ExecuteNormalJump(Vector2 direction) {
        animator.SetTrigger("Jump");
        coyoteJumpTimer = 0.0f;
        jumpDelayTimer = 0.0f;
        hasFirstJumped = true;
        canFirstJump = false;
        canFirstJumpInAir = false;
        currentAirborneMovementLerpSpeed = airborneBaseMovementLerpSpeed;
        rb.velocity = direction;
        normalJumpDustPS.transform.up = rb.velocity;
        PlayParticle(normalJumpDustPS);
    }

    private void ExecuteMultipleJump(Vector2 direction) {
        animator.SetTrigger("MultipleJump");
        hasMultipleJumped = true;
        currentAirborneMovementLerpSpeed = multipleJumpMovementLerpSpeed;
        extraJumpsRemaining--;
        rb.velocity = Vector2.zero;
        rb.velocity = direction;
        multipleJumpDustPS.transform.up = rb.velocity;
        PlayParticle(multipleJumpDustPS);
    }

    private void ExecuteWallJump(Vector2 direction) {
        animator.ResetTrigger("WallJump");
        animator.SetTrigger("WallJump");
        animator.SetTrigger("Jump");
        jumpDelayTimer = 0.0f;
        hasWallJumped = true;
        RefundExtraJumps(1);
        currentAirborneMovementLerpSpeed = wallJumpMovementLerpSpeed;
        timeLastWallJumped = Time.time;
        rb.velocity = direction;
        wallJumpDustPS.transform.up = rb.velocity;
        PlayParticle(wallJumpDustPS);
    }
    
    private void ResetJumpingParameters() {
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("MultipleJump");
        animator.ResetTrigger("WallJump");

        canFirstJump = true;
        canMultipleJump = false;
        canWallJump = false;

        hasFirstJumped = false;
        hasMultipleJumped = false;
        hasWallJumped = false;

        coyoteJumpTimer = coyoteJumpLength;

        RefundExtraJumps(extraJumpsMax);
    }

    private void RefundExtraJumps(int refundAmount) {
        extraJumpsRemaining = Mathf.Clamp(extraJumpsRemaining + refundAmount, 0, extraJumpsMax);
    }

    private void HandleFalling() {
        // Fall faster and allow small jumps. jumpVelocityFallof is the point at which we start adding extra gravity. Using 0 causes floating
        if (!isWallSliding) {
            if (isFalling)
                SetGravityScale(fallGravityScale);
            else if (isAscending && (!hasReleasedJumpButton || !enableVariableJump))
                SetGravityScale(ascendingGravityScale);
            else if (isAscending && hasReleasedJumpButton && enableVariableJump) {
                SetGravityScale(lowJumpGravityScale);
                if (enableInstantFall)
                    rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                else {
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, 0.0f, Time.fixedDeltaTime));
                }
            }
            else
                SetGravityScale(1.0f);
        }
        else if (isWallSliding) {
            if (isAscending)
                SetGravityScale(ascendingGravityScale * 1.5f);
            else if (isFalling)
                SetGravityScale((fallGravityScale * 2.0f) / 3.0f);
        }
    }

    private void SetGravityScale(float gravity) {
        rb.gravityScale = gravity;
    }

    private void HandleWallSlide() {
        //var sliding = isPushingAnyWall && !isAscending;
        var sliding = isAgainstAnyWall && !isGrounded && !isAscending;

        if (!isWallSliding && sliding) {
            animator.ResetTrigger("WallJump");
            isWallSliding = true;
            transform.SetParent(isAgainstLeftWall ? leftWall[0].transform : rightWall[0].transform);
            wallSlideDustPS.transform.position = (Vector2)transform.position + new Vector2(isAgainstLeftWall ? -wallCheckHorizontalOffset : wallCheckHorizontalOffset, wallCheckVerticalOffset);
            wallSlideDustPS.transform.rotation = Quaternion.Euler(0.0f, 0.0f, isAgainstLeftWall ? 180.0f : -180.0f);
            PlayParticle(wallSlideDustPS);
        }
        else if (isWallSliding && ((!isWallGrabbing && !sliding) || hitsGround)) {
            if (!hitsGround) transform.SetParent(null);
            lastHorizontalDirection *= -1.0f; // Change last horizontal view direction to that of the opposite direction of the wall the player is sliding
            isWallSliding = false;
            PlayParticle(wallSlideDustPS, false);
        }

        // Dont add sliding until actually falling or it will prevent jumping against a wall
        if (isFalling && isWallSliding) { // Normal Wall Slide speed
            if (0.0f <= movementDirection.y)
                rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(rb.velocity.x, wallSlideSpeed), 50.0f * Time.fixedDeltaTime);
            else if (movementDirection.y < 0.0f) // Faster Wall Slide speed
                rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(rb.velocity.x, wallSlideSpeed * rb.gravityScale), 50.0f * Time.fixedDeltaTime);

            lastHorizontalDirection = isAgainstLeftWall ? -1.0f : 1.0f; // Change last horizontal view direction to that of the direction of the wall the player is sliding
            if (!isGettingHorizontalInput)
                rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(isAgainstLeftWall ? -moveSpeed : moveSpeed, rb.velocity.y), 50.0f * Time.fixedDeltaTime);
        }
    }
#endregion

#region Dash
    private void HandleDashing() {

    }
#endregion

    private void UpdateAnimations()
    {
        animator.SetFloat("CurrentHorizontalDirection", hasHorizontalVelocity ? inputs.horizontal : 0.0f);
        animator.SetFloat("LastHorizontalDirection", lastHorizontalDirection);
        animator.SetFloat("VerticalDirection", inputs.vertical);
        animator.SetFloat("HorizontalVelocity", rb.velocity.x);
        animator.SetFloat("HorizontalVelocityNormalized", rb.velocity.normalized.x);
        animator.SetFloat("VerticalVelocity", isAirborne ? rb.velocity.y : 0.0f);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsAirborne", isAirborne);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsAscending", isAscending);
        animator.SetBool("IsMoving", isGettingHorizontalInput);
        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsAgainstAnyWall", isAgainstAnyWall);
        animator.SetBool("IsPushingAnyWall", isPushingAnyWall);
        animator.SetBool("IsWallSliding", isWallSliding);
    }

    private void DrawGrounderGizmos() {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(grounderHorizontalOffset, grounderVerticalOffset), grounderRadius);
        //Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(-grounderHorizontalOffset, grounderVerticalOffset), grounderRadius);
        Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(grounderHorizontalOffset, grounderVerticalOffset), new Vector2(grounderWidth, grounderHeigth));
    }

    private void DrawWallSlideGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(wallCheckHorizontalOffset, wallCheckVerticalOffset), wallCheckRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(-wallCheckHorizontalOffset, wallCheckVerticalOffset), wallCheckRadius);
    }

    private void OnDrawGizmos() {
        DrawGrounderGizmos();
        DrawWallSlideGizmos();
    }

    private void PlayParticle(ParticleSystem particle, bool activate = true) {
        if (activate) particle.Play();
        else particle.Stop();
    }
}