using Godot;

using System;

public partial class EmbarkDisplay : PanelContainer
{
    public event EventHandler Closed;

    [Export]
    private PackedScene _levelOneScene;
    [Export]
    private PackedScene _levelTwoScene;
    [Export]
    private PackedScene _levelThreeScene;
    [Export]
    private PackedScene _levelFourScene;
    [Export]
    private Button _levelOneButton;
    [Export]
    private Button _levelTwoButton;
    [Export]
    private Button _levelThreeButton;
    [Export]
    private Button _levelFourButton;
    [Export]
    private Button _backButton;

    public override void _Ready()
    {
        _levelOneButton.Pressed += OnLevelOneButtonPressed;
        _levelTwoButton.Pressed += OnLevelTwoButtonPressed;
        _levelThreeButton.Pressed += OnLevelThreeButtonPressed;
        _levelFourButton.Pressed += OnLevelFourButtonPressed;
        _backButton.Pressed += OnBackButtonPressed;
    }

    private void OnLevelOneButtonPressed()
    {
        ChangeSceneToLevel(_levelOneScene);
    }

    private void OnLevelTwoButtonPressed()
    {
        ChangeSceneToLevel(_levelTwoScene);
    }

    private void OnLevelThreeButtonPressed()
    {
        ChangeSceneToLevel(_levelThreeScene);
    }

    private void OnLevelFourButtonPressed()
    {
        ChangeSceneToLevel(_levelFourScene);
    }

    private static void ChangeSceneToLevel(PackedScene levelScene)
    {
        Game.Instance.ChangeSceneToLevel(levelScene);
    }

    private void OnBackButtonPressed()
    {
        Visible = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
