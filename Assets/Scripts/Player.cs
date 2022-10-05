using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{    

    public GameObject prefabBody;
    float speed = 0.4F;
    enum Direction { LEFT, RIGHT, TOP, BOTTOM }
    Direction direction = Direction.LEFT;
    Vector3Int[] directionVector = {new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0)};
    // Start is called before the first frame update
    enum State {
        MOVE,
        EAT,
        END
    }
    State gameState = State.MOVE;
    void Start()
    {
        // TODO think about colors: Head - blue, Body - green, Tail - red?
        int children = transform.childCount;
        print("count: "+children);
        for (int i = 0; i < children; ++i)
        {
            print("For loop: " + transform.GetChild(i).gameObject.name);
        }
        StartCoroutine(move()); 
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        //Get the value of the Horizontal input axis.

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
            // Debug.DrawRay(current, current + jump * speed / 2, Color.green, 1, false);
            if (prevState == State.MOVE) 
            {
                transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
            }
            Debug.DrawRay(current, currentDirection * speed, Color.green, 1, false);
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
            // If end the game, then end the game
            // If obstacle body, then end the game
            // If tail and privies move was eat, then end game
            // If tail and privies move was move, then regular move
            // move after eat: move all (without tail), then add last body part (tail stay in the same place, use Transform.SetAsLastSibling on a tail)
            // regular move: move all, with tail
            if(prevState == State.EAT)
            {
                // TODO put new body after head
                // rest of body stay in the same place
                GameObject second = Instantiate(prefabBody, nextPosition, Quaternion.identity);
                second.name = "Body";
                second.transform.parent = transform;
                second.transform.SetSiblingIndex(1);
            }
            if (prevState == State.MOVE)
            {
                Debug.Log("move body");
                for (int i = 1; i < transform.childCount - 1; i++)
                {
                    current = transform.GetChild(i).position;
                    transform.GetChild(i).position = nextPosition;
                    nextPosition = current;
                }
                Debug.Log("move rest of the tail");
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
           return State.EAT;
        } else {
           Debug.Log("Error!!!"); 
        }
        return State.MOVE;
    }

}
