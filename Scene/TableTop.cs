using Godot;
using System;

namespace CardBattler.Scene;

public partial class TableTop : Node3D
{
    private Camera3D camera;
    private RigidBody3D selectedBody;
    private bool isDragging = false;
    private Plane dragPlane;

    // Define the lift offset
    private const float LiftOffset = 0.5f; // Adjust this value to control the lift height

    public override void _Ready()
    {
        // Get the reference to the main camera
        camera = GetNode<Camera3D>("Camera3D");  // Ensure the camera node is correctly referenced
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    // Start dragging if the mouse is pressed
                    StartDragging();
                }
                else
                {
                    // Stop dragging when the mouse button is released
                    StopDragging();
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        if (isDragging && selectedBody != null)
        {
            DragBody();
        }
    }

    private void StartDragging()
    {

        // Perform a raycast from the camera to the mouse position to check for RigidBody3D
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
        Vector3 rayDirection = camera.ProjectRayNormal(mousePos);
        var spaceState = GetWorld3D().DirectSpaceState;

        // Set up a dictionary for the IntersectRay parameters
        var query = new PhysicsRayQueryParameters3D
        {
            From = rayOrigin,
            To = rayOrigin + rayDirection * 1000
        };

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        //{ && result["collider"] is RigidBody3D body)
        {
            var collider = result["collider"].As<RigidBody3D>();
            if(collider != null) { 
                selectedBody = collider;
                isDragging = true;

                // Set the drag plane at the position of the selected body
                Vector3 hitPoint = (Vector3)result["position"];
                dragPlane = new Plane(Vector3.Up, hitPoint);
            } 
        }
    }

    private void StopDragging()
    {
        selectedBody = null;
        isDragging = false;
    }

    private void DragBody()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
        Vector3 rayDirection = camera.ProjectRayNormal(mousePos);

        // Calculate the intersection point between the ray and the drag plane
        Vector3? intersectPoint = dragPlane.IntersectsRay(rayOrigin, rayDirection);

        // Move the selected body to the intersected position (keep its current Z or apply custom logic)
        selectedBody.GlobalTransform = new Transform3D(
            selectedBody.GlobalTransform.Basis,
            //new Vector3(intersectPoint.Value.X, intersectPoint.Value.Y, selectedBody.GlobalTransform.Origin.Z)
            new Vector3(intersectPoint.Value.X, intersectPoint.Value.Y + LiftOffset, intersectPoint.Value.Z)

        );
    }

}
