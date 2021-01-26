using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class captain : MonoBehaviour
{
    // Piloting variables
    public float thrust = 1f; // magnitude of rocket acceleration
    public GameObject target; // captain pursues this ship
    public bool is_evader; // does this captain try to juke the chasing ship?
    public bool is_chaser; // does this captain chase the target ship?
    public bool is_predictor; // does this captain predict the movement of the target ship?
    public Material laser_beam_material;

    // Weapon variables
    public float α = 45f; // cylindrical angle of weapon targetting
    public float β = 5; // maximum range of laser

    private bool game_over = false;
    private Rigidbody ship;
    private Rigidbody target_ship;
    private Vector3 aim; // direction captain is piloting the ship
    private bool has_target; // is this captain an attacker?
    private bool target_in_angle;
    private bool target_in_range;
    private GameObject laser_beam;

    // Start is called before the first frame update
    void Start()
    {
        // Locate enemy ship
        has_target = target ?? false; // if target exists

        // Initiliaze ships
        ship = gameObject.GetComponent<Rigidbody>();
        if (has_target)
        {
            target_ship = target.GetComponent<Rigidbody>();
        }
        
        // Initial position and direction
        if (is_chaser)
        {
            // Put target random distance away
            target.transform.position = new Vector3(transform.position.x + Random.Range(10f, 20f), transform.position.y + Random.Range(10f, 20f), transform.position.y + Random.Range(10f, 20f));
        }
        else
        {
            // Random initial direction to run from chaser
            aim = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }

        // Initialize laser beam
        laser_beam = new GameObject("Laser beam");
        laser_beam.AddComponent<LineRenderer>();
        laser_beam.GetComponent<LineRenderer>().enabled = false;
    }

    void OnDestroy()
    {
        game_over = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (game_over == false)
        {
            fly_ship();
            if (has_target)
            {
                aim_laser();
            }
        }
    }

    private void fly_ship()
    {
        // Chasing target
        if (is_chaser)
        {
            // Calculate aim vector
            if (is_predictor)
            {
                // Aim to intercept target
                aim = target.transform.position - transform.position; // aim at current position
                // Quadratic formula
                var b = Vector3.Dot(aim, target_ship.velocity);
                var sqrt = Mathf.Sqrt(b * b - aim.sqrMagnitude * (target_ship.velocity.sqrMagnitude - ship.velocity.sqrMagnitude));
                var path_1 = (-b - sqrt) / aim.sqrMagnitude;
                var path_2 = (-b + sqrt) / aim.sqrMagnitude;

                // Pick interception path
                aim = path_1 > path_2 ? path_1 * aim + target.gameObject.GetComponent<Rigidbody>().velocity : path_2 * aim + target.gameObject.GetComponent<Rigidbody>().velocity;
            }
            else
            {
                // Aim at target current position
                aim = target.transform.position - transform.position;
            }
        }
        // Evading pursuer
        else if (is_evader)
        {
            // After 500 frames, start to evade
            if (Time.frameCount > 500 && Time.frameCount % 100 == 0)
            {
                aim = new Vector3(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)); // Move a random direction
            }
        }

        // Rotate to face forward
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(aim), 1);

        // Apply acceleration force to ship
        ship.AddForce(aim.normalized * thrust);
    }

    private void aim_laser()
    {
        // Check if target is within angle
        target_in_angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(transform.forward, target.transform.position - transform.position) / (transform.forward.magnitude * (target.transform.position - transform.position).magnitude)) < α ? true : false;

        // Check if target is within distance
        target_in_range = Vector3.Distance(target.transform.position, transform.position) < β ? true : false;

        // If aim is good, fire
        if (target_in_angle && target_in_range)
        {
            // Create laser beam
            laser_beam.GetComponent<LineRenderer>().enabled = true;
            laser_beam.transform.position = transform.position;
            var beam = laser_beam.GetComponent<LineRenderer>();
            beam.material = laser_beam_material;
            beam.startWidth = 0.5f;
            beam.endWidth = 0.1f;
            beam.SetPosition(0, transform.position);
            beam.SetPosition(1, target.transform.position);
            
            // Destroy targets
            Destroy(laser_beam, 0.2f);
            Destroy(target, 0.3f);
            
            // Stop game
            Time.timeScale = 0;
            game_over = true;
        }
    }
}
