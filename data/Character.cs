using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using Unigine;

[Component(PropertyGuid = "140eb41297d86e735315f33dfb3213a6e37b387b")]
public class Character : Component
{
    public float MaxSpeed = 4;
    public float MaxAngularSpeed = 5;
	public float GrabRadius = 0.5f;

	public float Health = 100;

    public bool IsWorldRotate = true;

	public float ScreenBorderSize = 0.3f;

	public Player player;

	public Node gun_slot;

	private Gun current_gun;
	private WidgetLabel label;

	private float damage_time = 0.1f;
	private float damage_timer = 0;

	void Init()
	{
		Visualizer.Enabled = true;

		Input.MouseHandle = Input.MOUSE_HANDLE.USER;
		node.ObjectBodyRigid.MaxLinearVelocity = MaxSpeed;
		node.ObjectBodyRigid.MaxAngularVelocity = MaxAngularSpeed;
		node.ObjectBodyRigid.AngularScale = vec3.UP;

		label = new WidgetLabel();
		Gui.GetCurrent().AddChild(label);
		label.FontSize = 20;
		label.FontOutline = 1;
		updateLabel();

    }

	void updateLabel()
	{
		label.Text = Health > 0 ? $"Heath {Health}" : "YOU LOSE!";
	}

	public void Damage(float damage)
	{
		Health -= damage;
		updateLabel();

        if (Health <= 0)
		{
			Game.Enabled = false;
			Render.VignetteMask = true;
		}

		damage_timer = damage_time;

	}
	
	void Update()
	{
		if (damage_timer > 0)
		{
			damage_timer -= Game.IFps;
			Render.FadeColor = new vec4(1, 0, 0, (damage_timer / damage_time) * 0.3);
		}

		if (current_gun)
		{
			if (Input.IsMouseButtonPressed(Input.MOUSE_BUTTON.LEFT))
				current_gun.Shoot();
			if (Input.IsKeyDown(Input.KEY.F))
			{ 
				current_gun.Throw();
				current_gun.node.Translate(vec3.FORWARD);
				current_gun.node.SetWorldParent(null);
				current_gun = null;
			}
		}
		else
		{
            List<Node> nodes = new List<Node>();
            World.GetIntersection(new WorldBoundSphere(node.WorldPosition, GrabRadius), nodes);
            foreach (Node n in nodes)
            {
                Gun gun = GetComponent<Gun>(n);
                if (gun != null)
                {
                    gun.Grab();
                    current_gun = gun;
					gun.node.Parent = node;
					gun.node.WorldTransform = gun_slot.WorldTransform;
                    break;
                }
            }
        }

		if (Health <= 0 && Input.IsKeyDown(Input.KEY.ENTER))
		{
			World.ReloadWorld();
		}
	}

	void PostUpdate()
	{
		ivec2 size = WindowManager.MainWindow.ClientSize;
		player.GetScreenPosition(out int x, out int y, node.WorldPosition, size.x, size.y);
		vec2 relative_size = new vec2 (x, y) / size;

		int ws = 0;
		int da = 0;

        ws = ((relative_size.y < ScreenBorderSize) ? 1 : 0) - ((relative_size.y > (1 - ScreenBorderSize)) ? 1 : 0);
        da = ((relative_size.x < ScreenBorderSize) ? 1 : 0) - ((relative_size.x > (1 - ScreenBorderSize)) ? 1 : 0);

		player.WorldTranslate(new vec3(-da, ws, 0) * Game.IFps * node.BodyLinearVelocity.Length);
    }
	
	void UpdatePhysics()
	{
		int ws =  (Input.IsKeyPressed(Input.KEY.W) ? 1 : 0) - (Input.IsKeyPressed(Input.KEY.S) ? 1 : 0);
		int da = (Input.IsKeyPressed(Input.KEY.D) ? 1 : 0) - (Input.IsKeyPressed(Input.KEY.A) ? 1 : 0);

		vec3 dir = IsWorldRotate ? new vec3(da, ws, 0) : new vec3(node.WorldTransform.GetRotate() * new vec3(da, ws, 0));
        node.ObjectBodyRigid.AddLinearImpulse(dir);


		ivec2 mouse_pos = Input.MousePosition;
		vec3 dir_from_camera = player.GetDirectionFromMainWindow(mouse_pos.x, mouse_pos.y);
		WorldIntersection intersection = new WorldIntersection();
		if (World.GetIntersection(player.WorldPosition, player.WorldPosition + dir_from_camera * 100, 255, intersection))
		{
			Visualizer.RenderPoint3D(intersection.Point, 0.1f, vec4.RED, false, 0.05f);
			vec3 dir_player_to_mouse = new vec3(intersection.Point - node.WorldPosition);
			vec3 forward = node.GetWorldDirection(MathLib.AXIS.Y);
			float angle = MathLib.Angle(forward, dir_player_to_mouse, vec3.UP);
			angle = MathLib.Clamp(angle, -90.0f, 90.0f) / 90.0f;
			node.ObjectBodyRigid.AddAngularImpulse(new vec3(0, 0, angle));
		}



   
	}
	
	void Shutdown()
	{
        label.DeleteLater(); 

    }
}