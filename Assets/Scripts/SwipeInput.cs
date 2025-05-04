using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public float minSwipeDistance = 50f;
    public PinyataController pinataController;

    private Vector2 startTouch;
    private bool isSwiping = false;

    private void Update()
    {
        if (!GameStateManager.Instance.IsState(GameState.Playing))
        {
            return;
        }
        
#if UNITY_EDITOR

        if (Input.GetMouseButtonDown(0))
        {
            startTouch = UnityEngine.Input.mousePosition;
            isSwiping = true;
        }

        if (UnityEngine.Input.GetMouseButtonUp(0) && isSwiping)
        {
            Vector2 endTouch = UnityEngine.Input.mousePosition;
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

        if (swipeDelta.magnitude > minSwipeDistance)
        {
            var swipeDir = swipeDelta.normalized;
            pinataController.OnSwipe(swipeDir, swipeDelta.magnitude);
        }
    }
}