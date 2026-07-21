using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "0b6beaac3bdd676b2b5784ede4a85e9ccb50aea2")]
public class Gun : Component
{
	private bool grabed = false;

	public float ShootingDistance = 5;
	public float ShootingReloadTime = 0.5f;
	public float Damage = 20.0f;

	private float timer_reload = 0;

	void Init()
	{
		
	}

	public void Shoot()
	{
		if (timer_reload <= 0)
		{
			WorldIntersection worldIntersection = new WorldIntersection();
			Object o = World.GetIntersection(node.WorldPosition, node.WorldPosition + node.GetWorldDirection(MathLib.AXIS.Y) * ShootingDistance, 5, worldIntersection);
			if (o != null)
			{
				Enemy enemy = GetComponentInParent<Enemy>(o);
				if (enemy != null)
				{
					enemy.Damage(Damage);
				}
                Visualizer.RenderLine3D(node.WorldPosition, worldIntersection.Point, vec4.RED, 0.05f);
            }
			else
			{
				Visualizer.RenderLine3D(node.WorldPosition, node.WorldPosition + node.GetWorldDirection(MathLib.AXIS.Y) * ShootingDistance, vec4.RED, 0.05f);
			}

			timer_reload = ShootingReloadTime;
		}
		
	}
	public void Grab()
	{ 
		grabed = true;
	}

	public void Throw()
	{

		grabed = false;
	}
	
	void Update()
	{
		if (!grabed)
		{
			// hold animation
			node.Rotate(0,0, 180 * Game.IFps);
		}

		if (timer_reload > 0)
		{
			timer_reload -= Game.IFps;
		}


	}
}