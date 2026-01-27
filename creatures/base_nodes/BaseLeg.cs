using Godot;
using System;

public partial class BaseLeg : Skeleton2D
{
    RayCast2D WallChecker;
    Node2D Target;
    Node2D ResetPosition;
    Node2D BackupGrabPos;

    [Export] int WallCheckerRotationIndex = 45;
    [Export] int StartWallCheckerRotation = 45;

    Vector2 GrabbedBodyLocation;
    bool FirstStep = false;
    bool isTweening = false;

    public enum BaseLegState
    {
        Search,
        Grab,
        Reset
    }

    BaseLegState CurrentState = BaseLegState.Search;

    public override void _Ready()
    {
        Target = GetNode<Node2D>("Target");
        ResetPosition = GetNode<Node2D>("ResetPosition");
        BackupGrabPos = GetNode<Node2D>("BackupGrabPos");
        WallChecker = GetNode<RayCast2D>("WallChecker");
    }

    public override void _Process(double delta)
    {
        switch (CurrentState)
        {
            case BaseLegState.Search:
                Search();
                break;

            case BaseLegState.Grab:
                Grab();
                break;

            case BaseLegState.Reset:
                ResetLeg();
                break;
        }
    }

    void Search()
    {
        WallChecker.Rotation = Mathf.DegToRad(
            FirstStep ? WallCheckerRotationIndex : StartWallCheckerRotation
        );

        if (WallChecker.IsColliding())
        {
            GrabbedBodyLocation = WallChecker.GetCollisionPoint();
            FirstStep = true;
            CurrentState = BaseLegState.Grab;
        }
    }

    void Grab()
    {
        if (isTweening) return;

        isTweening = true;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(
            Target,
            "global_position",
            GrabbedBodyLocation,
            0.1f
        );

        tween.TweenCallback(Callable.From(() =>
        {
            isTweening = false;
        }));

        WallChecker.GlobalRotation =
            (GrabbedBodyLocation - GlobalPosition).Angle();

        if (!WallChecker.IsColliding())
        {
            CurrentState = BaseLegState.Reset;
        }
    }

    void ResetLeg()
    {
        if (isTweening) return;

        isTweening = true;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(
            Target,
            "global_position",
            ResetPosition.GlobalPosition,
            0.1f
        );

        tween.TweenCallback(Callable.From(() =>
        {
            isTweening = false;
            CurrentState = BaseLegState.Search;
        }));
    }

    public void ForceReset()
    {
        CurrentState = BaseLegState.Reset;
        isTweening = false;
    }
}
