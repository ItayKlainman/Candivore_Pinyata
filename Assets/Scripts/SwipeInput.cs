using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public float minSwipeDistance = 50f;

    private Vector2 startTouch;
    private bool isSwiping = false;

    private PinyataController pinataController;


    public void Initialize(PinyataController pinata)
    {
        pinataController = pinata;
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsState(GameState.Playing))
        {
            return;
        }
        
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
        
        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        
        if (hit.collider != null && hit.collider.CompareTag("HealthPack"))
        {
            hit.collider.GetComponent<HealthPack>()?.TriggerHeal();
        }

        if (swipeDelta.magnitude > minSwipeDistance)
        {
            var swipeDir = swipeDelta.normalized;
            pinataController.OnSwipe(swipeDir, swipeDelta.magnitude);
        }
    }
}