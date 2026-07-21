using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "2911cd4487f4488f884d03a74831c27228d721d5")]
public class Enemy : Component
{
	public float Health = 100;
	public Obstacle SelfObstacle = null;
	public float ObstacleRadius = 0.5f;

    public float MaxSpeed = 4;
    public float MaxAngularSpeed = 180;

    public float AttackDamage = 20;
    public float AttackTime = 1.0f;
	private float attack_timer = 0;

    private Character character;

	private PathRoute route = new PathRoute();
	private dvec3 target_point = new dvec3();
	private bool target_found = false;
	private bool active = false;

	void Init()
	{
		character = FindComponentInWorld<Character>();

		route.AddExcludeObstacle(SelfObstacle);
		route.Radius = ObstacleRadius;

		createPath();
	}

	private void createPath()
	{
		route.Create2D(node.WorldPosition, character.node.WorldPosition);
	}

	public void Damage(float damage)
	{
		Health -= damage;
		if (Health <= 0)
		{
			node.DeleteLater();
		}
	}
	
	void Update()
	{
        if (!active)
        {
			active = (node as Object).IsVisibleCamera;
			attack_timer = 0;
			return;
        }

		if (attack_timer >= 0)
		{
			attack_timer -= Game.IFps;
		}

        if (route.IsReady)
		{
			route.RenderVisualizer(vec4.GREEN);
			if (route.NumPoints > 1)
			{
				target_found = true;
                Visualizer.RenderPoint3D(route.GetPoint(1), 0.2f, vec4.RED);
                target_point = route.GetPoint(1);
				target_point.z = node.WorldPosition.z;
			}
			else
			{
				Visualizer.RenderPoint3D(route.GetPoint(0), 0.2f, vec4.RED);
				target_found = false;
			}
			createPath();
		}
		else if (!route.IsQueued)
		{
			target_found = false;
			createPath();
		}


		if (target_found)
		{
			quat target_rot = MathLib.SetTo(node.WorldPosition, target_point, vec3.UP, MathLib.AXIS.Y).GetRotate();
			quat current_rot = node.GetWorldRotation();
			current_rot = MathLib.RotateTowards(current_rot, target_rot, MaxAngularSpeed * Game.IFps);
			node.SetWorldRotation(current_rot);

            vec3 to_target = new vec3(target_point - node.WorldPosition);
            to_target.Normalize();
			if (MathLib.Distance(node.WorldPosition, character.node.WorldPosition) > ObstacleRadius * 2)
	            node.WorldTranslate(to_target * MaxSpeed * Game.IFps);
        }

		if (attack_timer <= 0)
		{ 
			List<Node> nodes = new List<Node>();
			World.GetIntersection(new WorldBoundSphere(node.WorldPosition, ObstacleRadius * 2), nodes);
			foreach (Node n in nodes)
			{
				Character p = GetComponentInParent<Character>(n);
				if (p)
				{
					p.Damage(AttackDamage);
					attack_timer = AttackTime;
				}
			}
		}

    }
}