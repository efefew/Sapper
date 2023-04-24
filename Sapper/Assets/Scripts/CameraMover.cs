using UnityEngine;

public class CameraMover : MonoBehaviour
{
    private Transform tr;
    private Camera cam;
    public float speed = 0.001f, zoom = 0.05f;
    [SerializeField]
    [Header("крайние углы камеры")]
    private Vector2 minCam, maxCam;
    [Header("крайние углы поля")]
    public Vector2 min, max;
    public RectTransform rect;

    private void Start()
    {
        tr = transform;
        tr.position = new Vector3(0, 0, -100);
        cam.orthographicSize = 1000;
        OnChangeCameraPosition();
    }

    /// <summary>
    /// вычисляет расположения углов камеры в пространстве
    /// </summary>
    /// <param name="camera">камера, над которой производят рассчёты</param>
    /// <returns>расположение углов камеры в пространстве</returns>
    private (Vector2, Vector2) CameraBorders(Camera camera)
    {
        float size = camera.orthographicSize;
        int width = Screen.width;
        int height = Screen.height;
        Vector2 cameraPosition = camera.transform.position;
        Vector2 minCameraPoint,
                maxCameraPoint;
        float wh = (float)width / height;

        minCameraPoint.x = -size * wh + cameraPosition.x;
        minCameraPoint.y = -size + cameraPosition.y;
        maxCameraPoint.x = size * wh + cameraPosition.x;
        maxCameraPoint.y = size + cameraPosition.y;

        return (minCameraPoint, maxCameraPoint);
    }

    /// <summary>
    /// вычисляет позицию камеры, чтобы она была внутри границ
    /// </summary>
    /// <param name="point">границы, за которые камера не должна выходить (мин, макс)</param>
    /// <param name="camera">камера, над которой производят рассчёты</param>
    /// <param name="z">значение оси z камеры</param>
    /// <returns>позиция камеры внутри границ</returns>
    private Vector3 CameraPositionInsideBorders((Vector2 min, Vector2 max) point, Camera camera, float z)
    {
        (Vector2 min, Vector2 max) cameraPoint = CameraBorders(cam);
        float size = camera.orthographicSize;
        int width = Screen.width;
        int height = Screen.height;
        Vector2 cameraPosition = camera.transform.position;
        float wh = (float)width / height;
        float sizeX = size, sizeY = size;
        if ((point.max.x - point.min.x) < (cameraPoint.max.x - cameraPoint.min.x))
            sizeX = (point.max.x - point.min.x) / (2 * wh);
        if ((point.max.y - point.min.y) < (cameraPoint.max.y - cameraPoint.min.y))
            sizeY = (point.max.y - point.min.y) / 2;
        if (sizeX != sizeY)
            size = sizeX < sizeY ? sizeX : sizeY;
        camera.orthographicSize = size;

        if (point.min.x > cameraPoint.min.x)
            cameraPosition = new Vector2(point.min.x + size * wh, cameraPosition.y);
        if (point.min.y > cameraPoint.min.y)
            cameraPosition = new Vector2(cameraPosition.x, point.min.y + size);

        if (point.max.x < cameraPoint.max.x)
            cameraPosition = new Vector2(point.max.x - size * wh, cameraPosition.y);
        if (point.max.y < cameraPoint.max.y)
            cameraPosition = new Vector2(cameraPosition.x, point.max.y - size);
        return new Vector3(cameraPosition.x, cameraPosition.y, z);
    }

    /// <summary>
    /// меняет позицию интерфейса по краям оси Y
    /// </summary>
    /// <param name="up">поставить интерфейс в верхний угол?</param>
    /// <param name="rect">интерфейс</param>
    private void RectChangeY(bool up, RectTransform rect)
    {
        rect.anchorMin = new Vector2(rect.anchorMin.x, up ? 0 : 1);
        rect.anchorMax = new Vector2(rect.anchorMax.x, up ? 0 : 1);
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, up ? 1 : -1 * 225);
    }

    /// <summary>
    /// меняет позицию интерфейса по краям оси X
    /// </summary>
    /// <param name="right">поставить интерфейс в правый угол?</param>
    /// <param name="rect">интерфейс</param>
    private void RectChangeX(bool right, RectTransform rect)
    {
        rect.anchorMin = new Vector2(right ? 0 : 1, rect.anchorMin.y);
        rect.anchorMax = new Vector2(right ? 0 : 1, rect.anchorMin.y);
        rect.anchoredPosition = new Vector2(right ? 1 : -1 * 175, rect.anchoredPosition.y);
    }
    /// <summary>
    /// Вызывает при изменении камеры
    /// </summary>
    public void OnChangeCameraPosition()
    {
        if (!cam)
            cam = Camera.main;
        cam.transform.position = CameraPositionInsideBorders((min, max), cam, -100);
        (minCam, maxCam) = CameraBorders(cam);
        RectChangeY((minCam.y - min.y) >= (max.y - maxCam.y), rect);
        RectChangeX((minCam.x - min.x) >= (max.x - maxCam.x), rect);
    }

    private void FixedUpdate()
    {
        if (Input.anyKey || Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if ((Input.GetKey(KeyCode.E) || Input.GetAxis("Mouse ScrollWheel") >= 0.1) && cam.orthographicSize > 3)
                cam.orthographicSize -= zoom * cam.orthographicSize;
            if (Input.GetKey(KeyCode.Q) || Input.GetAxis("Mouse ScrollWheel") <= -0.1)
                cam.orthographicSize += zoom * cam.orthographicSize;

            float force = speed * cam.orthographicSize;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                tr.position -= transform.right * force;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                tr.position += transform.right * force;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                tr.position += transform.up * force;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                tr.position -= transform.up * force;
            OnChangeCameraPosition();
        }
    }
}
