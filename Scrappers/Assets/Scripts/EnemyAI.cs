using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (Seeker))]
public class EnemyAI : MonoBehaviour {

    //stay on target
    public Transform target;

    //times per second path updates
    public float updateRate = 2f;

    //caching
    private Seeker seeker;
    private Rigidbody2D rb;

    //Calculated path
    public Path path;

    //The AI's speed per second
    public float speed = 300f;
    public ForceMode2D fMode;

    [HideInInspector]
    public bool pathIsEnded = false;

    //max distance from point before AI find next
    public float nextWaypointDistance = 3;

    // The waypoint we are currently moving towards
    private int currentWaypoint = 0;
    private bool facingRight = false;

    private bool searchingForPlayer = false;

    void Start () {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        if (target == null){
            if (!searchingForPlayer){
                StartCoroutine(SearchForPlayer());
            }
            return;
        }
        // Start a new path to taget, return the result to the OnPathComplete method
        seeker.StartPath(transform.position, target.position, OnPathComplete);
        StartCoroutine(UpdatePath());

    }

    IEnumerator SearchForPlayer()
    {
        GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
        if (searchResult == null){
            yield return new WaitForSeconds(updateRate);
            StartCoroutine(SearchForPlayer());
        }else{
            searchingForPlayer = false;
            target = searchResult.transform;
            StartCoroutine(UpdatePath());
            yield break;
        }
    }

    IEnumerator UpdatePath()
    {
        if (target == null)
        {
            if (!searchingForPlayer)
            {
                StartCoroutine(SearchForPlayer());
            }
            yield break;
        }
        seeker.StartPath(transform.position, target.position, OnPathComplete);

        yield return new WaitForSeconds(1f / updateRate);
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path p){
        if(p.error){
            Debug.Log("path error: " + p.error);
        }else{
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            if (!searchingForPlayer)
            {
                StartCoroutine(SearchForPlayer());
            }
            return;
        }
        float targetInd = target.position.x - transform.position.x;
        if (targetInd > 0 && !facingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (targetInd < 0 && facingRight)
        {
            // ... flip the player.
            Flip();
        }
        if (path == null)
            return;
        if (currentWaypoint >= path.vectorPath.Count){
            if (pathIsEnded)
                return;

            pathIsEnded = true;
            return;
        }
        pathIsEnded = false;

        //Direction to the next waypoint

        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;

        //move the ai
        rb.AddForce(dir, fMode);

        float dist = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
        if (dist < nextWaypointDistance){
            currentWaypoint++;
            return;
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}