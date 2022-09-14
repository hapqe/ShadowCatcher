using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PhysicsTest : MonoBehaviour
{
    struct Particle{
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 acceleration;
        public Vector2 force;
        public float mass;
        public float radius;
        public float inverseMass;
        public Color color;
        public Particle(Vector2 position, float mass, float radius, Color color){
            this.position = position;
            this.velocity = Vector2.zero;
            this.acceleration = Vector2.zero;
            this.force = Vector2.zero;
            this.mass = mass;
            this.radius = radius;
            this.inverseMass = 1 / mass;
            this.color = color;
        }
        public void Draw(){
            Gizmos.color = color;
            Gizmos.DrawSphere(position, radius);
        }
    }

    void Integrate(Particle p, float dt){
        p.acceleration = p.force * p.inverseMass;
        
        p.position += p.velocity * dt;
        p.velocity += p.acceleration * dt;

        p.force = Vector2.zero;
    }

    interface IForceGenerator{
        void ApplyForce(Particle p);
    }

    class ConstantForceGenerator : IForceGenerator{
        public Vector2 gravity;
        public ConstantForceGenerator(Vector2 gravity){
            this.gravity = gravity;
        }
        public void ApplyForce(Particle p){
            p.force += p.mass * gravity;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Particle p = new Particle(new Vector2(0, 0), 1, 1, Color.red);
    ConstantForceGenerator gravity = new ConstantForceGenerator(new Vector2(0, -9.8f));

    void OnDrawGizmos()
    {
        p.Draw();
    }

    private void Update()
    {
        gravity.ApplyForce(p);
        Integrate(p, Time.deltaTime);
    }
}
