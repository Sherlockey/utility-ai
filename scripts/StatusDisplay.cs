using Godot;

using System;

public partial class StatusDisplay : Control
{
    [Export]
    private Label _nameLabel;
    [Export]
    private TextureRect _portrait;
    [Export]
    private Label _healthLabel;
    [Export]
    private Label _attackLabel;
    [Export]
    private Label _speedLabel;
    [Export]
    private Label _movementLabel;

    public void Update(StringName name, Texture2D portraitTexture,
        string currentHealth, string maxHealth, string attack, string speed, string movement)
    {
        _nameLabel.Text = name;
        _portrait.Texture = portraitTexture;
        _healthLabel.Text = currentHealth + "/" + maxHealth;
        _attackLabel.Text = attack;
        _speedLabel.Text = speed;
        _movementLabel.Text = movement;
    }
}
