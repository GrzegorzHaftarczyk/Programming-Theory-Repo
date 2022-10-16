using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{    

    public GameObject prefabBody;
    public GameObject prefabFruit;
    public GameObject environment;
    public float boardLeft, boardRight, boardUp, boardDown;
    float speed = 0.4F;
    enum Direction { LEFT, RIGHT, TOP, BOTTOM }
    Direction direction = Direction.LEFT;
    Vector3Int[] directionVector = {new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0)};
    
    private float horizontalLength, vertivalLength;
    enum State {
        MOVE,
        EAT,
        END
    }
    State gameState = State.MOVE;
    void Start()
    {
        horizontalLength = (int)((boardRight - boardLeft)/speed);
        vertivalLength = (int)((boardUp - boardDown)/speed);
        addRandomFruit();
        StartCoroutine(move());
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        if(horizontalInput > 0.1) {
            direction = Direction.RIGHT;
        } else if (horizontalInput < -0.1) {
            direction = Direction.LEFT;
        }
        if(verticalInput > 0.1) {
            direction = Direction.TOP;
        } else if (verticalInput < -0.1) {
            direction = Direction.BOTTOM;
        }
    }

    IEnumerator move() 
    {
        while(gameState != State.END)
        {
            yield return new WaitForSeconds(1f);
            State prevState = gameState;
            Vector3 currentDirection = directionVector[(int) direction];
            Vector3 jump = new Vector3(currentDirection.x * speed, currentDirection.y * speed, 0);
            Vector3 current = transform.GetChild(0).position;
            RaycastHit hitInfo;
            if (prevState == State.MOVE) 
            {
                transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
            }
            if (Physics.Raycast(current, currentDirection, out hitInfo, speed))
            {
                gameState = collideWith(hitInfo.collider);
            } else {
                gameState = State.MOVE;
            }
            if(gameState == State.END)
            {
                break;
            }
            Vector3 newPosition = current + jump;
            transform.GetChild(0).position = transform.GetChild(0).position + jump;
            Vector3 nextPosition = current;
            if(prevState == State.EAT)
            {
                GameObject second = Instantiate(prefabBody, nextPosition, Quaternion.identity);
                second.name = "Body";
                second.transform.parent = transform;
                second.transform.SetSiblingIndex(1);
            }
            if (prevState == State.MOVE)
            {
                for (int i = 1; i < transform.childCount - 1; i++)
                {
                    current = transform.GetChild(i).position;
                    transform.GetChild(i).position = nextPosition;
                    nextPosition = current;
                }
                transform.GetChild(transform.childCount - 1).position = nextPosition;
                transform.GetChild(transform.childCount - 1).gameObject.SetActive(true);
            }
        }
        // TODO handle end of a game
    }

    State collideWith(Collider coolider)
    {
        Debug.Log("collide: "+coolider.tag);
        if(coolider.tag == "Obstacle") {
           Debug.Log("End of a game");
           return State.END;
        } else if (coolider.tag == "Food") {
           Debug.Log("Yummy!!!");
           Destroy(coolider.gameObject);
           addRandomFruit();
           return State.EAT;
        } else {
           Debug.Log("Error!!!"); 
        }
        return State.MOVE;
    }

    private void addRandomFruit() {
        Vector3 randomPlaceOnBoard = getRandomVectorOnBoard();
        while(Physics.OverlapBox(randomPlaceOnBoard, new Vector3(0.075f, 0.075f, 0.075f)).Length > 1)
        {
            randomPlaceOnBoard = getRandomVectorOnBoard();
        }
        addFruit(randomPlaceOnBoard);
    }

    private void addFruit(Vector3 randomPlaceOnBoard) {
        GameObject fruit = Instantiate(prefabFruit, randomPlaceOnBoard, Quaternion.identity);
        fruit.transform.parent = environment.transform;
    }

    private Vector3 getRandomVectorOnBoard()
    {
        return new Vector3(Random.Range(0, horizontalLength) * speed - boardRight, Random.Range(0, vertivalLength) * speed - boardUp, 0);
    }


}
