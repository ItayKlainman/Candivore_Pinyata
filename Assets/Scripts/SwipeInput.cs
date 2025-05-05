
using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public float minSwipeDistance = 50f;
    public LayerMask swipeableLayers;

    private Vector2 startTouch;
    private bool isSwiping = false;

    private PinyataController pinataController;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    public void Initialize(PinyataController pinata)
    {
        pinataController = pinata;
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsState(GameState.Playing))
            return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouch = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            Vector2 endTouch = Input.mousePosition;
            ProcessSwipe(endTouch);
            isSwiping = false;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouch = touch.position;
                isSwiping = true;
            }

            if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isSwiping)
            {
                ProcessSwipe(touch.position);
                isSwiping = false;
            }
        }
#endif
    }

    private void ProcessSwipe(Vector2 endTouch)
    {
        var swipeDelta = endTouch - startTouch;

        if (swipeDelta.magnitude < minSwipeDistance)
            return;

        Vector2 startWorld = _camera.ScreenToWorldPoint(startTouch);
        Vector2 endWorld = _camera.ScreenToWorldPoint(endTouch);
        
        var swipeDirection = (endWorld - startWorld).normalized;
        var hit = Physics2D.Linecast(startWorld, endWorld, swipeableLayers);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Pinata"))
            {
                pinataController?.OnSwipe(swipeDirection, swipeDelta.magnitude);
            }
            else if (hit.collider.CompareTag("HealthPack"))
            {
                hit.collider.GetComponent<HealthPack>()?.TriggerHeal();
            }
        }
    }
}
