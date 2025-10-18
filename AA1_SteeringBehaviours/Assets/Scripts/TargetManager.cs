using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public List<Transform> npcs;
    public Transform player;
    //private Vector2 areaSize = new Vector2(10, 5); // àrea on es mourà el target
    private List<SteeringAgent> agents = new List<SteeringAgent>();
    private CompositeSteering composite;
    public float speed = 0.0f;

    void Start()
    {
        // Configurar tots els agents
        foreach (Transform npc in npcs)
        {
            SteeringAgent agent = npc.GetComponent<SteeringAgent>();
            if (agent == null) continue;

            CompositeSteering composite = new CompositeSteering();
            composite.AddBehavior(new Seek(player), 0.4f);
            composite.AddBehavior(new Arrive(player, 2f), 1f);

            agent.SetBehavior(composite);
            agents.Add(agent);
        }
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical  = Input.GetAxis("Vertical");
        //float distance = Vector2.Distance(npc.position, player.position);
        Vector3 movementDirection = new Vector3(horizontal, vertical, 0);
        player.position += movementDirection * speed * Time.deltaTime;
    }
}
