using Godot;
using System;

namespace CardBattler.Scene;

public partial class TableTop : Node3D
{
    private Camera3D camera;
    private RigidBody3D selectedBody;
    private bool isDragging = false;
    private Plane dragPlane;
    private Vector3? dragTarget;

    // Define const
    private const float LiftOffset = 0.5f; // Adjust this value to control the lift height
    private const float dragStrength = 10.0f;
    private const float dragDampening = 0.8f;

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

    /*public override void _Process(double delta)
    {
        if (isDragging && selectedBody != null)
        {
            DragBody();
        }
    }*/

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

        dragTarget = intersectPoint;

        // Move the selected body to the intersected position (keep its current Z or apply custom logic)
        /*selectedBody.GlobalTransform = new Transform3D(
            selectedBody.GlobalTransform.Basis,
            //new Vector3(intersectPoint.Value.X, intersectPoint.Value.Y, selectedBody.GlobalTransform.Origin.Z)
            new Vector3(intersectPoint.Value.X, intersectPoint.Value.Y + LiftOffset, intersectPoint.Value.Z)
        );*/
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isDragging && selectedBody != null)
        {
            DragBody();
            ApplyDraggingForce((float)delta);
        }
    }

    private void ApplyDraggingForce(float delta)
    {
        if(selectedBody != null)
        {
            Vector3 currentPosition = selectedBody.GlobalTransform.Origin;

            if (dragTarget.HasValue)
            {
                Vector3 direction = (dragTarget.Value - currentPosition) * dragStrength;
                Vector3 newPosition = currentPosition.Lerp(dragTarget.Value, dragStrength * delta);
                newPosition = newPosition.Lerp(dragTarget.Value, dragDampening * delta);
                selectedBody.GlobalTransform = new Transform3D(selectedBody.GlobalTransform.Basis, newPosition);
            }
        }
    }
}
