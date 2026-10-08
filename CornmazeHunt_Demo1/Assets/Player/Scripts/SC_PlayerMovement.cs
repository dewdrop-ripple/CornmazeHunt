using Unity.Android.Gradle.Manifest;
using UnityEngine;
using static UnityEngine.LightAnchor;

public class SC_PlayerMovement : MonoBehaviour
{
    public enum PlayerMovementSetting
    {
        WALK,
        RUN,
        IN_MENU
    }

    [SerializeField] private PlayerMovementSetting mMovementSetting = PlayerMovementSetting.WALK;

    [SerializeField] private float mWalkSpeed = 1.0f;
    [SerializeField] private float mRunSpeed = 1.0f;

    private Vector3 mKinematicMotion = Vector3.zero;

    [SerializeField] private Camera mPlayerCamera;
    [SerializeField] private GameObject mPlayerBody;
    private CharacterController mCharacterController;
    private Rigidbody mRigidbody;

    // --------------------
    // Credit: Karl Ramstedt on Github - https://gist.github.com/KarlRamstedt/407d50725c7b6abeaf43aee802fdd88e
    // --------------------
    [Range(0.1f, 9f)][SerializeField] private float mLookSensitivity = 2f;
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] private float mLookYRotationLimit = 88f;

    private Vector2 mLookRotation = Vector2.zero;
    // --------------------

    [SerializeField] private float mBaseSprintStaminaDrain;

    [SerializeField] private float mMaxHealth;
    [SerializeField] private float mMaxStamina;

    [SerializeField] private float mHealth;
    [SerializeField] private float mStamina;

    // UI
    [SerializeField] private Canvas mHUD;

    // Start Delay
    [SerializeField] private bool mIsEnabled = false;

    [SerializeField] private int mMapFillInRadius = 3;
    

    private void Start()
    {
        mCharacterController = GetComponent<CharacterController>();
        mRigidbody = GetComponent<Rigidbody>();
    }

    // --- Movement and Data --- //

    private void UpdateCharacterData()
    {
        if (mMovementSetting == PlayerMovementSetting.RUN)
        {
            mStamina -= mBaseSprintStaminaDrain * Time.deltaTime;
        }
        else if (mStamina < mMaxStamina)
        {
            mStamina += mBaseSprintStaminaDrain / 3.0f * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!mIsEnabled)
        {
            return;
        }

        if (mMovementSetting == PlayerMovementSetting.IN_MENU)
        {
            SetCursorLocked(false);
            mHUD.enabled = false;
        }
        else
        {
            SetCursorLocked(true);
            mHUD.enabled = true;

            UpdateCharacterData();

            // Movement
            SetLookRotation();
            ApplyPlayerInput();

            // Lock to Ground
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 1))
            {
                if (hitInfo.distance > 0.1f)
                {
                    mCharacterController.Move(new Vector3(0, -1 * hitInfo.distance, 0));
                }
            }
            else
            {
                mCharacterController.Move(new Vector3(0, -0.5f, 0));
            }

            // Move
            mCharacterController.Move(mKinematicMotion * Time.deltaTime); // Velocity is in m/s, so account for framerate
        }
    }

    // --------------------
    // Credit: Karl Ramstedt on Github - https://gist.github.com/KarlRamstedt/407d50725c7b6abeaf43aee802fdd88e
    // --------------------
    private void SetLookRotation()
    {
        mLookRotation.x += Input.GetAxis("Mouse X") * mLookSensitivity;
        mLookRotation.y += Input.GetAxis("Mouse Y") * mLookSensitivity;
        mLookRotation.y = Mathf.Clamp(mLookRotation.y, -mLookYRotationLimit, mLookYRotationLimit);

        var xQuat = Quaternion.AngleAxis(mLookRotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(mLookRotation.y, Vector3.left);

        transform.localRotation = xQuat;
        mPlayerCamera.transform.localRotation = yQuat;
    }
    // --------------------

    // Lock/Unlock cursor for UI vs game
    private void SetCursorLocked(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void ApplyPlayerInput()
    {
        mKinematicMotion = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && mStamina > 5 && mKinematicMotion != Vector3.zero)
        {
            mMovementSetting = PlayerMovementSetting.RUN;
        }
        else
        {
            mMovementSetting = PlayerMovementSetting.WALK;
        }

        // RMB
        if (Input.GetMouseButtonDown(1))
        {
            FindAnyObjectByType<SC_MazeManager>().FillInTilesAroundSpot(transform.position, mMapFillInRadius);
        }

        switch (mMovementSetting)
        {
            case PlayerMovementSetting.RUN:
                mKinematicMotion *= mRunSpeed;
                break;

            case PlayerMovementSetting.WALK:
            default:
                mKinematicMotion *= mWalkSpeed;
                break;
        }

        mKinematicMotion = transform.TransformDirection(mKinematicMotion);
    }


    // --- Damage --- //

    public void TakeDamage(float damage)
    {
        if (mHealth < 0.0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }


    public void SetHealth(float health)
    {
        this.mHealth = health;
    }

    public float GetHealth()
    {
        return mHealth;
    }

    public float GetStamina()
    {
        return mStamina;
    }

    public float GetMaxHealth()
    {
        return mMaxHealth;
    }

    public float GetMaxStamina()
    {
        return mMaxStamina;
    }


    // --- Enabling --- //

    public void Enable()
    {
        mIsEnabled = true;
    }
}
