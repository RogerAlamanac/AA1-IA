using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public Transform npc;
    public Transform target;
    public Vector2 areaSize = new Vector2(8, 5); // àrea on es mourà el target

    private SteeringAgent agent;
    private CompositeSteering composite;

    void Start()
    {
        agent = npc.GetComponent<SteeringAgent>();

        // Afegim Seek i Arrive
        composite = new CompositeSteering();
        composite.AddBehavior(new Seek(target), 0.5f);
        composite.AddBehavior(new Arrive(target, 2f), 1f);

        agent.SetBehavior(composite);
    }

    void Update()
    {
        float distance = Vector2.Distance(npc.position, target.position);

        if (distance < 0.3f)
        {
            // Canviar target a posició aleatòria dins del rectangle
            Vector2 randomPos = new Vector2(
                Random.Range(-areaSize.x, areaSize.x),
                Random.Range(-areaSize.y, areaSize.y)
            );

            target.position = randomPos;
        }
    }
}
