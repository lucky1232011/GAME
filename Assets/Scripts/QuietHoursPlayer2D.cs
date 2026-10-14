using UnityEngine;
using UnityEngine.InputSystem;

public sealed class QuietHoursPlayer2D : MonoBehaviour
{
    public float moveSpeed = 3.8f;
    public SpriteRenderer characterSprite;
    public Sprite[] animationFrames;
    public AudioSource footsteps;
    public AudioClip stepClip;
    public bool controlsEnabled;

    Rigidbody2D body;
    float stepClock;
    float animationClock;
    int animationIndex;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (!controlsEnabled || Keyboard.current == null)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        float direction = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direction -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direction += 1f;
        body.linearVelocity = new Vector2(direction * moveSpeed, 0f);

        if (Mathf.Abs(direction) > 0.1f)
        {
            characterSprite.flipX = direction < 0f;
            animationClock -= Time.deltaTime;
            if (animationClock <= 0f)
            {
                animationIndex = animationIndex == 1 ? 2 : 1;
                SetFrame(animationIndex);
                animationClock = 0.18f;
            }
            stepClock -= Time.deltaTime;
            if (stepClock <= 0f)
            {
                if (footsteps != null && stepClip != null) footsteps.PlayOneShot(stepClip, 0.38f);
                stepClock = 0.48f;
            }
        }
        else
        {
            stepClock = 0f;
            animationClock = 0f;
            animationIndex = 0;
            SetFrame(0);
        }
    }

    void SetFrame(int index)
    {
        if (animationFrames != null && animationFrames.Length > index && animationFrames[index] != null)
            characterSprite.sprite = animationFrames[index];
    }
}
