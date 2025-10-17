using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public Transform npc;
    public Transform player;
    private Vector2 areaSize = new Vector2(10, 5); // àrea on es mourà el target

    private SteeringAgent agent;
    private CompositeSteering composite;
    public float speed = 0.0f;

    void Start()
    {
        agent = npc.GetComponent<SteeringAgent>();

        // Afegim Seek i Arrive
        composite = new CompositeSteering();
        composite.AddBehavior(new Seek(player), 0.5f);
        composite.AddBehavior(new Arrive(player, 2f), 1f);

        agent.SetBehavior(composite);
    }

    void Update()
    {
        float distance = Vector2.Distance(npc.position, player.position);
        float horizontal = Input.GetAxis("Horizontal");
        float vertical  = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontal,0, vertical);
        player.position += movementDirection * speed * Time.deltaTime;
           
        if (distance < 0.3f)
        {
            
        }
    }
}
