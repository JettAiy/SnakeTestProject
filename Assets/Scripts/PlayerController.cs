using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public event System.Action<int> OnScoreChange;
    public event System.Action OnReset;

    //singleton
    public static PlayerController Current;

    //camera
    private Camera _camera;
    Vector3 cameraOffset = new Vector3(0, 0, -10f);

    //score
    private int score;

    private List<Transform> snakeSegments = new List<Transform>();
    private List<Vector2> snakePositions = new List<Vector2>();

    [Header("PREFABS")]
    [SerializeField] private Transform parentContainer;
    [SerializeField] private Transform prefab_snakeHead;
    [SerializeField] private Transform prefab_segment;
    
    [Space]
    [Header("CONTROL SETTINGS")]
    [SerializeField] private float circleDiameter = 2;
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private float rotationSpeed = 5f;

    [Space]
    [Header("RESOURCES")]
    [SerializeField] private Sprite defaultFood;
    [SerializeField] private Sprite gatheredFood;
    [SerializeField] private float foodRespawnTime;

    private List<CollisionDetector> foodDetectors = new List<CollisionDetector>();

    //player can control
    bool isNoControl;

    #region INIT
    private void Awake()
    {
        _camera = Camera.main;
        isNoControl = true;

        if (Current == null)
            Current = this;
        else
            Destroy(this);
    }

    private void Init()
    {
        //create snake head
        snakeSegments.Add(Instantiate(prefab_snakeHead, parentContainer));
        snakePositions.Add(snakeSegments[0].position);

        snakeSegments[0].GetComponent<SnakeHead>().OnCollision += PlayerController_OnCollision;

        //set camera to head
        _camera.transform.position = snakeSegments[0].position + cameraOffset;

        isNoControl = false;

        //reset score
        score = 0;
        OnScoreChange?.Invoke(score);
    }

    #endregion

    #region CONTROL

    private void Update()
    {
        if (snakeSegments.Count == 0 || isNoControl) return;

        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(AddSegment(1));

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        RotateSnake();
        MoveSnake();
        CheckFoodTimer();

    }

    private void LateUpdate()
    {
        if (snakeSegments.Count == 0 || isNoControl) return;

        Vector3 newPosition = Vector3.Lerp(_camera.transform.position, snakeSegments[0].position, Time.deltaTime);
        newPosition.z = cameraOffset.z;
        _camera.transform.position = newPosition;
    }



    public void ResetControl()
    {
        isNoControl = true;

        //delete all segments before clear list
        foreach (var segment in snakeSegments)
        {
            Destroy(segment.gameObject);
        }

        snakePositions.Clear();
        snakeSegments.Clear();

        Init();
    }

    private void RotateSnake()
    {
        Vector3 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        mousePos.x = mousePos.x - snakeSegments[0].position.x;
        mousePos.y = mousePos.y - snakeSegments[0].position.y;

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        Quaternion newRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        snakeSegments[0].localRotation = Quaternion.Lerp(snakeSegments[0].localRotation, newRotation, Time.deltaTime * rotationSpeed);
    }

    private void MoveSnake()
    {

        snakeSegments[0].Translate(snakeSegments[0].right * speed * Time.smoothDeltaTime, Space.World);

        float distance = ((Vector2)snakeSegments[0].position - snakePositions[0]).magnitude;

        if (distance > circleDiameter)
        {
            Vector2 direction = ((Vector2)snakeSegments[0].position - snakePositions[0]).normalized;

            snakePositions.Insert(0, snakePositions[0] + direction * circleDiameter);
            snakePositions.RemoveAt(snakePositions.Count - 1);

            distance -= circleDiameter;
        }

        for (int i = 1; i < snakeSegments.Count; i++)
        {
            snakeSegments[i].position = Vector2.Lerp(snakePositions[i], snakePositions[i - 1], distance / circleDiameter);
        }

    }

    private void CheckFoodTimer()
    {
        float currentTime = Time.time;

        List<CollisionDetector> forDelete = new List<CollisionDetector>();

        foreach (var detector in foodDetectors)
        {
            if (currentTime - detector.time >= foodRespawnTime)
            {
                detector.time = 0;
                detector.GetComponent<SpriteRenderer>().sprite = defaultFood;
                detector.GetComponent<LeanAnimation>().ScaleAnimation();

                forDelete.Add(detector);
            }
        }

        foreach (var deletingDetector in forDelete)
        {
            foodDetectors.Remove(deletingDetector);
        }
    }

    #endregion


    #region EVENTS
    private void PlayerController_OnCollision(CollisionDetector detector)
    {
        if (detector.collisionType == CollisionDetector.CollisionType.OBSTACLE)
        {
            isNoControl = true;
            OnDie();
        }
        else if (detector.collisionType == CollisionDetector.CollisionType.FOOD)
        {
            //check if we can eat this food
            if (foodDetectors.Contains(detector))
                return;
            foodDetectors.Add(detector); //add food to respawn system

            //add score       
            score += detector.points;
            detector.time = Time.time; //add food time to respawn
            OnScoreChange?.Invoke(score);

            //add new segment depending on points
            float timeToWait = 0.3f;
            for (int i = 0; i < detector.points; i++)
            {
                StartCoroutine(AddSegment(i*timeToWait));
            }
            
            //eat animation
            snakeSegments[0].GetComponent<LeanAnimation>().ScaleAnimationOneTime();

            //food dissapear animation
            detector.GetComponent<LeanAnimation>().DissapearAnimation(()=> { detector.GetComponent<SpriteRenderer>().sprite = gatheredFood; });
        }
        else if (detector.collisionType == CollisionDetector.CollisionType.TAIL)
        {
            isNoControl = true;
            OnDie();
        }
    }

    private void OnDie()
    {
        foreach (var segment in snakeSegments)
        {
            float x = Random.Range(-10, 10);
            float y = Random.Range(-10, 10);

            Vector2 forceDir = new Vector2(x, y);

            Rigidbody2D rb = segment.GetComponent<Rigidbody2D>();
            rb.constraints = new RigidbodyConstraints2D();
            rb.AddForce(forceDir, ForceMode2D.Impulse);
        }

        OnReset?.Invoke();
    } 

    IEnumerator AddSegment(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        Transform parent = snakeSegments[snakeSegments.Count -1];

        Transform newSegment = Instantiate(prefab_segment, parent.position, parent.rotation) as Transform;
        newSegment.SetParent(parent.parent);

        newSegment.name = "SEGAMENT_" + snakeSegments.Count;

        snakeSegments.Add(newSegment);
        snakePositions.Add(newSegment.position);

        //first two segment always collide with head, and we cant rotate to them
        if (snakeSegments.Count <= 3)
        {
            newSegment.GetComponent<CircleCollider2D>().enabled = false;
        }
    }

    #endregion

}
