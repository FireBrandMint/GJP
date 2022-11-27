# GJP

Deterministic intersection engine for c#, it does the complicated work for you like calculations but leaves things like how you separate two objects when they collide up to you.

Not very robust, but not especially confusing either... at least i think so.

Weird commit messages may come as i update this, but remember: it's the only way i can stay motivated, so it's for the worst but it's the only way.

# Guide

- Use the static constructors of the shapes like ConvexPolygon.CreateRect and CircleShape.CreateCircle to create Shape instances.

- Create a simulation with DtCollisionSimulation.

- Set the simulation for the Shape with the SetSimulation method found in it.

- Execute the Tick method of the simulation when the game updates.

- After the simulation is set on the shape, use Activate for it to start existing in the simulation or Deactivate to be in stasis, see if it's already existing or not with IsActive.

- Use the Detecting bool of the shape instance after you already set the simulation to make the shape start actually detecting stuff. (not necessary with shapes that just need to be detected like terrain and moving plataforms)

- Use the IntersectOnly bool to determine wether or not a object is NOT solid and DOES NOT solve overlap.

- Keep in mind that the ObjectUsingIt reference on the shape instance is what determines the response to all collision based on the information given to it. The methods found in the interface CollisionAntenna wich the ObjectUsingIt inherits will be signaled when the shape detects something or is detected by something, so write code to handle that.

- Finally, when disposing of a Shape instance, always use the Dispose method found in it.

# Lacking features

- Object pooling, since computers hates deleting memory. (almost not lacking)

- Segmented motion for people that want to make fast objects.

- Raycast.
